﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SciLors_Mashed_Trainer.Types {
    public class Player : BaseMemory {
        public enum PlayerId {
            ONE = 0,
            TWO = 1,
            THREE = 2,
            FOUR = 3
        }

        private const int BASE_ADDRESS = 0x8B06E0;
        private const int BASE_ADDRESS_EXTENDER = 0x928;
        private const int BASE_POINTS_ADDRESS = 0x8D8B40;
        
        private const int PLAYER_ALIVE = 0x04; //0/1

        private const int PLAYER_POSITION_X = BASE_ADDRESS_EXTENDER + 0x30;
        private const int PLAYER_POSITION_Y = BASE_ADDRESS_EXTENDER + 0x38;
        private const int PLAYER_POSITION_Z = BASE_ADDRESS_EXTENDER + 0x34;

        //Distances between each players options
        private const int PLAYER_BASE_DISTANCE = 0xD04;
        private const int PLAYER_WEAPON_DISTANCE = 0xB4;
        private const int PLAYER_POINTS_DISTANCE = 0x4;

        private int addressPoints;
        public int Points {
            get {
                return ReadInt(addressPoints);
            }
            set {
                Write(addressPoints, value);
            }
        }

        private int addressBase;
        public bool IsActive {
            get {
                if (Game.PlayerCount > (int)Id)
                    return true;
                return false;
            }
        }
        public bool IsAlive {
            get {
                return ReadBoolean(addressBase + PLAYER_ALIVE);
            }
            set {
                Write(addressBase + PLAYER_ALIVE, value);
            }
        }

        public float PosX {
            get {
                return ReadFloat(addressBase + PLAYER_POSITION_X);
            }
            set {
                Write(addressBase + PLAYER_POSITION_X, value);
            }
        }
        public float PosY {
            get {
                return ReadFloat(addressBase + PLAYER_POSITION_Y);
            }
            set {
                Write(addressBase + PLAYER_POSITION_Y, value);
            }
        }
        public float PosZ {
            get {
                return ReadFloat(addressBase + PLAYER_POSITION_Z);
            }
            set {
                Write(addressBase + PLAYER_POSITION_Z, value);
            }
        }

        public PlayerId Id { get; set; }
        public Game Game { get; set; }
        public Player(Game game, PlayerId id) : base(game.hProcess) {
            Game = game;
            Id = id;
            addressBase = BASE_ADDRESS + PLAYER_BASE_DISTANCE * (int)Id;
            addressPoints = BASE_POINTS_ADDRESS + PLAYER_POINTS_DISTANCE * (int)Id;
        }
    }
}
