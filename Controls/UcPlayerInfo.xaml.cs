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
using System.Globalization;

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
            uwsWeaponSelector.SetCheckedAll(false);
            uwsWeaponSelector.SetChecked(Player.Weapon.GetActiveWeaponId(), true);
        }

        private void btnWeaponDrop_Click(object sender, RoutedEventArgs e) {
            Player.DropWeapon();
        }

        private void uwsWeaponSelector_WeaponClick(WeaponId weaponId) {
            uwsWeaponSelector.SetCheckedAll(false);
            uwsWeaponSelector.SetChecked(weaponId, true);
            Player.EquipWeapon(weaponId);
        }

        private void txtPosition_LostFocus(object sender, RoutedEventArgs e) {
        }

        private void txtPosition_GotFocus(object sender, RoutedEventArgs e) {

        }

        private void chkPositionX_Checked(object sender, RoutedEventArgs e) {
            Player.Settings.FreezePositionSettings.Position.X = Player.Position.X;
        }

        private void chkPositionY_Checked(object sender, RoutedEventArgs e) {
            Player.Settings.FreezePositionSettings.Position.Y = Player.Position.Y;
        }

        private void chkPositionZ_Checked(object sender, RoutedEventArgs e) {
            Player.Settings.FreezePositionSettings.Position.Z = Player.Position.Z;
        }

        private void chkPoints_Checked(object sender, RoutedEventArgs e) {
            Player.Settings.FreezePointsSettings.Points = Player.Points;
        }
    }
}
