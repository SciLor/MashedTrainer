using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciLors_Mashed_Trainer.Types.Weapons {
    public class FlameThrower : Weapon {

        public FlameThrower(Game game) : base(game) { }

        public static List<IntPtr> GetValidPointers() {
            return Weapons.Weapon.CalculateValidPointers(new IntPtr(0x6A9588), 0x68);
        }

        public override WeaponId GetActiveWeaponId() {
            return Weapon.WeaponId.Flamethrower;
        }

        public override string ToString() { return "Flame Thrower"; }
    }
}
