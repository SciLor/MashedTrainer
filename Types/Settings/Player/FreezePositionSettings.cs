using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciLors_Mashed_Trainer.Types.Settings.Player {
    public class FreezePositionSettings {
        public bool IsFreezeX { get; set; }
        public bool IsFreezeY { get; set; }
        public bool IsFreezeZ { get; set; }
        public Position Position { get; set; }

        public bool HasFreeze { get { return IsFreezeX | IsFreezeY | IsFreezeZ; } }

        public FreezePositionSettings() {
            Position = new Position();
        }
    }
}
