using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace SciLors_Mashed_Trainer.Types {
    public class Game : BaseMemorySharp {
        private IntPtr PLAYER_COUNT = new IntPtr(0x8D8B30 - PROCESS_BASE);
        private IntPtr DISTANCE_THRESHOLD = new IntPtr(0x5DDB14 - PROCESS_BASE);

        public int PlayerCount {
            get {
                //return Memory.Read<int>(PLAYER_COUNT);
                return Memory[PLAYER_COUNT].Read<int>();
            }
        }

        public float MaximumDistance
        {
            get { return 10; }
        }
        public float DistanceWarningThreshold
        {
            get { return Memory[DISTANCE_THRESHOLD].Read<float>(); }
            set { Memory[DISTANCE_THRESHOLD].Write<float>(value); }
        }

        public bool IsActive
        {
            get { return true; }
        }

        public Game(Process process) : base(process) { }
    }
}
