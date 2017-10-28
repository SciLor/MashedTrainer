using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SciLors_Mashed_Trainer.Types {
    public class Game : BaseMemory {
        private const int PLAYER_COUNT = 0x8D8B30;

        public int PlayerCount {
            get {
                return ReadInt(PLAYER_COUNT);
            }
        }

        public Game(IntPtr hProcess) : base(hProcess) { }
    }
}
