using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciLors_Mashed_Trainer.Types.Weapons {
    public class Shotgun : Weapon {

        public Shotgun(Game game) : base(game) { }

        public static List<IntPtr> GetValidPointers() {
            return Weapons.Weapon.CalculateValidPointers(new IntPtr(0x6AAB80), 0x24);
        }

        public override string ToString() { return "Shotgun"; }
    }
}
