using SciLors_Mashed_Trainer.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciLors_Mashed_Trainer.Types.Settings.Game {
    public class WeaponBoxesSettings {
        public bool IsEnabled { get; set; }
        public UcWeaponSelector? WeaponSelector { get; set; }
        public bool IsSkipBots { get; set; }

        public WeaponBoxesSettings() {
            IsEnabled = false;
            IsSkipBots = true;
        }
    }
}
