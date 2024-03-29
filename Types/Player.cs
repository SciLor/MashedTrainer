﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using SciLors_Mashed_Trainer.Types.Weapons;
using static SciLors_Mashed_Trainer.Types.Weapons.Weapon;
using SciLors_Mashed_Trainer.Types.Settings.Player;

namespace SciLors_Mashed_Trainer.Types {
    public class Player : BaseMemorySharp {
        public enum PlayerId {
            ONE = 0,
            TWO = 1,
            THREE = 2,
            FOUR = 3
        }

        public const int CHANGE_POINTS_INITIAL_VALUE = -1000; // FFFFFC18

        private IntPtr BASE_ADDRESS = new IntPtr(0x8B06E0 - PROCESS_BASE);
        private IntPtr BASE_WEAPON_ADDRESS = new IntPtr(0x8BEDCC - PROCESS_BASE);
        private IntPtr BASE_POINTS_ADDRESS = new IntPtr(0x8D8B40 - PROCESS_BASE);
        private IntPtr BASE_POINTS_CHANGE_ADDRESS = new IntPtr(0x8D8B80 - PROCESS_BASE);
        private IntPtr BASE_POINT_CHANGE_VISUAL_ADDRESS = new IntPtr(0x8D8B60 - PROCESS_BASE);
        private IntPtr BASE_DISTANCE_ADDRESS = new IntPtr(0x8C7E40 - PROCESS_BASE);
        private IntPtr BASE_DAMAGE_ADDRESS = new IntPtr(0x65A9E8 - PROCESS_BASE);
        private IntPtr BASE_PLAYER_COLOR = new IntPtr(0x69D028 - PROCESS_BASE); //Only Human!

        private const int PLAYER_ALIVE = 0x004; //0/1
        private const int PLAYER_CONTROLS_DISABLED = 0x010; //0/1
        private const int PLAYER_BOT = 0xD00; //0/1

        private const int BASE_ADDRESS_EXTENDER = 0x928;
        private const int PLAYER_POSITION_X = BASE_ADDRESS_EXTENDER + 0x30;
        private const int PLAYER_POSITION_Y = BASE_ADDRESS_EXTENDER + 0x38;
        private const int PLAYER_POSITION_Z = BASE_ADDRESS_EXTENDER + 0x34;

        private const int PLAYER_POINTS_CHANGE_OFFSET = 0x40;
        private const int PLAYER_POINTS_CHANGE_VISUAL_OFFSET = 0x20;

        //Distances between each players options
        private const int PLAYER_BASE_DISTANCE = 0xD04;
        private const int PLAYER_WEAPON_DISTANCE = 0xB4;
        private const int PLAYER_POINTS_DISTANCE = 0x4;
        private const int PLAYER_DISTANCE_DISTANCE = 0x4;
        private const int PLAYER_DAMAGE_DISTANCE = 0x28;
        private const int PLAYER_COLOR_DISTANCE = 0xC;

        private const int DAMAGE_FRONT_DAMAGE_OFFSET = 0x00;
        private const int DAMAGE_BACK_DAMAGE_OFFSET = 0x04;
        private const int DAMAGE_HOOD_OFFSET = 0x08;
        private const int DAMAGE_TRUNK_OFFSET = 0x0C;
        private const int DAMAGE_GLASS_HOOD_OFFSET = 0x10;
        private const int DAMAGE_GLASS_TRUNK_OFFSET = 0x14;

        public PlayerSettings Settings { get; set; }

        private int playerPointsOffset;
        private int points;
        public int Points {
            get { return points; }
            set {
                Process[BASE_POINTS_ADDRESS].Write<int>(playerPointsOffset, value);
                points = value;
            }
        }

        public bool IsActive {
            get {
                if (!Game.IsActive)
                    return false;
                if (Game.PlayerCount <= (int)Id)
                    return false;
                return true;
            }
        }

        private int playerWeaponOffset;
        public Weapon Weapon {
            get {
                return Game.WeaponHelper.GetWeapon(this);
            }
        }
        private IntPtr weaponPointer = IntPtr.Zero;
        public IntPtr WeaponPointer {
            get {
                return weaponPointer;
            }
        }

        private int playerBaseOffset;
        private bool isAlive;
        public bool IsAlive {
            get {
                return isAlive;
            }
            set {
                Process[BASE_ADDRESS].Write<bool>(playerBaseOffset + PLAYER_ALIVE, value);
                isAlive = value;
                IsControlsDisabled = !value;
            }
        }
        private bool isControlsDisabled;
        public bool IsControlsDisabled {
            get {
                return isControlsDisabled;
            }
            set {
                Process[BASE_ADDRESS].Write<bool>(playerBaseOffset + PLAYER_CONTROLS_DISABLED, value);
                isControlsDisabled = value;
            }
        }

        private bool isBot;
        public bool IsBot {
            get {
                return isBot;
            }
        }

        private int playerDamageOffset;
        private float damageFront;
        public float DamageFront {
            get { return damageFront; }
            set {
                Process[BASE_DAMAGE_ADDRESS].Write<float>(playerDamageOffset + DAMAGE_FRONT_DAMAGE_OFFSET, value);
            }
        }
        private float damageBack;
        public float DamageBack {
            get { return damageBack; }
            set {
                Process[BASE_DAMAGE_ADDRESS].Write<float>(playerDamageOffset + DAMAGE_BACK_DAMAGE_OFFSET, value);
            }
        }
        private bool isDamagedHood;
        public bool IsDamagedHood {
            get { return isDamagedHood; }
            set {
                Process[BASE_DAMAGE_ADDRESS].Write<bool>(playerDamageOffset + DAMAGE_HOOD_OFFSET, value);
            }
        }
        private bool isDamagedTrunk;
        public bool IsDamagedTrunk {
            get { return isDamagedTrunk; }
            set {
                Process[BASE_DAMAGE_ADDRESS].Write<bool>(playerDamageOffset + DAMAGE_TRUNK_OFFSET, value);
            }
        }
        private bool isDamagedGlassHood;
        public bool IsDamagedGlassHood {
            get { return isDamagedGlassHood; }
            set {
                Process[BASE_DAMAGE_ADDRESS].Write<bool>(playerDamageOffset + DAMAGE_GLASS_HOOD_OFFSET, value);
            }
        }
        private bool isDamagedGlassTrunk;
        public bool IsDamagedGlassTrunk {
            get { return isDamagedGlassTrunk; }
            set {
                Process[BASE_DAMAGE_ADDRESS].Write<bool>(playerDamageOffset + DAMAGE_GLASS_TRUNK_OFFSET, value);
            }
        }
        public bool IsDamagedGlass {
            get { return IsDamagedGlassHood || IsDamagedGlassTrunk; }
            set {
                IsDamagedGlassHood = value;
                IsDamagedGlassTrunk = value;
            }
        }

        private Position position = new Position();
        public Position Position {
            get { return position; }
            set {
                Process[BASE_ADDRESS].Write<float>(playerBaseOffset + PLAYER_POSITION_X, value.X);
                Process[BASE_ADDRESS].Write<float>(playerBaseOffset + PLAYER_POSITION_Y, value.Y);
                Process[BASE_ADDRESS].Write<float>(playerBaseOffset + PLAYER_POSITION_Z, value.Z);

                position = value;
            }
        }

        private int playerDistanceOffset;
        private float distance;
        public float Distance {
            get {
                return distance;
            }
        }

        private int pointsChange;
        public int PointsChange {
            get { return pointsChange; }
            set {
                Process[BASE_POINTS_ADDRESS].Write<int>(playerPointsOffset + PLAYER_POINTS_CHANGE_OFFSET, value);
                Process[BASE_POINTS_ADDRESS].Write<int>(playerPointsOffset + PLAYER_POINTS_CHANGE_VISUAL_OFFSET, value);
            }
        }

        public PlayerId Id { get; set; }
        public Game Game { get; set; }
        public Player(Game game, PlayerId id) : base(game.Process) {
            Game = game;
            game.Players.Add(this);
            Id = id;
            Settings = new PlayerSettings();
            playerBaseOffset = PLAYER_BASE_DISTANCE * (int)Id;
            playerWeaponOffset = PLAYER_WEAPON_DISTANCE * (int)Id;
            playerPointsOffset = PLAYER_POINTS_DISTANCE * (int)Id;
            playerDistanceOffset = PLAYER_DISTANCE_DISTANCE * (int)Id;
            playerDamageOffset = PLAYER_DAMAGE_DISTANCE * (int)Id;
        }

        public void Update() {
            if (!Game.IsRunning)
                return;
            points = Process[BASE_POINTS_ADDRESS].Read<int>(playerPointsOffset);
            pointsChange = Process[BASE_POINTS_ADDRESS].Read<int>(playerPointsOffset + PLAYER_POINTS_CHANGE_OFFSET);

            weaponPointer = new IntPtr(Process[BASE_WEAPON_ADDRESS].Read<int>(playerWeaponOffset));
            isAlive = Process[BASE_ADDRESS].Read<bool>(playerBaseOffset + PLAYER_ALIVE);
            isControlsDisabled = Process[BASE_ADDRESS].Read<bool>(playerBaseOffset + PLAYER_CONTROLS_DISABLED);
            isBot = Process[BASE_ADDRESS].Read<bool>(playerBaseOffset + PLAYER_BOT);

            position.X = Process[BASE_ADDRESS].Read<float>(playerBaseOffset + PLAYER_POSITION_X);
            position.Y = Process[BASE_ADDRESS].Read<float>(playerBaseOffset + PLAYER_POSITION_Y);
            position.Z = Process[BASE_ADDRESS].Read<float>(playerBaseOffset + PLAYER_POSITION_Z);

            distance = Process[BASE_DISTANCE_ADDRESS].Read<float>(playerDistanceOffset);

            damageFront = Process[BASE_DAMAGE_ADDRESS].Read<float>(playerDamageOffset + DAMAGE_FRONT_DAMAGE_OFFSET);
            damageBack = Process[BASE_DAMAGE_ADDRESS].Read<float>(playerDamageOffset + DAMAGE_BACK_DAMAGE_OFFSET);
            isDamagedHood = Process[BASE_DAMAGE_ADDRESS].Read<bool>(playerDamageOffset + DAMAGE_HOOD_OFFSET);
            isDamagedTrunk = Process[BASE_DAMAGE_ADDRESS].Read<bool>(playerDamageOffset + DAMAGE_TRUNK_OFFSET);
            isDamagedGlassHood = Process[BASE_DAMAGE_ADDRESS].Read<bool>(playerDamageOffset + DAMAGE_GLASS_HOOD_OFFSET);
            isDamagedGlassTrunk = Process[BASE_DAMAGE_ADDRESS].Read<bool>(playerDamageOffset + DAMAGE_GLASS_TRUNK_OFFSET);

            RaisePropertyChanged();
        }

        public void EquipWeapon(WeaponId weaponId) {
            Game.EquipWeapon(Id, weaponId);
        }

        public void DropWeapon() {
            Game.DropWeapon(Id);
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
