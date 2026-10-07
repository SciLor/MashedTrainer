using System;
using System.Collections.Generic;

namespace SciLors_Mashed_Trainer.Types.Weapons {
    public class WeaponHelper {
        private readonly Game game;
        private readonly NoWeapon noWeapon;
        private readonly Dictionary<IntPtr, Weapon> weaponCache = new Dictionary<IntPtr, Weapon>();

        public WeaponHelper(Game game) {
            this.game = game;
            this.noWeapon = new NoWeapon(game);
        }

        public Weapon GetWeapon(Player player) {
            IntPtr ptr = player.WeaponPointer;
            if (weaponCache.TryGetValue(ptr, out Weapon? weapon)) {
                return weapon;
            }
            weapon = InitWeapon(ptr);
            weaponCache[ptr] = weapon;
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
            if (MachineGun.GetValidPointers().Contains(pointer)) {
                return new MachineGun(game);
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
            return this.noWeapon;
        }
    }
}
