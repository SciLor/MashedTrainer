using SciLors_Mashed_Trainer.Types;
using SciLors_Mashed_Trainer.Types.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SciLors_Mashed_Trainer.Controls {
    /// <summary>
    /// Interaction logic for UcWeaponSelector.xaml
    /// </summary>
    public partial class UcWeaponSelector : UserControl {

        public static readonly DependencyProperty GameProperty =
            DependencyProperty.Register("Game", typeof(Game), typeof(UcWeaponSelector), new PropertyMetadata(null));
        public Game Game {
            get { return (Game)GetValue(GameProperty); }
            set {
                SetValue(GameProperty, value);
            }
        }

        public delegate void WeaponClickEventHandler(Weapon.WeaponId weaponId);

        public event WeaponClickEventHandler WeaponClick;
        private Dictionary<Weapon.WeaponId, ToggleButton> toggleButtons;

        public UcWeaponSelector() {
            InitializeComponent();

            toggleButtons = new Dictionary<Weapon.WeaponId, ToggleButton> {
                { Weapon.WeaponId.Mortar, btnMortar },
                { Weapon.WeaponId.Machinegun, btnMachinegun },
                { Weapon.WeaponId.Drum, btnDrum },

                { Weapon.WeaponId.Rocket, btnRocket },
                { Weapon.WeaponId.Mines, btnMines },
                { Weapon.WeaponId.Flamethrower, btnFlamethrower },

                { Weapon.WeaponId.Shotgun, btnShotgun },
                { Weapon.WeaponId.Flashbang, btnFlashbang },
                { Weapon.WeaponId.Oil, btnOil }
            };

            foreach (var pair in toggleButtons) {
                pair.Value.Tag = pair.Key;
            }
        }

        public void SetCheckedAll(bool isChecked) {
            foreach (ToggleButton button in toggleButtons.Values) {
                button.IsChecked = isChecked;
            }
        }

        public bool? IsChecked(Weapon.WeaponId weaponId) {
            if (toggleButtons.ContainsKey(weaponId))
                return toggleButtons[weaponId].IsChecked;
            return null;
        }
        public void SetChecked(Weapon.WeaponId weaponId, bool isChecked) {
            if (toggleButtons.ContainsKey(weaponId))
                toggleButtons[weaponId].IsChecked = isChecked;
        }

        public List<Weapon.WeaponId> GetEnabledWeapons() {
            List<Weapon.WeaponId> weapons = new List<Weapon.WeaponId>();

            foreach (var item in toggleButtons) {
                if (item.Value.IsChecked == true)
                    weapons.Add(item.Key);
            }

            return weapons;
        }

        private void btnWeapon_Click(object sender, RoutedEventArgs e) {
            WeaponClick?.Invoke((Weapon.WeaponId)((Control)sender).Tag);
        }
    }
}
