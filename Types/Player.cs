using System;
using System.ComponentModel;
using SciLors_Mashed_Trainer.Core;
using SciLors_Mashed_Trainer.Types.Settings.Player;
using SciLors_Mashed_Trainer.Types.Weapons;
using static SciLors_Mashed_Trainer.Types.Weapons.Weapon;

namespace SciLors_Mashed_Trainer.Types {
    public class Player : BaseMemorySharp {
        public enum PlayerId {
            ONE = 0,
            TWO = 1,
            THREE = 2,
            FOUR = 3
        }

        public const int CHANGE_POINTS_INITIAL_VALUE = -1000;

        private readonly IntPtr BASE_ADDRESS = new IntPtr(0x8B06E0);
        private readonly IntPtr BASE_WEAPON_ADDRESS = new IntPtr(0x8BEDCC);
        private readonly IntPtr BASE_POINTS_ADDRESS = new IntPtr(0x8D8B40);
        private readonly IntPtr BASE_DISTANCE_ADDRESS = new IntPtr(0x8C7E40);
        private readonly IntPtr BASE_DAMAGE_ADDRESS = new IntPtr(0x65A9E8);

        private const int PLAYER_ALIVE = 0x004;
        private const int PLAYER_CONTROLS_DISABLED = 0x010;
        private const int PLAYER_BOT = 0xD00;

        private const int PLAYER_MATRIX = 0x928;
        private const int PLAYER_MATRIX_SIZE = 0x40;
        private const int PLAYER_MATRIX_INDEX = 0x9AC;
        private const int MATRIX_RIGHT = 0x00;
        private const int MATRIX_UP = 0x10;
        private const int MATRIX_AT = 0x20;
        private const int MATRIX_POS = 0x30;
        private const int PLAYER_VELOCITY = 0x144;
        private const int PLAYER_POSITION_X = MATRIX_POS + 0x00;
        private const int PLAYER_POSITION_Y = MATRIX_POS + 0x08;
        private const int PLAYER_POSITION_Z = MATRIX_POS + 0x04;
        private const float FLIP_LIFT = 1.5f;

        private const int PLAYER_POINTS_CHANGE_OFFSET = 0x40;
        private const int PLAYER_POINTS_CHANGE_VISUAL_OFFSET = 0x20;

        private const int PLAYER_BASE_DISTANCE = 0xD04;
        private const int PLAYER_WEAPON_DISTANCE = 0xB4;
        private const int PLAYER_POINTS_DISTANCE = 0x4;
        private const int PLAYER_DISTANCE_DISTANCE = 0x4;
        private const int PLAYER_DAMAGE_DISTANCE = 0x28;

        private const int DAMAGE_FRONT_DAMAGE_OFFSET = 0x00;
        private const int DAMAGE_BACK_DAMAGE_OFFSET = 0x04;
        private const int DAMAGE_HOOD_OFFSET = 0x08;
        private const int DAMAGE_TRUNK_OFFSET = 0x0C;
        private const int DAMAGE_GLASS_HOOD_OFFSET = 0x10;
        private const int DAMAGE_GLASS_TRUNK_OFFSET = 0x14;

        public PlayerSettings Settings { get; set; }

        private readonly int playerPointsOffset;
        private int points;
        public int Points {
            get => points;
            set {
                Target.Write<int>(IntPtr.Add(BASE_POINTS_ADDRESS, playerPointsOffset), value);
                points = value;
                OnPropertyChanged(nameof(Points));
            }
        }

        public bool IsActive {
            get {
                if (!Game.IsActive) return false;
                if (Game.PlayerCount <= (int)Id) return false;
                return true;
            }
        }

        private readonly int playerWeaponOffset;
        public Weapon Weapon => Game.WeaponHelper.GetWeapon(this);
        private IntPtr weaponPointer = IntPtr.Zero;
        public IntPtr WeaponPointer => weaponPointer;

        private readonly int playerBaseOffset;
        private bool isOnRoof;
        public bool IsOnRoof => isOnRoof;

        private int matrixIndex;
        private int MatrixOffset(int index) {
            return playerBaseOffset + PLAYER_MATRIX + ((index & 1) * PLAYER_MATRIX_SIZE);
        }

        private bool isAlive;
        public bool IsAlive {
            get => isAlive;
            set {
                Target.Write<bool>(IntPtr.Add(BASE_ADDRESS, playerBaseOffset + PLAYER_ALIVE), value);
                isAlive = value;
                IsControlsDisabled = !value;
                OnPropertyChanged(nameof(IsAlive));
            }
        }

        private bool isControlsDisabled;
        public bool IsControlsDisabled {
            get => isControlsDisabled;
            set {
                Target.Write<bool>(IntPtr.Add(BASE_ADDRESS, playerBaseOffset + PLAYER_CONTROLS_DISABLED), value);
                isControlsDisabled = value;
                OnPropertyChanged(nameof(IsControlsDisabled));
            }
        }

        private bool isBot;
        public bool IsBot => isBot;

        private readonly int playerDamageOffset;
        private float damageFront;
        public float DamageFront {
            get => damageFront;
            set {
                Target.Write<float>(IntPtr.Add(BASE_DAMAGE_ADDRESS, playerDamageOffset + DAMAGE_FRONT_DAMAGE_OFFSET), value);
                damageFront = value;
                OnPropertyChanged(nameof(DamageFront));
            }
        }

        private float damageBack;
        public float DamageBack {
            get => damageBack;
            set {
                Target.Write<float>(IntPtr.Add(BASE_DAMAGE_ADDRESS, playerDamageOffset + DAMAGE_BACK_DAMAGE_OFFSET), value);
                damageBack = value;
                OnPropertyChanged(nameof(DamageBack));
            }
        }

        private bool isDamagedHood;
        public bool IsDamagedHood {
            get => isDamagedHood;
            set {
                Target.Write<bool>(IntPtr.Add(BASE_DAMAGE_ADDRESS, playerDamageOffset + DAMAGE_HOOD_OFFSET), value);
                isDamagedHood = value;
                OnPropertyChanged(nameof(IsDamagedHood));
            }
        }

        private bool isDamagedTrunk;
        public bool IsDamagedTrunk {
            get => isDamagedTrunk;
            set {
                Target.Write<bool>(IntPtr.Add(BASE_DAMAGE_ADDRESS, playerDamageOffset + DAMAGE_TRUNK_OFFSET), value);
                isDamagedTrunk = value;
                OnPropertyChanged(nameof(IsDamagedTrunk));
            }
        }

        private bool isDamagedGlassHood;
        public bool IsDamagedGlassHood {
            get => isDamagedGlassHood;
            set {
                Target.Write<bool>(IntPtr.Add(BASE_DAMAGE_ADDRESS, playerDamageOffset + DAMAGE_GLASS_HOOD_OFFSET), value);
                isDamagedGlassHood = value;
                OnPropertyChanged(nameof(IsDamagedGlassHood));
            }
        }

        private bool isDamagedGlassTrunk;
        public bool IsDamagedGlassTrunk {
            get => isDamagedGlassTrunk;
            set {
                Target.Write<bool>(IntPtr.Add(BASE_DAMAGE_ADDRESS, playerDamageOffset + DAMAGE_GLASS_TRUNK_OFFSET), value);
                isDamagedGlassTrunk = value;
                OnPropertyChanged(nameof(IsDamagedGlassTrunk));
            }
        }

        public bool IsDamagedGlass {
            get => IsDamagedGlassHood || IsDamagedGlassTrunk;
            set {
                IsDamagedGlassHood = value;
                IsDamagedGlassTrunk = value;
            }
        }

        private bool isUpdatingFromMemory;
        private Position position = new Position();
        public Position Position {
            get => position;
            set {
                if (position != null) {
                    position.PropertyChanged -= Position_PropertyChanged;
                }
                position = value ?? new Position();
                position.PropertyChanged += Position_PropertyChanged;
                WritePositionToMemory(position);
                OnPropertyChanged(nameof(Position));
            }
        }

        private void Position_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
            if (isUpdatingFromMemory) return;
            WritePositionToMemory(position);
        }

        private void WritePositionToMemory(Position pos) {
            for (int i = 0; i < 2; i++) {
                Target.Write<float>(IntPtr.Add(BASE_ADDRESS, MatrixOffset(i) + PLAYER_POSITION_X), pos.X);
                Target.Write<float>(IntPtr.Add(BASE_ADDRESS, MatrixOffset(i) + PLAYER_POSITION_Y), pos.Y);
                Target.Write<float>(IntPtr.Add(BASE_ADDRESS, MatrixOffset(i) + PLAYER_POSITION_Z), pos.Z);
            }
        }

        private readonly int playerDistanceOffset;
        private float distance;
        public float Distance => distance;

        private int pointsChange;
        public int PointsChange {
            get => pointsChange;
            set {
                Target.Write<int>(IntPtr.Add(BASE_POINTS_ADDRESS, playerPointsOffset + PLAYER_POINTS_CHANGE_OFFSET), value);
                Target.Write<int>(IntPtr.Add(BASE_POINTS_ADDRESS, playerPointsOffset + PLAYER_POINTS_CHANGE_VISUAL_OFFSET), value);
                pointsChange = value;
                OnPropertyChanged(nameof(PointsChange));
            }
        }

        public PlayerId Id { get; set; }
        public Game Game { get; set; }

        public Player(Game game, PlayerId id) : base(game.Target) {
            Game = game;
            game.Players.Add(this);
            Id = id;
            Settings = new PlayerSettings();
            position.PropertyChanged += Position_PropertyChanged;
            playerBaseOffset = PLAYER_BASE_DISTANCE * (int)Id;
            playerWeaponOffset = PLAYER_WEAPON_DISTANCE * (int)Id;
            playerPointsOffset = PLAYER_POINTS_DISTANCE * (int)Id;
            playerDistanceOffset = PLAYER_DISTANCE_DISTANCE * (int)Id;
            playerDamageOffset = PLAYER_DAMAGE_DISTANCE * (int)Id;
        }

        public void Update() {
            if (!Game.IsRunning) return;

            points = Target.Read<int>(IntPtr.Add(BASE_POINTS_ADDRESS, playerPointsOffset));
            pointsChange = Target.Read<int>(IntPtr.Add(BASE_POINTS_ADDRESS, playerPointsOffset + PLAYER_POINTS_CHANGE_OFFSET));

            weaponPointer = new IntPtr(Target.Read<int>(IntPtr.Add(BASE_WEAPON_ADDRESS, playerWeaponOffset)));
            isAlive = Target.Read<bool>(IntPtr.Add(BASE_ADDRESS, playerBaseOffset + PLAYER_ALIVE));
            isControlsDisabled = Target.Read<bool>(IntPtr.Add(BASE_ADDRESS, playerBaseOffset + PLAYER_CONTROLS_DISABLED));
            bool newIsBot = Target.Read<bool>(IntPtr.Add(BASE_ADDRESS, playerBaseOffset + PLAYER_BOT));
            if (newIsBot != isBot) {
                isBot = newIsBot;
                OnPropertyChanged(nameof(IsBot));
            }

            matrixIndex = Target.Read<int>(IntPtr.Add(BASE_ADDRESS, playerBaseOffset + PLAYER_MATRIX_INDEX)) & 1;
            isOnRoof = Target.Read<float>(IntPtr.Add(BASE_ADDRESS, MatrixOffset(matrixIndex) + MATRIX_UP + 4)) < 0;

            isUpdatingFromMemory = true;
            try {
                position.X = Target.Read<float>(IntPtr.Add(BASE_ADDRESS, MatrixOffset(matrixIndex) + PLAYER_POSITION_X));
                position.Y = Target.Read<float>(IntPtr.Add(BASE_ADDRESS, MatrixOffset(matrixIndex) + PLAYER_POSITION_Y));
                position.Z = Target.Read<float>(IntPtr.Add(BASE_ADDRESS, MatrixOffset(matrixIndex) + PLAYER_POSITION_Z));
            } finally {
                isUpdatingFromMemory = false;
            }

            distance = Target.Read<float>(IntPtr.Add(BASE_DISTANCE_ADDRESS, playerDistanceOffset));

            damageFront = Target.Read<float>(IntPtr.Add(BASE_DAMAGE_ADDRESS, playerDamageOffset + DAMAGE_FRONT_DAMAGE_OFFSET));
            damageBack = Target.Read<float>(IntPtr.Add(BASE_DAMAGE_ADDRESS, playerDamageOffset + DAMAGE_BACK_DAMAGE_OFFSET));
            isDamagedHood = Target.Read<bool>(IntPtr.Add(BASE_DAMAGE_ADDRESS, playerDamageOffset + DAMAGE_HOOD_OFFSET));
            isDamagedTrunk = Target.Read<bool>(IntPtr.Add(BASE_DAMAGE_ADDRESS, playerDamageOffset + DAMAGE_TRUNK_OFFSET));
            isDamagedGlassHood = Target.Read<bool>(IntPtr.Add(BASE_DAMAGE_ADDRESS, playerDamageOffset + DAMAGE_GLASS_HOOD_OFFSET));
            isDamagedGlassTrunk = Target.Read<bool>(IntPtr.Add(BASE_DAMAGE_ADDRESS, playerDamageOffset + DAMAGE_GLASS_TRUNK_OFFSET));

            RaisePropertyChanged();
        }

        public void EquipWeapon(WeaponId weaponId) {
            Game.EquipWeapon(Id, weaponId);
        }

        public void DropWeapon() {
            Game.DropWeapon(Id);
        }

        private float[] ReadVector(int offset) {
            return new float[] {
                Target.Read<float>(IntPtr.Add(BASE_ADDRESS, offset)),
                Target.Read<float>(IntPtr.Add(BASE_ADDRESS, offset + 4)),
                Target.Read<float>(IntPtr.Add(BASE_ADDRESS, offset + 8))
            };
        }

        private void WriteVector(int offset, float[] v) {
            for (int i = 0; i < 3; i++) {
                Target.Write<float>(IntPtr.Add(BASE_ADDRESS, offset + 4 * i), v[i]);
            }
        }

        private static float Dot(float[] a, float[] b) {
            return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
        }

        public void Flip() {
            int current = MatrixOffset(Target.Read<int>(IntPtr.Add(BASE_ADDRESS, playerBaseOffset + PLAYER_MATRIX_INDEX)));
            float[] right = ReadVector(current + MATRIX_RIGHT);
            float[] up = ReadVector(current + MATRIX_UP);
            float[] pos = ReadVector(current + MATRIX_POS);
            for (int i = 0; i < 3; i++) {
                right[i] = -right[i];
                up[i] = -up[i];
            }
            pos[1] += FLIP_LIFT;
            for (int i = 0; i < 2; i++) {
                WriteVector(MatrixOffset(i) + MATRIX_RIGHT, right);
                WriteVector(MatrixOffset(i) + MATRIX_UP, up);
                WriteVector(MatrixOffset(i) + MATRIX_POS, pos);
            }
        }

        public void Turn(float degrees) {
            int current = MatrixOffset(Target.Read<int>(IntPtr.Add(BASE_ADDRESS, playerBaseOffset + PLAYER_MATRIX_INDEX)));
            float[] right = ReadVector(current + MATRIX_RIGHT);
            float[] at = ReadVector(current + MATRIX_AT);
            float[] velocity = ReadVector(playerBaseOffset + PLAYER_VELOCITY);
            double rad = degrees * Math.PI / 180.0;
            float c = (float)Math.Cos(rad);
            float s = (float)Math.Sin(rad);

            float[] newRight = new float[3];
            float[] newAt = new float[3];
            float[] newVelocity = new float[3];
            float vRight = Dot(velocity, right);
            float vAt = Dot(velocity, at);
            for (int i = 0; i < 3; i++) {
                newRight[i] = c * right[i] + s * at[i];
                newAt[i] = c * at[i] - s * right[i];
                newVelocity[i] = velocity[i] + vRight * (newRight[i] - right[i]) + vAt * (newAt[i] - at[i]);
            }
            for (int i = 0; i < 2; i++) {
                WriteVector(MatrixOffset(i) + MATRIX_RIGHT, newRight);
                WriteVector(MatrixOffset(i) + MATRIX_AT, newAt);
            }
            WriteVector(playerBaseOffset + PLAYER_VELOCITY, newVelocity);
        }

        private void RepairFront() {
            DamageFront = 0.0f;
            IsDamagedHood = false;
            IsDamagedGlassHood = false;
        }

        private void RepairBack() {
            DamageBack = 0.0f;
            IsDamagedTrunk = false;
            IsDamagedGlassTrunk = false;
        }

        public void Repair() {
            RepairFront();
            RepairBack();
        }
    }
}
