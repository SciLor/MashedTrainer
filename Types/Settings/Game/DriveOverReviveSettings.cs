using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciLors_Mashed_Trainer.Types.Settings.Game {
    public class DriveOverReviveSettings {

        public bool IsEnabled { get; set; }
        public float MinimalReviceDistance { get; set; }
        public bool IsSkipBots { get; set; }

        public DriveOverReviveSettings() {
            IsEnabled = false;
            MinimalReviceDistance = 2.0f;
            IsSkipBots = true;
        }
    }
}
