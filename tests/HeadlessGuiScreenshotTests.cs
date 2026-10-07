using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using NUnit.Framework;
using SciLors_Mashed_Trainer;
using SciLors_Mashed_Trainer.Core;

namespace SciLors_Mashed_Trainer.Tests {
    [TestFixture]
    public class HeadlessGuiScreenshotTests {
        private string outputDir = null!;

        [SetUp]
        public void Setup() {
            outputDir = Path.Combine(AppContext.BaseDirectory, "screenshots");
            Directory.CreateDirectory(outputDir);
        }

        [AvaloniaTest]
        public void CaptureGuiScreenshots() {
            var window = new MainWindow();
            var mockTarget = new MockMemoryTarget();
            window.AttachMockForTesting(mockTarget);

            window.Width = 1330;
            window.Height = 700;
            window.Show();

            var tabControl = window.FindControl<TabControl>("tabMain");
            Assert.That(tabControl, Is.Not.Null, "TabControl tabMain must exist");

            // 1. Capture Players tab (Index 0)
            tabControl!.SelectedIndex = 0;
            window.UpdateLayout();

            string playersPng = Path.Combine(outputDir, "tab_Players.png");
            var frame0 = window.CaptureRenderedFrame();
            Assert.That(frame0, Is.Not.Null, "Rendered frame should not be null");
            using (frame0) {
                frame0!.Save(playersPng);
            }
            Assert.That(File.Exists(playersPng), Is.True, "tab_Players.png should be generated");
            TestContext.WriteLine($"Generated screenshot: {playersPng} ({new FileInfo(playersPng).Length} bytes)");

            // 2. Capture Game tab (Index 1)
            var ucGame = window.FindControl<Controls.UcGameInfo>("ucGameInfo");
            if (ucGame != null) {
                var rws = ucGame.FindControl<Controls.UcWeaponSelector>("uwsRandomWeapon");
                rws?.SetChecked(Types.Weapons.Weapon.WeaponId.Mortar, true);
                rws?.SetChecked(Types.Weapons.Weapon.WeaponId.Rocket, true);
                rws?.SetChecked(Types.Weapons.Weapon.WeaponId.Mines, true);

                var bws = ucGame.FindControl<Controls.UcWeaponSelector>("uwsWeaponboxes");
                bws?.SetChecked(Types.Weapons.Weapon.WeaponId.Machinegun, true);
                bws?.SetChecked(Types.Weapons.Weapon.WeaponId.Flamethrower, true);
            }

            tabControl.SelectedIndex = 1;
            window.UpdateLayout();

            string gamePng = Path.Combine(outputDir, "tab_Game.png");
            var frame1 = window.CaptureRenderedFrame();
            Assert.That(frame1, Is.Not.Null, "Rendered frame should not be null");
            using (frame1) {
                frame1!.Save(gamePng);
            }
            Assert.That(File.Exists(gamePng), Is.True, "tab_Game.png should be generated");
            TestContext.WriteLine($"Generated screenshot: {gamePng} ({new FileInfo(gamePng).Length} bytes)");

            window.Close();
        }
    }
}
