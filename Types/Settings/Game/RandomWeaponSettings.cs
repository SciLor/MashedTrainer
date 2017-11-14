using SciLors_Mashed_Trainer.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciLors_Mashed_Trainer.Types.Settings.Game {
    public class RandomWeaponSettings {
        public bool IsEnabled { get; set; }
        public int MinimalTimeInS { get; set; }
        public int MaximalTimeInS { get; set; }
        public UcWeaponSelector WeaponSelector;
        public bool IsSameWeaponForAll { get; set; }
        public bool IsDropPreviousWeapon { get; set; }
        public bool IsSkipBots { get; set; }
        public DateTime NextRandomWeaponTimeStamp { get; set; }

        public RandomWeaponSettings() {
            IsEnabled = false;
            MinimalTimeInS = 20;
            MaximalTimeInS = 40;
            IsSameWeaponForAll = false;
            IsDropPreviousWeapon = false;
            IsSkipBots = true;
            NextRandomWeaponTimeStamp = DateTime.Now;
        }
    }
}
