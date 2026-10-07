using System;
using System.IO;

namespace SciLors_Mashed_Trainer.Types {
    public class GameFiles {
        private readonly Game game;

        public string GamePath {
            get {
                return AppContext.BaseDirectory;
            }
        }

        public string PowerupTexturePath => Path.Combine(GamePath, "Img");

        public string BoxDrum => Path.Combine(PowerupTexturePath, "DepthCharge.png");
        public string BoxFlamethrower => Path.Combine(PowerupTexturePath, "FlameThrower.png");
        public string BoxFlashbang => Path.Combine(PowerupTexturePath, "Shine.png");
        public string BoxMachineGun => Path.Combine(PowerupTexturePath, "gattlingun.png");
        public string BoxMine => Path.Combine(PowerupTexturePath, "mine.png");
        public string BoxMortar => Path.Combine(PowerupTexturePath, "Mortar.png");
        public string BoxRocket => Path.Combine(PowerupTexturePath, "Missile.png");
        public string BoxShotgun => Path.Combine(PowerupTexturePath, "Shotgun.png");
        public string BoxOil => Path.Combine(PowerupTexturePath, "RPG.png");
        public string Hazard => Path.Combine(PowerupTexturePath, "Hazard.png");

        public GameFiles(Game game) {
            this.game = game;
        }
    }
}
