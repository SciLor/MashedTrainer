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
        private IntPtr DISTANCE_THRESHOLD = new IntPtr(0x5DDB14 - PROCESS_BASE);
        private IntPtr MAXIMUM_POINTS = new IntPtr(0x658DE4 - PROCESS_BASE); //0x659338 //Readonly

        private RemoteAllocation funcChangeWeapon;
        private RemoteAllocation funcDropWeapon;

        public List<Player> Players = new List<Player>();

        public GameSettings Settings {
            get; set;
        }

        private int playerCount;
        public int PlayerCount {
            get {
                return playerCount;
            }
        }

        public float MaximumDistance {
            get { return 10; }
        }
        private float distanceWarningThreshold;
        public float DistanceWarningThreshold {
            get { return distanceWarningThreshold; }
            set {
                Process[DISTANCE_THRESHOLD].Write<float>(value);
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

        public WeaponHelper WeaponHelper;

        public Game(Process process) : base(process) {
            readAndInjectAsmFunctions();
            this.Settings = new GameSettings();
            this.WeaponHelper = new WeaponHelper(this);
        }
        
        private void readAndInjectAsmFunctions() {
            byte[] asmBytes = File.ReadAllBytes("Asm\\ChangeWeapon.bin");
            funcChangeWeapon = Process.Memory.Allocate(asmBytes.Length, MemoryProtectionFlags.ExecuteReadWrite);
            funcChangeWeapon.Write<byte>(asmBytes);

            asmBytes = File.ReadAllBytes("Asm\\DropWeapon.bin");
            funcDropWeapon = Process.Memory.Allocate(asmBytes.Length, MemoryProtectionFlags.ExecuteReadWrite);
            funcDropWeapon.Write<byte>(asmBytes);
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
                    rws.MinimalTimeInS,
                    rws.MaximalTimeInS
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
            playerCount = Process[PLAYER_COUNT].Read<int>(); //Memory.Read<int>(PLAYER_COUNT);
            distanceWarningThreshold = Process[DISTANCE_THRESHOLD].Read<float>();
            isActive = true;
            maximumPoints = Process[MAXIMUM_POINTS].Read<int>(); //Memory.Read<int>(PLAYER_COUNT);

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
                Process.Dispose();
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
