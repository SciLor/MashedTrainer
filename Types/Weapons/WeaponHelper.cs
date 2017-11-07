using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciLors_Mashed_Trainer.Types.Weapons {
    public class WeaponHelper {
        private Game game;
        private NoWeapon noWeapon;
        private Dictionary<IntPtr, Weapon> weaponCache = new Dictionary<IntPtr, Weapon>();

        public WeaponHelper(Game game) {
            this.game = game;
            this.noWeapon = new NoWeapon(game);
        }

        public Weapon GetWeapon(Player player) {
            IntPtr ptr = player.WeaponPointer;
            Weapon weapon;
            if (weaponCache.ContainsKey(ptr)) {
                weapon = weaponCache[ptr];
            } else {
                weapon = InitWeapon(ptr);
                weaponCache.Add(ptr, weapon);
            }
            return weapon;
        }
        private Weapon InitWeapon(IntPtr pointer) {
            if (Drum.GetValidPointers().Contains(pointer)) {
                return new Drum(game);
            }
            if (FlameThrower.GetValidPointers().Contains(pointer)) {
                return new FlameThrower(game);
            }
            if (Flashbang.GetValidPointers().Contains(pointer)) {
                return new Flashbang(game);
            }
            if (MaschineGun.GetValidPointers().Contains(pointer)) {
                return new MaschineGun(game);
            }
            if (Mine.GetValidPointers().Contains(pointer)) {
                return new Mine(game);
            }
            if (Mortar.GetValidPointers().Contains(pointer)) {
                return new Mortar(game);
            }
            if (Oil.GetValidPointers().Contains(pointer)) {
                return new Oil(game);
            }
            if (Rocket.GetValidPointers().Contains(pointer)) {
                return new Rocket(game);
            }
            if (Shotgun.GetValidPointers().Contains(pointer)) {
                return new Shotgun(game);
            }
            if (Drum.GetValidPointers().Contains(pointer)) {
                return new Drum(game);
            }
            return this.noWeapon;
        }
    }
}
