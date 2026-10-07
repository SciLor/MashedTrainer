using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Binarysharp.Assemblers.Fasm;
using System.IO;
using Binarysharp.MemoryManagement;
using Binarysharp.MemoryManagement.Memory;
using Binarysharp.MemoryManagement.Native;
using Binarysharp.MemoryManagement.Assembly.CallingConvention;
using static SciLors_Mashed_Trainer.Types.Player;
using static SciLors_Mashed_Trainer.Types.Weapons.Weapon;
using SciLors_Mashed_Trainer.Types.Weapons;
using SciLors_Mashed_Trainer.Types.Settings.Game;
using SciLors_Mashed_Trainer.Types.Settings.Player;

namespace SciLors_Mashed_Trainer.Types {
    public class Game : BaseMemorySharp, IDisposable {
        private IntPtr PLAYER_COUNT = new IntPtr(0x8D8B30 - PROCESS_BASE);
        private IntPtr MAXIMUM_POINTS = new IntPtr(0x658DE4 - PROCESS_BASE); //0x659338 is the same value for another mode, written once at 0x41FC10
        private IntPtr GAME_ACTIVE = new IntPtr(0x6AE110 - PROCESS_BASE); //race audio streams created (0x46F5C0) / destroyed; also zero on pause

        private RemoteAllocation funcChangeWeapon;
        private RemoteAllocation funcDropWeapon;

        //The game shares these float constants between dozens of unrelated places (HUD, AI, physics), so instead of
        //changing the constant itself we redirect only the instruction operands that belong to the feature.
        private List<PatchedFloat> patches = new List<PatchedFloat>();
        private PatchedFloat maxDistance;
        private PatchedFloat warningDistance;
        private PatchedFloat maxDamage;
        private PatchedFloat cameraTilt;
        private PatchedFloat cameraHeightDivider;
        private PatchedFloat cameraHeightAdd;
        private PatchedFloat cameraHeightFactor;
        private PatchedFloat cameraZoomLimit;

        private PatchedFloat Patch(int originalAddress, int[] pointerSites, int[] immediateSites = null) {
            PatchedFloat patch = new PatchedFloat(Process, originalAddress, pointerSites, immediateSites);
            patches.Add(patch);
            return patch;
        }

        public List<Player> Players = new List<Player>();

        public bool IsRunning {
            get { return Process.IsRunning; }
        }

        public GameFiles GameFiles {
            get; set;
        }

        public GameSettings Settings {
            get; set;
        }

        private int playerCount;
        public int PlayerCount {
            get {
                return playerCount;
            }
        }

        private float maximumDistance;
        public float MaximumDistance {
            get { return maximumDistance; }
            set {
                maxDistance.Value = value;
                maximumDistance = value;
            }
        }
        private float distanceWarningThreshold;
        public float DistanceWarningThreshold {
            get { return distanceWarningThreshold; }
            set {
                warningDistance.Value = value;
                distanceWarningThreshold = value;
            }
        }

        private bool isActive;
        public bool IsActive {
            get { return isActive; }
        }

        private int maximumPoints;
        public int MaximumPoints {
            get { return maximumPoints; }
        }

        private float maximumDamage;
        public float MaximumDamage {
            get { return maximumDamage; }
            set {
                maxDamage.Value = value;
            }
        }

        private float cameraTiltMultiplicator;
        public float CameraTiltMultiplicator {
            get { return cameraTiltMultiplicator; }
            set {
                cameraTilt.Value = value;
            }
        }

        private float cameraHeightDistanceDivider;
        public float CameraHeightDistanceDivider {
            get { return cameraHeightDistanceDivider; }
            set {
                cameraHeightDivider.Value = value;
            }
        }
        private float cameraHeightDistanceAdd;
        public float CameraHeightDistanceAdd {
            get { return cameraHeightDistanceAdd; }
            set {
                cameraHeightAdd.Value = value;
            }
        }
        private float cameraHeightDistanceFactor;
        public float CameraHeightDistanceFactor {
            get { return cameraHeightDistanceFactor; }
            set {
                cameraHeightFactor.Value = value;
            }
        }
        private float cameraZoomLimitValue;
        public float CameraZoomLimit {
            get { return cameraZoomLimitValue; }
            set {
                cameraZoomLimit.Value = value;
            }
        }

        public WeaponHelper WeaponHelper;

        public Game(Process process) : base(process) {
            readAndInjectAsmFunctions();
            this.Settings = new GameSettings();
            this.WeaponHelper = new WeaponHelper(this);
            this.GameFiles = new GameFiles(this);
        }
        
        private void readAndInjectAsmFunctions() {
            byte[] asmBytes = File.ReadAllBytes("Asm\\ChangeWeapon.bin");
            funcChangeWeapon = Process.Memory.Allocate(asmBytes.Length);
            funcChangeWeapon.Write<byte>(asmBytes);

            asmBytes = File.ReadAllBytes("Asm\\DropWeapon.bin");
            funcDropWeapon = Process.Memory.Allocate(asmBytes.Length);
            funcDropWeapon.Write<byte>(asmBytes);

            //fcomp operand in FUN_004131d0 (equality test against 0x8C7E00, see doc/TrainerAnalysis.md)
            maxDistance = Patch(0x5DD620, new[] { 0x41340D });
            //fcomp operand in FUN_0044c140 (distance warning); the global 7.0 is also used by the HUD layout
            warningDistance = Patch(0x5DDB14, new[] { 0x44C16B });
            //fcomp operand in FUN_00423fe0 + the 50.0 it stores when the limit is exceeded
            maxDamage = Patch(0x5DE290, new[] { 0x423FEC }, new[] { 0x423FFA });
            //camera function FUN_0044fa30: pitch = tilt * zoom / zoomLimit - 5
            cameraTilt = Patch(0x5DE290, new[] { 0x450AEC });
            //camera distance = zoom / factor / divider + add
            cameraHeightFactor = Patch(0x5DDB18, new[] { 0x450BBA });
            cameraHeightDivider = Patch(0x5DD41C, new[] { 0x450BC0 });
            cameraHeightAdd = Patch(0x5DD330, new[] { 0x450BC6 });
            //zoom is clamped to 10.0 (compares + immediates) and normalised by 10.0 in FUN_0044fa30
            cameraZoomLimit = Patch(0x5DD620, CAMERA_ZOOM_LIMIT_OPERANDS, CAMERA_ZOOM_LIMIT_IMMEDIATES);
        }

        private static readonly int[] CAMERA_ZOOM_LIMIT_OPERANDS = {
            0x45081A, 0x450956, 0x45099F, 0x450AF8, 0x450B1A, 0x450C69, 0x450C7B, 0x450C93, 0x450CBB,
            0x451013, 0x45101F, 0x45102B, 0x451037, 0x451043, 0x45104F, 0x4511F9, 0x451205, 0x451211,
            0x45121D, 0x451229, 0x451235, 0x4512B8, 0x4512FC, 0x45141E, 0x451468
        };
        private static readonly int[] CAMERA_ZOOM_LIMIT_IMMEDIATES = { 0x45082B, 0x450967, 0x45097A, 0x4509B0 };

        private void ExecuteExtraFeatures() {
            if (!IsActive)
                return;

            DriveOverRevive();
            RandomWeaponEquip();
            ChangeWeaponBoxes();

            foreach (Player player in Players) {
                FreezePoints(player);
                FreezePlayer(player);
            }
        }

        private void DriveOverRevive() {
            DriveOverReviveSettings dos = Settings.DriveOverReviveSettings;
            if (!dos.IsEnabled)
                return;

            foreach (Player playerAlive in Players.Where(pA => pA.IsAlive)) {
                foreach (Player playerDead in Players.Where(pD => !pD.IsAlive && pD.IsActive)) {
                    if (playerDead.IsBot && dos.IsSkipBots)
                        continue;

                    float distance = playerDead.Position.GetDistance(playerAlive.Position);
                    if (distance > dos.MinimalReviceDistance)
                        continue;

                    playerDead.IsAlive = true;
                    if (playerDead.IsOnRoof)
                        playerDead.Flip();
                    if (dos.IsRepair)
                        playerDead.Repair();

                    int currentPointsChange = playerDead.PointsChange;
                    if (currentPointsChange != Player.CHANGE_POINTS_INITIAL_VALUE) {
                        foreach (Player playerDeadOther in Players) {
                            if (!playerDeadOther.IsAlive && playerDeadOther.IsActive
                                && playerDeadOther != playerDead
                                && playerDeadOther.PointsChange > playerDead.PointsChange) {
                                if (playerDeadOther.PointsChange < 0)
                                    playerDeadOther.PointsChange -= 1;
                            }
                        }
                        playerDead.Points -= playerDead.PointsChange;
                        playerDead.PointsChange = Player.CHANGE_POINTS_INITIAL_VALUE;
                    }

                }
            }
        }

        private void RandomWeaponEquip() {
            RandomWeaponSettings rws = Settings.RandomWeaponSettings;
            if (!rws.IsEnabled)
                return;

            List<Weapon.WeaponId> weapons = rws.WeaponSelector.GetEnabledWeapons();

            if (weapons.Count == 0)
                return;

            if (DateTime.Now.Subtract(rws.NextRandomWeaponTimeStamp).TotalMilliseconds > 0) {
                Weapon.WeaponId nextWeapon = weapons[StaticRandom.Random.Next(weapons.Count)];
                foreach (Player player in Players.Where(p => p.IsAlive)) {
                    if (player.IsBot && rws.IsSkipBots)
                        continue;

                    if (!rws.IsSameWeaponForAll) 
                        nextWeapon = weapons[StaticRandom.Random.Next(weapons.Count)];

                    if (rws.IsDropPreviousWeapon || player.Weapon.GetActiveWeaponId() == WeaponId.None)
                        player.EquipWeapon(nextWeapon);
                }

                rws.NextRandomWeaponTimeStamp = DateTime.Now.AddSeconds(StaticRandom.Random.Next(
                    Math.Min(rws.MinimalTimeInS, rws.MaximalTimeInS),
                    Math.Max(rws.MinimalTimeInS, rws.MaximalTimeInS)
                ));
            }
        }

        private void ChangeWeaponBoxes() {
            WeaponBoxesSettings wbs = Settings.WeaponBoxesSettings;
            if (!wbs.IsEnabled)
                return;
        }

        private void FreezePoints(Player player) {
            FreezePointsSettings fp = player.Settings.FreezePointsSettings;
            if (fp.IsFreeze)
                player.Points = fp.Points;
        }

        private void FreezePlayer(Player player) {
            FreezePositionSettings fps = player.Settings.FreezePositionSettings;
            if (fps.HasFreeze) {
                if (fps.IsFreezeX)
                    player.Position.X = fps.Position.X;
                if (fps.IsFreezeY)
                    player.Position.Y = fps.Position.Y;
                if (fps.IsFreezeZ)
                    player.Position.Z = fps.Position.Z;

                player.Position = player.Position; //Force Update
            }
        }
        

        public void Update() {
            if (!IsRunning)
                return;

            playerCount = Process[PLAYER_COUNT].Read<int>(); //Memory.Read<int>(PLAYER_COUNT);
            maximumDistance = maxDistance.Value;
            distanceWarningThreshold = warningDistance.Value;
            maximumPoints = Process[MAXIMUM_POINTS].Read<int>(); //Memory.Read<int>(PLAYER_COUNT);
            
            maximumDamage = maxDamage.Value;

            isActive = Process[GAME_ACTIVE].Read<bool>();

            cameraTiltMultiplicator = cameraTilt.Value;
            cameraHeightDistanceDivider = cameraHeightDivider.Value;
            cameraHeightDistanceAdd = cameraHeightAdd.Value;
            cameraHeightDistanceFactor = cameraHeightFactor.Value;
            cameraZoomLimitValue = cameraZoomLimit.Value;

            foreach (Player player in Players) {
                player.Update();
            }

            ExecuteExtraFeatures();
            RaisePropertyChanged();
        }
 
        public void EquipWeapon(PlayerId playerId, WeaponId weaponId) {
            DropWeapon(playerId);
            funcChangeWeapon.Execute(CallingConventions.Stdcall, (int)playerId, (int)weaponId);
        }
        public void DropWeapon(PlayerId playerId) {
            funcDropWeapon.Execute(CallingConventions.Stdcall, (int)playerId);
        }

    #region IDisposable Support
        private bool disposed = false; // To detect redundant calls

        protected virtual void DoDispose() {
            if (!disposed) {
                if (IsRunning) {
                    //Revert changed pointers/immediates in code
                    foreach (PatchedFloat patch in patches) {
                        patch.Restore();
                    }
                    Process.Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }
        ~Game() {
            DoDispose();
        }
        #endregion
    }

    //A float in the game's memory that a set of instructions is redirected to (operand addresses) and/or
    //copies as an immediate (e.g. mov [x], imm32). Addresses are absolute (image base 0x400000).
    public class PatchedFloat {
        private const int IMAGE_BASE = 0x400000;
        private readonly MemorySharp process;
        private readonly RemoteAllocation memory;
        private readonly int originalAddress;
        private readonly float original;
        private readonly int[] operandSites;
        private readonly int[] immediateSites;

        public PatchedFloat(MemorySharp process, int originalAddress, int[] operandSites, int[] immediateSites) {
            this.process = process;
            this.originalAddress = originalAddress;
            this.operandSites = operandSites;
            this.immediateSites = immediateSites ?? new int[0];
            original = process[new IntPtr(originalAddress - IMAGE_BASE)].Read<float>();
            memory = process.Memory.Allocate(4);
            memory.Write<float>(original);
            int target = memory.Information.AllocationBase.ToInt32();
            foreach (int site in operandSites) {
                process[new IntPtr(site - IMAGE_BASE)].Write<int>(target);
            }
        }

        public float Value {
            get { return memory.Read<float>(); }
            set {
                memory.Write<float>(value);
                foreach (int site in immediateSites) {
                    process[new IntPtr(site - IMAGE_BASE)].Write<float>(value);
                }
            }
        }

        public void Restore() {
            foreach (int site in operandSites) {
                process[new IntPtr(site - IMAGE_BASE)].Write<int>(originalAddress);
            }
            foreach (int site in immediateSites) {
                process[new IntPtr(site - IMAGE_BASE)].Write<float>(original);
            }
        }
    }
}
