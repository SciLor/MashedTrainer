using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciLors_Mashed_Trainer.Types.Weapons {
    public class Mortar : Weapon {

        public Mortar(Game game) : base(game) { }

        public static List<IntPtr> GetValidPointers() {
            return Weapons.Weapon.CalculateValidPointers(new IntPtr(0x6A29B8), 0x1C);
        }

        public override string ToString() { return "Mortar"; }
    }
}
