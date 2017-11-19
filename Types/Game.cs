using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Binarysharp.Assemblers.Fasm;
using System.IO;
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
        private IntPtr MAXIMUM_DISTANCE;
        private IntPtr MAXIMUM_DISTANCE_ORIGINAL = new IntPtr(0x5DD620 - PROCESS_BASE);
        private IntPtr DISTANCE_WARNING_THRESHOLD = new IntPtr(0x5DDB14 - PROCESS_BASE);
        private IntPtr MAXIMUM_POINTS = new IntPtr(0x658DE4 - PROCESS_BASE); //0x659338 //Readonly
        private IntPtr MAXIMUM_DAMAGE_TRESHOLD;
        private IntPtr MAXIMUM_DAMAGE_TRESHOLD_ORIGINAL = new IntPtr(0x5DE290 - PROCESS_BASE);
        private IntPtr MAXIMUM_DAMAGE_RESET_VALUE = new IntPtr(0x423FFA - PROCESS_BASE); //Change value in asm mov
        private IntPtr GAME_ACTIVE = new IntPtr(0x6AE110 - PROCESS_BASE); //also zero on pause
        private IntPtr CAMERA_TILT_MULTIPLICATOR = new IntPtr(0x5DE290 - PROCESS_BASE); //normally bound to max distance
        private IntPtr CAMERA_HEIGHT_DISTANCE_DIVIDER;
        private IntPtr CAMERA_HEIGHT_DISTANCE_DIVIDER_ORIGINAL = new IntPtr(0x5DD41C - PROCESS_BASE);
        private IntPtr CAMERA_HEIGHT_DISTANCE_ADD;
        private IntPtr CAMERA_HEIGHT_DISTANCE_ADD_ORIGINAL = new IntPtr(0x5DD330 - PROCESS_BASE);

        //Pointers to change target address in code;
        private IntPtr MAXIMUM_DISTANCE_POINTER = new IntPtr(0x41340D - PROCESS_BASE);
        private IntPtr MAXIMUM_DAMAGE_TRESHOLD_POINTER = new IntPtr(0x423FEC - PROCESS_BASE);
        private IntPtr CAMERA_HEIGHT_DISTANCE_DIVIDER_POINTER = new IntPtr(0x450BC0 - PROCESS_BASE);
        private IntPtr CAMERA_HEIGHT_DISTANCE_ADD_POINTER = new IntPtr(0x450BC6 - PROCESS_BASE);

        private RemoteAllocation funcChangeWeapon;
        private RemoteAllocation funcDropWeapon;

        private RemoteAllocation memMaxDistance;
        private RemoteAllocation memMaxDamage;
        private RemoteAllocation memCameraHeightDistanceDivider;
        private RemoteAllocation memCameraHeightDistanceAdd;

        public List<Player> Players = new List<Player>();

        public bool IsRunning {
            get { return Process.IsRunning; }
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
                memMaxDistance.Write<float>(value);
                maximumDistance = value;
            }
        }
        private float distanceWarningThreshold;
        public float DistanceWarningThreshold {
            get { return distanceWarningThreshold; }
            set {
                Process[DISTANCE_WARNING_THRESHOLD].Write<float>(value);
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
                memMaxDamage.Write<float>(value);
                Process[MAXIMUM_DAMAGE_RESET_VALUE].Write<float>(value);
            }
        }

        private float cameraTiltMultiplicator;
        public float CameraTiltMultiplicator {
            get { return cameraTiltMultiplicator; }
            set {
                Process[CAMERA_TILT_MULTIPLICATOR].Write<float>(value);
            }
        }

        private float cameraHeightDistanceDivider;
        public float CameraHeightDistanceDivider {
            get { return cameraHeightDistanceDivider; }
            set {
                memCameraHeightDistanceDivider.Write<float>(value);
            }
        }
        private float cameraHeightDistanceAdd;
        public float CameraHeightDistanceAdd {
            get { return cameraHeightDistanceAdd; }
            set {
                memCameraHeightDistanceAdd.Write<float>(value);
            }
        }

        public WeaponHelper WeaponHelper;

        public Game(Process process) : base(process) {
            readAndInjectAsmFunctions();
            this.Settings = new GameSettings();
            this.WeaponHelper = new WeaponHelper(this);
        }
        
        private void readAndInjectAsmFunctions() {
            byte[] asmBytes = File.ReadAllBytes("Asm\\ChangeWeapon.bin");
            funcChangeWeapon = Process.Memory.Allocate(asmBytes.Length);
            funcChangeWeapon.Write<byte>(asmBytes);

            asmBytes = File.ReadAllBytes("Asm\\DropWeapon.bin");
            funcDropWeapon = Process.Memory.Allocate(asmBytes.Length);
            funcDropWeapon.Write<byte>(asmBytes);

            memMaxDistance = Process.Memory.Allocate(4);
            MAXIMUM_DISTANCE = memMaxDistance.Information.AllocationBase;
            Process[MAXIMUM_DISTANCE_POINTER].Write<int>(MAXIMUM_DISTANCE.ToInt32());
            MaximumDistance = Process[MAXIMUM_DISTANCE_ORIGINAL].Read<float>();

            memMaxDamage = Process.Memory.Allocate(4);
            MAXIMUM_DAMAGE_TRESHOLD = memMaxDamage.Information.AllocationBase;
            Process[MAXIMUM_DAMAGE_TRESHOLD_POINTER].Write<int>(MAXIMUM_DAMAGE_TRESHOLD.ToInt32());
            MaximumDamage = Process[MAXIMUM_DAMAGE_TRESHOLD_ORIGINAL].Read<float>();


            memCameraHeightDistanceDivider = Process.Memory.Allocate(4);
            CAMERA_HEIGHT_DISTANCE_DIVIDER = memCameraHeightDistanceDivider.Information.AllocationBase;
            Process[CAMERA_HEIGHT_DISTANCE_DIVIDER_POINTER].Write<int>(CAMERA_HEIGHT_DISTANCE_DIVIDER.ToInt32());
            CameraHeightDistanceDivider = Process[CAMERA_HEIGHT_DISTANCE_DIVIDER_ORIGINAL].Read<float>();

            memCameraHeightDistanceAdd = Process.Memory.Allocate(4);
            CAMERA_HEIGHT_DISTANCE_ADD = memCameraHeightDistanceAdd.Information.AllocationBase;
            Process[CAMERA_HEIGHT_DISTANCE_ADD_POINTER].Write<int>(CAMERA_HEIGHT_DISTANCE_ADD.ToInt32());
            CameraHeightDistanceAdd = Process[CAMERA_HEIGHT_DISTANCE_ADD_ORIGINAL].Read<float>();
        }

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
            maximumDistance = memMaxDistance.Read<float>();
            distanceWarningThreshold = Process[DISTANCE_WARNING_THRESHOLD].Read<float>();
            maximumPoints = Process[MAXIMUM_POINTS].Read<int>(); //Memory.Read<int>(PLAYER_COUNT);
            
            maximumDamage = Process[MAXIMUM_DAMAGE_RESET_VALUE].Read<float>();

            isActive = Process[GAME_ACTIVE].Read<bool>();

            cameraTiltMultiplicator = Process[CAMERA_TILT_MULTIPLICATOR].Read<float>();
            cameraHeightDistanceDivider = memCameraHeightDistanceDivider.Read<float>();
            cameraHeightDistanceAdd = memCameraHeightDistanceAdd.Read<float>();

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
                    //Revert changed pointers in code
                    Process[MAXIMUM_DISTANCE_POINTER].Write<int>(MAXIMUM_DISTANCE_ORIGINAL.ToInt32() + PROCESS_BASE);
                    Process[MAXIMUM_DAMAGE_TRESHOLD_POINTER].Write<int>(MAXIMUM_DAMAGE_TRESHOLD_ORIGINAL.ToInt32() + PROCESS_BASE);
                    Process[CAMERA_HEIGHT_DISTANCE_DIVIDER_POINTER].Write<int>(CAMERA_HEIGHT_DISTANCE_DIVIDER_ORIGINAL.ToInt32() + PROCESS_BASE);
                    Process[CAMERA_HEIGHT_DISTANCE_ADD_POINTER].Write<int>(CAMERA_HEIGHT_DISTANCE_ADD_ORIGINAL.ToInt32() + PROCESS_BASE);
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
}
