using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.NUnit;
using NUnit.Framework;
using SciLors_Mashed_Trainer.Controls;
using SciLors_Mashed_Trainer.Core;
using SciLors_Mashed_Trainer.Types;
using SciLors_Mashed_Trainer.Types.Weapons;

namespace SciLors_Mashed_Trainer.Tests {
    [TestFixture]
    public class GuiMemorySyncTests {
        [AvaloniaTest]
        public void TestAllPlayerGuiElementsExistAndFunction() {
            var window = new MainWindow();
            var mock = new MockMemoryTarget();
            window.AttachMockForTesting(mock);
            window.Show();

            var grdPlayers = window.FindControl<Grid>("grdPlayers")!;
            Assert.That(grdPlayers, Is.Not.Null);
            Assert.That(grdPlayers.Children.Count, Is.EqualTo(4), "Must have 4 player cards");

            for (int i = 0; i < 4; i++) {
                var card = (UcPlayerInfo)grdPlayers.Children[i];
                Assert.That(card.Header, Is.EqualTo($"Player {i + 1}"));

                // 1. Basic group
                var sldPoints = card.FindControl<Slider>("sldPoints")!;
                var chkPoints = card.FindControl<CheckBox>("chkPoints")!;
                var chkAlive = card.FindControl<CheckBox>("chkAlive")!;
                var lblDistance = card.FindControl<TextBlock>("lblDistance")!;
                var imgWarning = card.FindControl<Image>("imgWarning")!;
                Assert.That(sldPoints, Is.Not.Null);
                Assert.That(chkPoints, Is.Not.Null);
                Assert.That(chkAlive, Is.Not.Null);
                Assert.That(lblDistance, Is.Not.Null);
                Assert.That(imgWarning, Is.Not.Null);

                // 2. Position group
                var txtX = card.FindControl<TextBox>("txtPositionX")!;
                var chkX = card.FindControl<CheckBox>("chkPositionX")!;
                var txtY = card.FindControl<TextBox>("txtPositionY")!;
                var chkY = card.FindControl<CheckBox>("chkPositionY")!;
                var txtZ = card.FindControl<TextBox>("txtPositionZ")!;
                var chkZ = card.FindControl<CheckBox>("chkPositionZ")!;
                var chkDisableControls = card.FindControl<CheckBox>("chkDisableControls")!;
                Assert.That(txtX, Is.Not.Null);
                Assert.That(chkX, Is.Not.Null);
                Assert.That(txtY, Is.Not.Null);
                Assert.That(chkY, Is.Not.Null);
                Assert.That(txtZ, Is.Not.Null);
                Assert.That(chkZ, Is.Not.Null);
                Assert.That(chkDisableControls, Is.Not.Null);

                // 3. Weapon group
                var lblWeapon = card.FindControl<TextBlock>("lblWeapon")!;
                var btnDrop = card.FindControl<Button>("btnWeaponDrop")!;
                var uws = card.FindControl<UcWeaponSelector>("uwsWeaponSelector")!;
                Assert.That(lblWeapon, Is.Not.Null);
                Assert.That(btnDrop, Is.Not.Null);
                Assert.That(uws, Is.Not.Null);

                // Check all 9 weapon toggle buttons exist
                Assert.That(uws.FindControl<ToggleButton>("btnMortar"), Is.Not.Null);
                Assert.That(uws.FindControl<ToggleButton>("btnMachinegun"), Is.Not.Null);
                Assert.That(uws.FindControl<ToggleButton>("btnDrum"), Is.Not.Null);
                Assert.That(uws.FindControl<ToggleButton>("btnRocket"), Is.Not.Null);
                Assert.That(uws.FindControl<ToggleButton>("btnMines"), Is.Not.Null);
                Assert.That(uws.FindControl<ToggleButton>("btnFlamethrower"), Is.Not.Null);
                Assert.That(uws.FindControl<ToggleButton>("btnShotgun"), Is.Not.Null);
                Assert.That(uws.FindControl<ToggleButton>("btnFlashbang"), Is.Not.Null);
                Assert.That(uws.FindControl<ToggleButton>("btnOil"), Is.Not.Null);

                // 4. Damage group
                var sldDmgFront = card.FindControl<Slider>("sldDamageFront")!;
                var lblDmgFront = card.FindControl<TextBlock>("lblDamageFront")!;
                var sldDmgBack = card.FindControl<Slider>("sldDamageBack")!;
                var lblDmgBack = card.FindControl<TextBlock>("lblDamageBack")!;
                var chkHood = card.FindControl<CheckBox>("chkDamageHood")!;
                var chkTrunk = card.FindControl<CheckBox>("chkDamageTrunk")!;
                var chkGlassHood = card.FindControl<CheckBox>("chkDamageGlassHood")!;
                var chkGlassTrunk = card.FindControl<CheckBox>("chkDamageGlassTrunk")!;
                var btnRepair = card.FindControl<Button>("btnDamageRepair")!;
                Assert.That(sldDmgFront, Is.Not.Null);
                Assert.That(lblDmgFront, Is.Not.Null);
                Assert.That(sldDmgBack, Is.Not.Null);
                Assert.That(lblDmgBack, Is.Not.Null);
                Assert.That(chkHood, Is.Not.Null);
                Assert.That(chkTrunk, Is.Not.Null);
                Assert.That(chkGlassHood, Is.Not.Null);
                Assert.That(chkGlassTrunk, Is.Not.Null);
                Assert.That(btnRepair, Is.Not.Null);

                // 5. Car physics group
                var btnFlip = card.FindControl<Button>("btnFlip")!;
                var btnTurn = card.FindControl<Button>("btnTurn")!;
                Assert.That(btnFlip, Is.Not.Null);
                Assert.That(btnTurn, Is.Not.Null);
            }

            window.Close();
        }

        [AvaloniaTest]
        public void TestAllGameTabGuiElementsExistAndFunction() {
            var window = new MainWindow();
            var mock = new MockMemoryTarget();
            window.AttachMockForTesting(mock);
            window.Show();

            var tabControl = window.FindControl<TabControl>("tabMain")!;
            tabControl.SelectedIndex = 1; // Game Tab
            window.UpdateLayout();

            var gameInfo = window.FindControl<UcGameInfo>("ucGameInfo")!;
            Assert.That(gameInfo, Is.Not.Null);

            // 1. Destroy Distance elements
            var txtMaxDist = gameInfo.FindControl<NumericUpDown>("txtMaxDistance")!;
            var txtWarnDist = gameInfo.FindControl<NumericUpDown>("txtWarnDistance")!;
            var btnResetDist = gameInfo.FindControl<Button>("btnResetDistance")!;
            Assert.That(txtMaxDist, Is.Not.Null);
            Assert.That(txtWarnDist, Is.Not.Null);
            Assert.That(btnResetDist, Is.Not.Null);

            // 2. Maximum Damage elements
            var sldMaxDmg = gameInfo.FindControl<Slider>("sldMaxDamage")!;
            var lblMaxDmg = gameInfo.FindControl<TextBlock>("lblMaxDamage")!;
            Assert.That(sldMaxDmg, Is.Not.Null);
            Assert.That(lblMaxDmg, Is.Not.Null);

            // 3. Camera elements
            var txtCameraTilt = gameInfo.FindControl<NumericUpDown>("txtCameraTiltMulti")!;
            var txtCameraHeightDiv = gameInfo.FindControl<NumericUpDown>("txtCameraHeightDistanceDiv")!;
            var txtCameraHeightAdd = gameInfo.FindControl<NumericUpDown>("txtCameraHeightDistanceAdd")!;
            var txtCameraHeightFactor = gameInfo.FindControl<NumericUpDown>("txtCameraHeightDistanceFactor")!;
            var txtCameraZoom = gameInfo.FindControl<NumericUpDown>("txtCameraZoomLimit")!;
            var btnResetCam = gameInfo.FindControl<Button>("btnResetCamera")!;
            Assert.That(txtCameraTilt, Is.Not.Null);
            Assert.That(txtCameraHeightDiv, Is.Not.Null);
            Assert.That(txtCameraHeightAdd, Is.Not.Null);
            Assert.That(txtCameraHeightFactor, Is.Not.Null);
            Assert.That(txtCameraZoom, Is.Not.Null);
            Assert.That(btnResetCam, Is.Not.Null);

            // 4. Random Weapon Equip elements
            var chkRandEnabled = gameInfo.FindControl<CheckBox>("chkWeaponDropEnabled")!;
            var txtRandMin = gameInfo.FindControl<NumericUpDown>("txtWeaponEquipMin")!;
            var txtRandMax = gameInfo.FindControl<NumericUpDown>("txtWeaponEquipMax")!;
            var uwsRand = gameInfo.FindControl<UcWeaponSelector>("uwsRandomWeapon")!;
            var chkRandSame = gameInfo.FindControl<CheckBox>("chkWeaponDropSame")!;
            var chkRandDrop = gameInfo.FindControl<CheckBox>("chkWeaponDropDrop")!;
            var chkRandSkip = gameInfo.FindControl<CheckBox>("chkWeaponDropSkipBots")!;
            Assert.That(chkRandEnabled, Is.Not.Null);
            Assert.That(txtRandMin, Is.Not.Null);
            Assert.That(txtRandMax, Is.Not.Null);
            Assert.That(uwsRand, Is.Not.Null);
            Assert.That(chkRandSame, Is.Not.Null);
            Assert.That(chkRandDrop, Is.Not.Null);
            Assert.That(chkRandSkip, Is.Not.Null);

            // 5. Drive Over Revive elements
            var chkReviveEnabled = gameInfo.FindControl<CheckBox>("chkReviveEnabled")!;
            var txtRespawnDist = gameInfo.FindControl<NumericUpDown>("txtRespawnDistance")!;
            var chkReviveRepair = gameInfo.FindControl<CheckBox>("chkReviveRepair")!;
            var chkReviveSkip = gameInfo.FindControl<CheckBox>("chkReviveSkipBots")!;
            Assert.That(chkReviveEnabled, Is.Not.Null);
            Assert.That(txtRespawnDist, Is.Not.Null);
            Assert.That(chkReviveRepair, Is.Not.Null);
            Assert.That(chkReviveSkip, Is.Not.Null);

            // 6. Weapon Boxes elements
            var chkBoxEnabled = gameInfo.FindControl<CheckBox>("chkWeaponBoxEnabled")!;
            var uwsBoxes = gameInfo.FindControl<UcWeaponSelector>("uwsWeaponboxes")!;
            var chkBoxSkip = gameInfo.FindControl<CheckBox>("chkWeaponBoxSkipBots")!;
            Assert.That(chkBoxEnabled, Is.Not.Null);
            Assert.That(uwsBoxes, Is.Not.Null);
            Assert.That(chkBoxSkip, Is.Not.Null);

            window.Close();
        }

        [AvaloniaTest]
        public void TestGuiToMemorySync() {
            var window = new MainWindow();
            var mock = new MockMemoryTarget();
            window.AttachMockForTesting(mock);
            window.Show();

            var grdPlayers = window.FindControl<Grid>("grdPlayers")!;
            var p1Card = (UcPlayerInfo)grdPlayers.Children[0];

            // 1. GUI -> Memory: Flip button
            var btnFlip = p1Card.FindControl<Button>("btnFlip")!;
            int p1MatrixOffset = 0x8B06E0 + 0x928;
            Assert.That(mock.Read<float>(new IntPtr(p1MatrixOffset + 0x14)), Is.EqualTo(1.0f).Within(0.01f));
            
            btnFlip.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Assert.That(mock.Read<float>(new IntPtr(p1MatrixOffset + 0x14)), Is.EqualTo(-1.0f).Within(0.01f));

            // 2. GUI -> Memory: Damage Repair button
            int p1DmgAddr = 0x65A9E8;
            mock.Write<float>(new IntPtr(p1DmgAddr + 0x00), 75.0f);
            mock.Write<bool>(new IntPtr(p1DmgAddr + 0x08), true);
            p1Card.Player!.Update();
            Assert.That(p1Card.Player.DamageFront, Is.EqualTo(75.0f));

            var btnRepair = p1Card.FindControl<Button>("btnDamageRepair")!;
            btnRepair.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Assert.That(mock.Read<float>(new IntPtr(p1DmgAddr + 0x00)), Is.EqualTo(0.0f));
            Assert.That(mock.Read<bool>(new IntPtr(p1DmgAddr + 0x08)), Is.False);

            // 3. GUI -> Memory: Points Slider
            var sldPoints = p1Card.FindControl<Slider>("sldPoints")!;
            sldPoints.Value = 7;
            Assert.That(mock.Read<int>(new IntPtr(0x8D8B40)), Is.EqualTo(7));

            // 4. GUI -> Memory: Alive CheckBox
            var chkAlive = p1Card.FindControl<CheckBox>("chkAlive")!;
            chkAlive.IsChecked = false;
            Assert.That(mock.Read<bool>(new IntPtr(0x8B06E0 + 0x004)), Is.False);

            // 5. GUI -> Memory: Position X change
            p1Card.Player.Position.X = 999.5f;
            Assert.That(mock.Read<float>(new IntPtr(p1MatrixOffset + 0x30)), Is.EqualTo(999.5f).Within(0.01f));

            // 6. GUI -> Memory: Weapon Selector Click
            var uws = p1Card.FindControl<UcWeaponSelector>("uwsWeaponSelector")!;
            var btnMortar = uws.FindControl<ToggleButton>("btnMortar")!;
            btnMortar.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
            Assert.That(mock.Read<int>(new IntPtr(0x8BEDCC)), Is.Not.EqualTo(0));

            window.Close();
        }

        [AvaloniaTest]
        public void TestMemoryToGuiSync() {
            var window = new MainWindow();
            var mock = new MockMemoryTarget();
            window.AttachMockForTesting(mock);
            window.Show();

            var grdPlayers = window.FindControl<Grid>("grdPlayers")!;
            var p1Card = (UcPlayerInfo)grdPlayers.Children[0];

            // 1. External game updates points in memory: 6
            mock.Write<int>(new IntPtr(0x8D8B40), 6);
            p1Card.Player!.Update();

            var sldPoints = p1Card.FindControl<Slider>("sldPoints")!;
            Assert.That(sldPoints.Value, Is.EqualTo(6));

            // 2. External game damages car in memory
            mock.Write<float>(new IntPtr(0x65A9E8), 42.0f);
            mock.Write<bool>(new IntPtr(0x65A9E8 + 0x08), true);
            p1Card.Player!.Update();

            var sldDamageFront = p1Card.FindControl<Slider>("sldDamageFront")!;
            var chkDamageHood = p1Card.FindControl<CheckBox>("chkDamageHood")!;
            Assert.That(sldDamageFront.Value, Is.EqualTo(42.0f).Within(0.01f));
            Assert.That(chkDamageHood.IsChecked, Is.True);

            // 3. External game moves car in memory
            int p1MatrixOffset = 0x8B06E0 + 0x928;
            mock.Write<float>(new IntPtr(p1MatrixOffset + 0x30), 123.45f);
            p1Card.Player!.Update();

            var txtPositionX = p1Card.FindControl<TextBox>("txtPositionX")!;
            Assert.That(float.Parse(txtPositionX.Text!), Is.EqualTo(123.45f).Within(0.01f));

            // 4. External game equips Rocket in memory
            mock.Write<int>(new IntPtr(0x8BEDCC), 0x6A5FF0);
            p1Card.Player!.Update();

            var uws = p1Card.FindControl<UcWeaponSelector>("uwsWeaponSelector")!;
            Assert.That(uws.IsChecked(Weapon.WeaponId.Rocket), Is.True);
            Assert.That(uws.IsChecked(Weapon.WeaponId.Machinegun), Is.False);

            window.Close();
        }
    }
}
