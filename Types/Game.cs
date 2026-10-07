using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SciLors_Mashed_Trainer.Core;
using SciLors_Mashed_Trainer.Types.Settings.Game;
using SciLors_Mashed_Trainer.Types.Settings.Player;
using SciLors_Mashed_Trainer.Types.Weapons;
using static SciLors_Mashed_Trainer.Types.Player;
using static SciLors_Mashed_Trainer.Types.Weapons.Weapon;

namespace SciLors_Mashed_Trainer.Types {
    public class Game : BaseMemorySharp, IDisposable {
        private readonly IntPtr PLAYER_COUNT = new IntPtr(0x8D8B30);
        private readonly IntPtr MAXIMUM_POINTS = new IntPtr(0x658DE4);
        private readonly IntPtr GAME_ACTIVE = new IntPtr(0x6AE110);

        private IntPtr funcChangeWeapon = IntPtr.Zero;
        private IntPtr funcDropWeapon = IntPtr.Zero;

        private readonly List<PatchedFloat> patches = new List<PatchedFloat>();
        private PatchedFloat maxDistance = null!;
        private PatchedFloat warningDistance = null!;
        private PatchedFloat maxDamage = null!;
        private PatchedFloat cameraTilt = null!;
        private PatchedFloat cameraHeightDivider = null!;
        private PatchedFloat cameraHeightAdd = null!;
        private PatchedFloat cameraHeightFactor = null!;
        private PatchedFloat cameraZoomLimit = null!;

        private PatchedFloat Patch(int originalAddress, int[] pointerSites, int[] immediateSites = null!) {
            var patch = new PatchedFloat(Target, originalAddress, pointerSites, immediateSites);
            patches.Add(patch);
            return patch;
        }

        public List<Player> Players = new List<Player>();

        public bool IsRunning => Target.IsRunning;

        public GameSettings Settings { get; set; } = null!;

        private int playerCount;
        public int PlayerCount => playerCount;

        private float maximumDistance;
        public float MaximumDistance {
            get => maximumDistance;
            set {
                maxDistance.Value = value;
                maximumDistance = value;
                OnPropertyChanged(nameof(MaximumDistance));
            }
        }

        private float distanceWarningThreshold;
        public float DistanceWarningThreshold {
            get => distanceWarningThreshold;
            set {
                warningDistance.Value = value;
                distanceWarningThreshold = value;
                OnPropertyChanged(nameof(DistanceWarningThreshold));
            }
        }

        private bool isActive;
        public bool IsActive => isActive;

        private int maximumPoints;
        public int MaximumPoints => maximumPoints;

        private float maximumDamage;
        public float MaximumDamage {
            get => maximumDamage;
            set {
                maxDamage.Value = value;
                maximumDamage = value;
                OnPropertyChanged(nameof(MaximumDamage));
            }
        }

        private float cameraTiltMultiplicator;
        public float CameraTiltMultiplicator {
            get => cameraTiltMultiplicator;
            set {
                cameraTilt.Value = value;
                cameraTiltMultiplicator = value;
                OnPropertyChanged(nameof(CameraTiltMultiplicator));
            }
        }

        private float cameraHeightDistanceDivider;
        public float CameraHeightDistanceDivider {
            get => cameraHeightDistanceDivider;
            set {
                cameraHeightDivider.Value = value;
                cameraHeightDistanceDivider = value;
                OnPropertyChanged(nameof(CameraHeightDistanceDivider));
            }
        }

        private float cameraHeightDistanceAdd;
        public float CameraHeightDistanceAdd {
            get => cameraHeightDistanceAdd;
            set {
                cameraHeightAdd.Value = value;
                cameraHeightDistanceAdd = value;
                OnPropertyChanged(nameof(CameraHeightDistanceAdd));
            }
        }

        private float cameraHeightDistanceFactor;
        public float CameraHeightDistanceFactor {
            get => cameraHeightDistanceFactor;
            set {
                cameraHeightFactor.Value = value;
                cameraHeightDistanceFactor = value;
                OnPropertyChanged(nameof(CameraHeightDistanceFactor));
            }
        }

        private float cameraZoomLimitValue;
        public float CameraZoomLimit {
            get => cameraZoomLimitValue;
            set {
                cameraZoomLimit.Value = value;
                cameraZoomLimitValue = value;
                OnPropertyChanged(nameof(CameraZoomLimit));
            }
        }

        public WeaponHelper WeaponHelper = null!;

        public Game(IMemoryTarget target) : base(target) {
            readAndInjectAsmFunctions();
            this.Settings = new GameSettings();
            this.WeaponHelper = new WeaponHelper(this);
        }

        private void readAndInjectAsmFunctions() {
            string baseDir = AppContext.BaseDirectory;
            string changeWeaponBin = Path.Combine(baseDir, "Asm", "ChangeWeapon.bin");
            if (File.Exists(changeWeaponBin)) {
                byte[] asmBytes = File.ReadAllBytes(changeWeaponBin);
                funcChangeWeapon = Target.Allocate(asmBytes.Length);
                Target.WriteBytes(funcChangeWeapon, asmBytes);
            }

            string dropWeaponBin = Path.Combine(baseDir, "Asm", "DropWeapon.bin");
            if (File.Exists(dropWeaponBin)) {
                byte[] asmBytes = File.ReadAllBytes(dropWeaponBin);
                funcDropWeapon = Target.Allocate(asmBytes.Length);
                Target.WriteBytes(funcDropWeapon, asmBytes);
            }

            maxDistance = Patch(0x5DD620, new[] { 0x41340D });
            warningDistance = Patch(0x5DDB14, new[] { 0x44C16B });
            maxDamage = Patch(0x5DE290, new[] { 0x423FEC }, new[] { 0x423FFA });
            cameraTilt = Patch(0x5DE290, new[] { 0x450AEC });
            cameraHeightFactor = Patch(0x5DDB18, new[] { 0x450BBA });
            cameraHeightDivider = Patch(0x5DD41C, new[] { 0x450BC0 });
            cameraHeightAdd = Patch(0x5DD330, new[] { 0x450BC6 });
            cameraZoomLimit = Patch(0x5DD620, CAMERA_ZOOM_LIMIT_OPERANDS, CAMERA_ZOOM_LIMIT_IMMEDIATES);
        }

        private static readonly int[] CAMERA_ZOOM_LIMIT_OPERANDS = {
            0x45081A, 0x450956, 0x45099F, 0x450AF8, 0x450B1A, 0x450C69, 0x450C7B, 0x450C93, 0x450CBB,
            0x451013, 0x45101F, 0x45102B, 0x451037, 0x451043, 0x45104F, 0x4511F9, 0x451205, 0x451211,
            0x45121D, 0x451229, 0x451235, 0x4512B8, 0x4512FC, 0x45141E, 0x451468
        };
        private static readonly int[] CAMERA_ZOOM_LIMIT_IMMEDIATES = { 0x45082B, 0x450967, 0x45097A, 0x4509B0 };

        private void ExecuteExtraFeatures() {
            if (!IsActive) return;

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
            if (!dos.IsEnabled) return;

            foreach (Player playerAlive in Players.Where(pA => pA.IsAlive)) {
                foreach (Player playerDead in Players.Where(pD => !pD.IsAlive && pD.IsActive)) {
                    if (playerDead.IsBot && dos.IsSkipBots) continue;

                    float distance = playerDead.Position.GetDistance(playerAlive.Position);
                    if (distance > dos.MinimalReviceDistance) continue;

                    playerDead.IsAlive = true;
                    if (playerDead.IsOnRoof) playerDead.Flip();
                    if (dos.IsRepair) playerDead.Repair();

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
            if (!rws.IsEnabled || rws.WeaponSelector == null) return;

            List<Weapon.WeaponId> weapons = rws.WeaponSelector.GetEnabledWeapons();
            if (weapons.Count == 0) return;

            if (DateTime.Now.Subtract(rws.NextRandomWeaponTimeStamp).TotalMilliseconds > 0) {
                Weapon.WeaponId nextWeapon = weapons[StaticRandom.Random.Next(weapons.Count)];
                foreach (Player player in Players.Where(p => p.IsAlive)) {
                    if (player.IsBot && rws.IsSkipBots) continue;
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
            if (!wbs.IsEnabled) return;
        }

        private void FreezePoints(Player player) {
            FreezePointsSettings fp = player.Settings.FreezePointsSettings;
            if (fp.IsFreeze)
                player.Points = fp.Points;
        }

        private void FreezePlayer(Player player) {
            FreezePositionSettings fps = player.Settings.FreezePositionSettings;
            if (fps.HasFreeze) {
                if (fps.IsFreezeX) player.Position.X = fps.Position.X;
                if (fps.IsFreezeY) player.Position.Y = fps.Position.Y;
                if (fps.IsFreezeZ) player.Position.Z = fps.Position.Z;
                player.Position = player.Position;
            }
        }

        public void Update() {
            if (!IsRunning) return;

            playerCount = Target.Read<int>(PLAYER_COUNT);
            maximumDistance = maxDistance.Value;
            distanceWarningThreshold = warningDistance.Value;
            maximumPoints = Target.Read<int>(MAXIMUM_POINTS);
            maximumDamage = maxDamage.Value;
            isActive = Target.Read<bool>(GAME_ACTIVE);

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
            if (funcChangeWeapon != IntPtr.Zero) {
                Target.ExecuteStdcall(funcChangeWeapon, (int)playerId, (int)weaponId);
            }
        }

        public void DropWeapon(PlayerId playerId) {
            if (funcDropWeapon != IntPtr.Zero) {
                Target.ExecuteStdcall(funcDropWeapon, (int)playerId);
            }
        }

        private bool disposed;
        protected virtual void DoDispose() {
            if (!disposed) {
                if (IsRunning) {
                    foreach (PatchedFloat patch in patches) {
                        patch.Restore();
                    }
                    if (funcChangeWeapon != IntPtr.Zero) Target.Free(funcChangeWeapon);
                    if (funcDropWeapon != IntPtr.Zero) Target.Free(funcDropWeapon);
                    Target.Dispose();
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
    }

    public class PatchedFloat {
        private readonly IMemoryTarget target;
        private readonly IntPtr allocatedMemory;
        private readonly int originalAddress;
        private readonly float original;
        private readonly int[] operandSites;
        private readonly int[] immediateSites;

        public PatchedFloat(IMemoryTarget target, int originalAddress, int[] operandSites, int[] immediateSites = null!) {
            this.target = target;
            this.originalAddress = originalAddress;
            this.operandSites = operandSites;
            this.immediateSites = immediateSites ?? Array.Empty<int>();

            original = target.Read<float>(new IntPtr(originalAddress));
            allocatedMemory = target.Allocate(4);
            target.Write<float>(allocatedMemory, original);

            int targetAddr = allocatedMemory.ToInt32();
            foreach (int site in operandSites) {
                target.Write<int>(new IntPtr(site), targetAddr);
            }
        }

        public float Value {
            get => target.Read<float>(allocatedMemory);
            set {
                target.Write<float>(allocatedMemory, value);
                foreach (int site in immediateSites) {
                    target.Write<float>(new IntPtr(site), value);
                }
            }
        }

        public void Restore() {
            foreach (int site in operandSites) {
                target.Write<int>(new IntPtr(site), originalAddress);
            }
            foreach (int site in immediateSites) {
                target.Write<float>(new IntPtr(site), original);
            }
            target.Free(allocatedMemory);
        }
    }
}
