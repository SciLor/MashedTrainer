using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciLors_Mashed_Trainer.Types.Weapons {
    public class Mine : Weapon {

        public Mine(Game game) : base(game) { }

        public static List<IntPtr> GetValidPointers() {
            return Weapons.Weapon.CalculateValidPointers(new IntPtr(0x6A8AF8), 0x18);
        }

        public override string ToString() { return "Mine"; }
    }
}
