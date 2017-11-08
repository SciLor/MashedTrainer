using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciLors_Mashed_Trainer.Types.Weapons {
    public class MaschineGun : Weapon {

        public MaschineGun(Game game) : base(game) { }

        public static List<IntPtr> GetValidPointers() {
            return Weapons.Weapon.CalculateValidPointers(new IntPtr(0x6A60D0), 0x48);
        }

        public override WeaponId GetActiveWeaponId() {
            return Weapon.WeaponId.Maschinegun;
        }

        public override string ToString() { return "Maschine Gun"; }
    }
}
