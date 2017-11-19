using SciLors_Mashed_Trainer.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SciLors_Mashed_Trainer.Controls {
    /// <summary>
    /// Interaction logic for UcGameInfo.xaml
    /// </summary>
    public partial class UcGameInfo : UserControl {
        public static readonly DependencyProperty GameProperty =
    DependencyProperty.Register("Game", typeof(Game), typeof(UcGameInfo), new PropertyMetadata(null));
        public Game Game {
            get { return (Game)GetValue(GameProperty); }
            set {
                SetValue(GameProperty, value);
                uwsRandomWeapon.Game = value;
                uwsWeaponboxes.Game = value;
                if (value != null) {
                    value.Settings.RandomWeaponSettings.WeaponSelector = uwsRandomWeapon;
                    value.Settings.WeaponBoxesSettings.WeaponSelector = uwsWeaponboxes;
                }
            }
        }

        public UcGameInfo() {
            InitializeComponent();
        }

        private void btnResetDistance_Click(object sender, RoutedEventArgs e) {
            Game.DistanceWarningThreshold = 7;
            Game.MaximumDistance = 10;
            Game.RaisePropertyChanged();
        }

        private void sldMaxDamage_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            lblMaxDamage.Content = Math.Round(sldMaxDamage.Value) + "%";
        }

        private void btnResetCamera_Click(object sender, RoutedEventArgs e) {
            Game.CameraTiltMultiplicator = 50;
            Game.CameraHeightDistanceDivider = 5;
            Game.CameraHeightDistanceAdd = 1;
        }
    }
}
