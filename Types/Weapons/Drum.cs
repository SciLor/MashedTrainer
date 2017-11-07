using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciLors_Mashed_Trainer.Types.Weapons {
    public class Drum : Weapon {

        public Drum(Game game) : base(game) { }

        public static List<IntPtr> GetValidPointers() {
            return Weapons.Weapon.CalculateValidPointers(new IntPtr(0x6A5C60), 0x2C);
        }

        public override string ToString() { return "Drum"; }
    }
}
