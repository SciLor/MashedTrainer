using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SciLors_Mashed_Trainer.Types {
    public class Player : BaseMemorySharp {
        public enum PlayerId {
            ONE = 0,
            TWO = 1,
            THREE = 2,
            FOUR = 3
        }

        private IntPtr BASE_ADDRESS = new IntPtr(0x8B06E0 - PROCESS_BASE);
        private IntPtr BASE_POINTS_ADDRESS = new IntPtr(0x8D8B40 - PROCESS_BASE);
        
        private const int PLAYER_ALIVE = 0x04; //0/1

        private const int BASE_ADDRESS_EXTENDER = 0x928;
        private const int PLAYER_POSITION_X = BASE_ADDRESS_EXTENDER + 0x30;
        private const int PLAYER_POSITION_Y = BASE_ADDRESS_EXTENDER + 0x38;
        private const int PLAYER_POSITION_Z = BASE_ADDRESS_EXTENDER + 0x34;

        //Distances between each players options
        private const int PLAYER_BASE_DISTANCE = 0xD04;
        private const int PLAYER_WEAPON_DISTANCE = 0xB4;
        private const int PLAYER_POINTS_DISTANCE = 0x4;

        private int playerPointsOffset;
        public int Points {
            get { return Memory[BASE_POINTS_ADDRESS].Read<int>(playerPointsOffset); }
            set { Memory[BASE_POINTS_ADDRESS].Write<int>(playerPointsOffset, value); }
        }

        public bool IsActive {
            get {
                if (Game.PlayerCount > (int)Id)
                    return true;
                return false;
            }
        }

        private int playerBaseOffset;
        public bool IsAlive {
            get {
                return Memory[BASE_ADDRESS].Read<bool>(playerBaseOffset + PLAYER_ALIVE);
            }
            set {
                Memory[BASE_ADDRESS].Write<bool>(playerBaseOffset + PLAYER_ALIVE, value);
            }
        }

        public float PosX {
            get {
                return Memory[BASE_ADDRESS].Read<float>(playerBaseOffset + PLAYER_POSITION_X);
            }
            set {
                Memory[BASE_ADDRESS].Write<float>(playerBaseOffset + PLAYER_POSITION_X, value);
            }
        }
        public float PosY {
            get {
                return Memory[BASE_ADDRESS].Read<float>(playerBaseOffset + PLAYER_POSITION_Y);
            }
            set {
                Memory[BASE_ADDRESS].Write<float>(playerBaseOffset + PLAYER_POSITION_Y, value);
            }
        }
        public float PosZ {
            get {
                return Memory[BASE_ADDRESS].Read<float>(playerBaseOffset + PLAYER_POSITION_Z);
            }
            set {
                Memory[BASE_ADDRESS].Write<float>(playerBaseOffset + PLAYER_POSITION_Z, value);
            }
        }

        public PlayerId Id { get; set; }
        public Game Game { get; set; }
        public Player(Game game, PlayerId id) : base(game.Memory) {
            Game = game;
            Id = id;
            playerBaseOffset = PLAYER_BASE_DISTANCE * (int)Id;
            playerPointsOffset = PLAYER_POINTS_DISTANCE * (int)Id;
        }
    }
}
