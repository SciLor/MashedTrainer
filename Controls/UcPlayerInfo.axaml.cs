using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using SciLors_Mashed_Trainer.Types;
using static SciLors_Mashed_Trainer.Types.Weapons.Weapon;

namespace SciLors_Mashed_Trainer.Controls {
    public partial class UcPlayerInfo : UserControl {
        public static readonly StyledProperty<string> HeaderProperty =
            AvaloniaProperty.Register<UcPlayerInfo, string>(nameof(Header), "Player");

        public string Header {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly StyledProperty<Player?> PlayerProperty =
            AvaloniaProperty.Register<UcPlayerInfo, Player?>(nameof(Player));

        public Player? Player {
            get => GetValue(PlayerProperty);
            set {
                var old = GetValue(PlayerProperty);
                if (old != null) old.PropertyChanged -= Player_PropertyChanged;
                SetValue(PlayerProperty, value);
                if (value != null) {
                    value.PropertyChanged += Player_PropertyChanged;
                    var uws = this.FindControl<UcWeaponSelector>("uwsWeaponSelector");
                    if (uws != null) {
                        uws.Game = value.Game;
                    }
                }
            }
        }

        public UcPlayerInfo() {
            InitializeComponent();
            var warning = this.FindControl<Image>("imgWarning")!;
            warning.Source = IconPack.Hazard();
            AttachedToVisualTree += (_, _) => { IconPack.Changed += SetWarningIcon; SetWarningIcon(); };
            DetachedFromVisualTree += (_, _) => IconPack.Changed -= SetWarningIcon;
            void SetWarningIcon() => warning.Source = IconPack.Hazard();
        }

        private void Player_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var uws = this.FindControl<UcWeaponSelector>("uwsWeaponSelector");
            var imgWarning = this.FindControl<Image>("imgWarning");

            if (e.PropertyName == "Weapon" && Player != null && uws != null) {
                uws.SetCheckedAll(false);
                uws.SetChecked(Player.Weapon.GetActiveWeaponId(), true);
            } else if (e.PropertyName == "Distance" && Player != null && imgWarning != null) {
                float playerDistance = Player.Distance;
                float warningThreshold = Player.Game.DistanceWarningThreshold;
                float maxDistance = Player.Game.MaximumDistance;
                if (Player.Game.IsActive && Player.IsAlive) {
                    if (playerDistance > warningThreshold && maxDistance > warningThreshold) {
                        imgWarning.Opacity = Math.Clamp((playerDistance - warningThreshold) / (maxDistance - warningThreshold), 0.01, 1.0);
                    } else {
                        imgWarning.Opacity = 0.01;
                    }
                } else {
                    imgWarning.Opacity = 0.01;
                }
            }
        }

        private void btnWeaponDrop_Click(object? sender, RoutedEventArgs e) {
            Player?.DropWeapon();
        }

        private void uwsWeaponSelector_WeaponClick(WeaponId weaponId) {
            var uws = this.FindControl<UcWeaponSelector>("uwsWeaponSelector");
            if (uws != null && Player != null) {
                uws.SetCheckedAll(false);
                uws.SetChecked(weaponId, true);
                Player.EquipWeapon(weaponId);
            }
        }

        private void chkPositionX_IsCheckedChanged(object? sender, RoutedEventArgs e) {
            if (Player != null) Player.Settings.FreezePositionSettings.Position.X = Player.Position.X;
        }

        private void chkPositionY_IsCheckedChanged(object? sender, RoutedEventArgs e) {
            if (Player != null) Player.Settings.FreezePositionSettings.Position.Y = Player.Position.Y;
        }

        private void chkPositionZ_IsCheckedChanged(object? sender, RoutedEventArgs e) {
            if (Player != null) Player.Settings.FreezePositionSettings.Position.Z = Player.Position.Z;
        }

        private void chkPoints_IsCheckedChanged(object? sender, RoutedEventArgs e) {
            if (Player != null) Player.Settings.FreezePointsSettings.Points = Player.Points;
        }

        private void sldDamageFront_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e) {
            var lbl = this.FindControl<TextBlock>("lblDamageFront");
            if (lbl != null) lbl.Text = Math.Round(e.NewValue) + "%";
        }

        private void sldDamageBack_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e) {
            var lbl = this.FindControl<TextBlock>("lblDamageBack");
            if (lbl != null) lbl.Text = Math.Round(e.NewValue) + "%";
        }

        private void btnFlip_Click(object? sender, RoutedEventArgs e) {
            Player?.Flip();
        }

        private void btnTurn_Click(object? sender, RoutedEventArgs e) {
            Player?.Turn(180);
        }

        private void btnDamageRepair_Click(object? sender, RoutedEventArgs e) {
            Player?.Repair();
        }
    }
}

