using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciLors_Mashed_Trainer.Types.Settings.Game {
    public class GameSettings {
        public DriveOverReviveSettings DriveOverReviveSettings { get; set; }
        public RandomWeaponSettings RandomWeaponSettings { get; set; }
        public WeaponBoxesSettings WeaponBoxesSettings { get; set; }

        public GameSettings() {
            DriveOverReviveSettings = new DriveOverReviveSettings();
            RandomWeaponSettings = new RandomWeaponSettings();
            WeaponBoxesSettings = new WeaponBoxesSettings();
        }
    }
}
