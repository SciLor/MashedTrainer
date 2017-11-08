using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SciLors_Mashed_Trainer.Types;
using static SciLors_Mashed_Trainer.Types.Weapons.Weapon;

namespace SciLors_Mashed_Trainer.Controls {
    /// <summary>
    /// Interaction logic for UcPlayerInfo.xaml
    /// </summary>
    public partial class UcPlayerInfo : UserControl {
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(String), typeof(UcPlayerInfo), new PropertyMetadata("Header Text"));
        public String Header {
            get { return (String)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty PlayerProperty =
            DependencyProperty.Register("Player", typeof(Player), typeof(UcPlayerInfo), new PropertyMetadata(null));
        public Player Player {
            get { return (Player)GetValue(PlayerProperty); }
            set {
                SetValue(PlayerProperty, value);
                if(value != null)
                    value.PropertyChanged += Player_PropertyChanged;
            }
        }

        public UcPlayerInfo() {
            InitializeComponent();
        }

        private void Player_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "Weapon") {
                SetWeaponButtons();
            } else if (e.PropertyName == "Distance" ) {
                float playerDistance = Player.Distance;
                float warningThreshold = Player.Game.DistanceWarningThreshold;
                float maxDistance = Player.Game.MaximumDistance;
                if (Player.Game.IsActive && Player.IsAlive) {
                    if (playerDistance == 0) {
                        grpBase.Background = new SolidColorBrush(Color.FromArgb(10, 0, 255, 0));
                        imgWarning.Opacity = 0;
                    } else {
                        byte green = (byte)(255 - ((playerDistance) / (maxDistance)) * 255);
                        grpBase.Background = new SolidColorBrush(Color.FromArgb(10, 255, green, 0));
                        if (playerDistance > warningThreshold) {
                            imgWarning.Opacity = (playerDistance - warningThreshold) / (maxDistance - warningThreshold);
                        } else {
                            imgWarning.Opacity = 0.01;
                        }
                    }
                } else {
                    grpBase.Background = null;
                    imgWarning.Opacity = 0.01;
                }
            }
        }

        private void SetWeaponButtons() {
            btnMortar.Opacity = 0.25;
            btnMachinegun.Opacity = 0.25;
            btnDrum.Opacity = 0.25;
            btnRocket.Opacity = 0.25;
            btnMines.Opacity = 0.25;
            btnFlamethrower.Opacity = 0.25;
            btnShotgun.Opacity = 0.25;
            btnFlashbang.Opacity = 0.25;
            btnOil.Opacity = 0.25;

            switch (Player.Weapon.GetActiveWeaponId()) {
                case WeaponId.Mortar:
                    btnMortar.Opacity = 1;
                    break;
                case WeaponId.Maschinegun:
                    btnMachinegun.Opacity = 1;
                    break;
                case WeaponId.Drum:
                    btnDrum.Opacity = 1;
                    break;
                case WeaponId.Rocket:
                    btnRocket.Opacity = 1;
                    break;
                case WeaponId.Mines:
                    btnMines.Opacity = 1;
                    break;
                case WeaponId.Flamethrower:
                    btnFlamethrower.Opacity = 1;
                    break;
                case WeaponId.Shotgun:
                    btnShotgun.Opacity = 1;
                    break;
                case WeaponId.Flashbang:
                    btnFlashbang.Opacity = 1;
                    break;
                case WeaponId.Oil:
                    btnOil.Opacity = 1;
                    break;
            }
        }

        private void sldPoints_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            lblPoints.Content = sldPoints.Value + " / 8";
        }

        private void btnMortar_Click(object sender, RoutedEventArgs e) {
            Player.EquipWeapon(WeaponId.Mortar);
        }

        private void btnMachinegun_Click(object sender, RoutedEventArgs e) {
            Player.EquipWeapon(WeaponId.Maschinegun);
        }

        private void btnDrum_Click(object sender, RoutedEventArgs e) {
            Player.EquipWeapon(WeaponId.Drum);
        }

        private void btnRocket_Click(object sender, RoutedEventArgs e) {
            Player.EquipWeapon(WeaponId.Rocket);
        }

        private void btnMines_Click(object sender, RoutedEventArgs e) {
            Player.EquipWeapon(WeaponId.Mines);
        }

        private void btnFlamethrower_Click(object sender, RoutedEventArgs e) {
            Player.EquipWeapon(WeaponId.Flamethrower);
        }

        private void btnShotgun_Click(object sender, RoutedEventArgs e) {
            Player.EquipWeapon(WeaponId.Shotgun);
        }

        private void btnFlashbang_Click(object sender, RoutedEventArgs e) {
            Player.EquipWeapon(WeaponId.Flashbang);
        }

        private void btnOil_Click(object sender, RoutedEventArgs e) {
            Player.EquipWeapon(WeaponId.Oil);
        }

        private void btnWeaponDrop_Click(object sender, RoutedEventArgs e) {
            Player.DropWeapon();
        }
    }
}
