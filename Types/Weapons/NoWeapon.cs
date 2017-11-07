using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciLors_Mashed_Trainer.Types.Weapons {
    public class NoWeapon : Weapon {

        public NoWeapon(Game game) : base(game) { }
        
        public override string ToString() { return "None"; }
    }
}
