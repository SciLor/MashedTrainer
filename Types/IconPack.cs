using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SciLors_Mashed_Trainer.Types.Weapons;

namespace SciLors_Mashed_Trainer.Types {
    /// <summary>Weapon icon sets embedded as Img/packs/&lt;name&gt;/*.png. "ingame" is the default, "classic" the legacy set.</summary>
    public static class IconPack {
        public static readonly string[] Packs = { "ingame", "classic" };

        private static readonly Dictionary<Weapon.WeaponId, string> files = new() {
            { Weapon.WeaponId.Mortar, "Mortar" },
            { Weapon.WeaponId.Machinegun, "gattlingun" },
            { Weapon.WeaponId.Drum, "DepthCharge" },
            { Weapon.WeaponId.Rocket, "Missile" },
            { Weapon.WeaponId.Mines, "mine" },
            { Weapon.WeaponId.Flamethrower, "FlameThrower" },
            { Weapon.WeaponId.Shotgun, "Shotgun" },
            { Weapon.WeaponId.Flashbang, "Shine" },
            { Weapon.WeaponId.Oil, "Oil" }
        };

        private static readonly string settingsFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SciLorsMashedTrainer", "iconpack.txt");

        public static string Current { get; private set; } = Load();
        public static event Action? Changed;

        public static void Set(string pack) {
            if (pack == Current || Array.IndexOf(Packs, pack) < 0) return;
            Current = pack;
            try {
                Directory.CreateDirectory(Path.GetDirectoryName(settingsFile)!);
                File.WriteAllText(settingsFile, pack);
            } catch (Exception) { /* not persisted, still applied for this session */ }
            Changed?.Invoke();
        }

        public static Bitmap? Icon(Weapon.WeaponId id) => files.TryGetValue(id, out var name) ? Open(name) : null;

        /// <summary>Out-of-range warning shown in the player panel: always the classic icon, the in-game pack has none.</summary>
        public static Bitmap? Hazard() => Open("Hazard", "classic");

        private static Bitmap? Open(string name, string? pack = null) {
            try {
                return new Bitmap(AssetLoader.Open(new Uri($"avares://SciLorsMashedTrainer/Img/packs/{pack ?? Current}/{name}.png")));
            } catch (Exception) {
                return null;
            }
        }

        private static string Load() {
            try {
                var pack = File.ReadAllText(settingsFile).Trim();
                if (Array.IndexOf(Packs, pack) >= 0) return pack;
            } catch (Exception) { }
            return Packs[0];
        }
    }
}
