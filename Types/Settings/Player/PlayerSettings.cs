using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciLors_Mashed_Trainer.Types.Settings.Player {
    public class PlayerSettings {
        public FreezePositionSettings FreezePositionSettings { get; set; }
        public FreezePointsSettings FreezePointsSettings { get; set; }

        public PlayerSettings() {
            FreezePositionSettings = new FreezePositionSettings();
            FreezePointsSettings = new FreezePointsSettings();
        }
    }
}
