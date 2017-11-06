using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciLors_Mashed_Trainer.Types {
    public class Weapon {
        public enum WeaponId {
            None = 0x00,
            Mortar = 0x07,
            MaschineGun = 0x09,
            Drum = 0x0A,
            Rocket = 0x0B,
            Mines = 0x0C,
            Flamethrower = 0x10,
            Shotgun = 0x11,
            Flashbang = 0x12,
            Oil = 0x13,
        }
    }
}
