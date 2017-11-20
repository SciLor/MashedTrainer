using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SciLors_Mashed_Trainer.Types {
    public class GameFiles {
        private Game game;

        public string GamePath {
            get { return Path.GetDirectoryName(game.Process.Modules.MainModule.Path); }
        }

        public string PowerupTexturePath {
            get { return Path.Combine(GamePath, "TOASTART\\Common\\POWERUPS\\textures\\"); }
        }

        public string BoxDrum {
            get { return Path.Combine(PowerupTexturePath, "DepthCharge.png"); }
        }
        public string BoxFlamethrower {
            get { return Path.Combine(PowerupTexturePath, "FlameThrower.png"); }
        }
        public string BoxFlashbang {
            get { return Path.Combine(PowerupTexturePath, "Shine.bmp"); }
        }
        public string BoxMachineGun {
            get { return Path.Combine(PowerupTexturePath, "gattlingun.png"); }
        }
        public string BoxMine {
            get { return Path.Combine(PowerupTexturePath, "mine.png"); }
        }
        public string BoxMortar {
            get { return Path.Combine(PowerupTexturePath, "Mortar.png"); }
        }
        public string BoxRocket {
            get { return Path.Combine(PowerupTexturePath, "Missile.png"); }
        }
        public string BoxShotgun {
            get { return Path.Combine(PowerupTexturePath, "Shotgun.png"); }
        }
        public string BoxOil {
            get { return Path.Combine(PowerupTexturePath, "RPG.png"); }
        }

        public string Hazard {
            get { return Path.Combine(PowerupTexturePath, "Hazard.bmp"); }
        }

        public GameFiles(Game game) {
            this.game = game;
        }
    }
}
