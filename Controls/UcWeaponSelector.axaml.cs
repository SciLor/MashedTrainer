using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using SciLors_Mashed_Trainer.Types;
using SciLors_Mashed_Trainer.Types.Weapons;

namespace SciLors_Mashed_Trainer.Controls {
    public partial class UcWeaponSelector : UserControl {
        public static readonly StyledProperty<Game?> GameProperty =
            AvaloniaProperty.Register<UcWeaponSelector, Game?>(nameof(Game));

        public Game? Game {
            get => GetValue(GameProperty);
            set => SetValue(GameProperty, value);
        }

        public delegate void WeaponClickEventHandler(Weapon.WeaponId weaponId);
        public event WeaponClickEventHandler? WeaponClick;

        private readonly Dictionary<Weapon.WeaponId, ToggleButton> toggleButtons;

        public UcWeaponSelector() {
            InitializeComponent();

            toggleButtons = new Dictionary<Weapon.WeaponId, ToggleButton> {
                { Weapon.WeaponId.Mortar, this.FindControl<ToggleButton>("btnMortar")! },
                { Weapon.WeaponId.Machinegun, this.FindControl<ToggleButton>("btnMachinegun")! },
                { Weapon.WeaponId.Drum, this.FindControl<ToggleButton>("btnDrum")! },

                { Weapon.WeaponId.Rocket, this.FindControl<ToggleButton>("btnRocket")! },
                { Weapon.WeaponId.Mines, this.FindControl<ToggleButton>("btnMines")! },
                { Weapon.WeaponId.Flamethrower, this.FindControl<ToggleButton>("btnFlamethrower")! },

                { Weapon.WeaponId.Shotgun, this.FindControl<ToggleButton>("btnShotgun")! },
                { Weapon.WeaponId.Flashbang, this.FindControl<ToggleButton>("btnFlashbang")! },
                { Weapon.WeaponId.Oil, this.FindControl<ToggleButton>("btnOil")! }
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
            if (toggleButtons.TryGetValue(weaponId, out var btn))
                return btn.IsChecked;
            return null;
        }

        public void SetChecked(Weapon.WeaponId weaponId, bool isChecked) {
            if (toggleButtons.TryGetValue(weaponId, out var btn))
                btn.IsChecked = isChecked;
        }

        public List<Weapon.WeaponId> GetEnabledWeapons() {
            var weapons = new List<Weapon.WeaponId>();
            foreach (var item in toggleButtons) {
                if (item.Value.IsChecked == true)
                    weapons.Add(item.Key);
            }
            return weapons;
        }

        private void btnWeapon_Click(object? sender, RoutedEventArgs e) {
            if (sender is Control ctrl && ctrl.Tag is Weapon.WeaponId wid) {
                WeaponClick?.Invoke(wid);
            }
        }
    }
}

