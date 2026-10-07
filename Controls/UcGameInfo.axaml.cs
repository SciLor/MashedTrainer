using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using SciLors_Mashed_Trainer.Types;

namespace SciLors_Mashed_Trainer.Controls {
    public partial class UcGameInfo : UserControl {
        public static readonly StyledProperty<Game?> GameProperty =
            AvaloniaProperty.Register<UcGameInfo, Game?>(nameof(Game));

        public Game? Game {
            get => GetValue(GameProperty);
            set {
                SetValue(GameProperty, value);
                var uwsRand = this.FindControl<UcWeaponSelector>("uwsRandomWeapon");
                var uwsBox = this.FindControl<UcWeaponSelector>("uwsWeaponboxes");
                if (uwsRand != null) uwsRand.Game = value;
                if (uwsBox != null) uwsBox.Game = value;
                if (value != null) {
                    if (uwsRand != null) value.Settings.RandomWeaponSettings.WeaponSelector = uwsRand;
                    if (uwsBox != null) value.Settings.WeaponBoxesSettings.WeaponSelector = uwsBox;
                }
            }
        }

        public UcGameInfo() {
            InitializeComponent();
        }

        private void btnResetDistance_Click(object? sender, RoutedEventArgs e) {
            if (Game != null) {
                Game.DistanceWarningThreshold = 7;
                Game.MaximumDistance = 10;
                Game.RaisePropertyChanged();
            }
        }

        private void sldMaxDamage_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e) {
            var lbl = this.FindControl<TextBlock>("lblMaxDamage");
            if (lbl != null) lbl.Text = Math.Round(e.NewValue) + "%";
        }

        private void btnResetCamera_Click(object? sender, RoutedEventArgs e) {
            if (Game != null) {
                Game.CameraTiltMultiplicator = 50;
                Game.CameraHeightDistanceDivider = 5;
                Game.CameraHeightDistanceAdd = 1;
                Game.CameraHeightDistanceFactor = 0.8f;
                Game.CameraZoomLimit = 10;
                Game.RaisePropertyChanged();
            }
        }
    }
}

