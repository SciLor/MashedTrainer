using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciLors_Mashed_Trainer.Types.Weapons {
    public class Flashbang : Weapon {

        public Flashbang(Game game) : base(game) { }

        public static List<IntPtr> GetValidPointers() {
            return Weapons.Weapon.CalculateValidPointers(new IntPtr(0x6A5D78), 0x14);
        }

        public override WeaponId GetActiveWeaponId() {
            return Weapon.WeaponId.Flashbang;
        }

        public override string ToString() { return "Flashbang"; }
    }
}
