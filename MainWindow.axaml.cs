using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SciLors_Mashed_Trainer.Controls;
using SciLors_Mashed_Trainer.Core;
using SciLors_Mashed_Trainer.Types;

namespace SciLors_Mashed_Trainer {
    public partial class MainWindow : Window {
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private Game? game;
        private readonly Player?[] players = new Player?[4];
        private readonly UcPlayerInfo[] playerInfos = new UcPlayerInfo[4];
        private bool forceMockMode = false;
        private Win32MemoryTarget? win32Target;
        private int idleTicks;

        public string ProgramVersion => "v0.2.0";
        public string ProgramName => "mashed-trainer";
        public string ProgramLongName => "SciLor's Mashed Trainer";
        public string ProgramTitle => $"{ProgramLongName} {ProgramVersion}";

        public MainWindow() {
            InitializeComponent();
            Title = ProgramTitle;
            InitializePlayerGrid();
            UpdateIconMenu();

            // On non-Windows platforms (e.g. Linux development/testing), default to Mock mode if no MFL process
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                forceMockMode = true;
            }

            tabMain.IsEnabled = false;
            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void InitializePlayerGrid() {
            var grid = this.FindControl<Grid>("grdPlayers");
            if (grid == null) return;
            grid.Children.Clear();

            for (int i = 0; i < playerInfos.Length; i++) {
                var playerInfo = new UcPlayerInfo {
                    Header = $"Player {i + 1}"
                };
                Grid.SetColumn(playerInfo, i);
                playerInfos[i] = playerInfo;
                grid.Children.Add(playerInfo);
            }
        }

        private void SetStatus(string message, bool isConnected) {
            var statusText = this.FindControl<TextBlock>("txtStatus");
            var statusLed = this.FindControl<Avalonia.Controls.Shapes.Ellipse>("elpStatusLed");
            if (statusText != null) statusText.Text = message;
            if (statusLed != null) {
                statusLed.Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse(isConnected ? "#00E676" : "#FFA000"));
            }
        }

        private void Timer_Tick(object? sender, EventArgs e) {
            var ucGameInfo = this.FindControl<UcGameInfo>("ucGameInfo");

            if (forceMockMode) {
                if (game == null) {
                    var mock = new MockMemoryTarget();
                    AttachToTarget(mock);
                    SetStatus("Running with Mock Game Target (Simulator)", true);
                } else {
                    game.Update();
                }
                return;
            }

            // Detached: look for MFL.exe once per second instead of every tick
            if (game == null && idleTicks++ % 20 != 0) return;

            Process[] proc = Process.GetProcessesByName("MFL");
            if (proc.Length == 1) {
                if (game == null) {
                    try {
                        win32Target = new Win32MemoryTarget(proc[0]);
                        AttachToTarget(win32Target);
                        SetStatus($"Attached to Mashed PID {proc[0].Id}", true);
                    } catch (Exception ex) {
                        SetStatus($"Failed to attach: {ex.Message}", false);
                    }
                } else {
                    game.Update();
                    var err = win32Target?.LastWriteError;
                    if (err != null) SetStatus(err, false);
                    else SetStatus($"Mashed Process PID: {proc[0].Id}", true);
                }
            } else if (proc.Length > 1) {
                SetStatus("Multiple MFL.exe processes found - close all but one.", false);
            } else if (game != null) {
                CleanUp();
                SetStatus("Mashed disconnected. Waiting for MFL.exe...", false);
            }
        }

        private void AttachToTarget(IMemoryTarget target) {
            var ucGameInfo = this.FindControl<UcGameInfo>("ucGameInfo");
            game = new Game(target);
            game.Update();
            tabMain.IsEnabled = true;

            foreach (Player.PlayerId playerId in Enum.GetValues<Player.PlayerId>()) {
                int id = (int)playerId;
                players[id] = new Player(game, playerId);
                playerInfos[id].Player = players[id];
            }

            if (ucGameInfo != null) {
                ucGameInfo.Game = game;
            }
        }

        public void AttachMockForTesting(MockMemoryTarget mockTarget) {
            AttachToTarget(mockTarget);
        }

        private void CleanUp() {
            var ucGameInfo = this.FindControl<UcGameInfo>("ucGameInfo");
            if (game != null) {
                foreach (var playerInfo in playerInfos) {
                    playerInfo.Player = null;
                }
                Array.Clear(players, 0, players.Length);
                if (ucGameInfo != null) ucGameInfo.Game = null;

                game.Dispose();
                game = null;
                win32Target = null;
                idleTicks = 0;
                tabMain.IsEnabled = false;
            }
        }

        protected override void OnClosing(WindowClosingEventArgs e) {
            timer.Stop();
            CleanUp();
            base.OnClosing(e);
        }

        private void mniExit_Click(object? sender, RoutedEventArgs e) {
            Close();
        }

        private void mniIcon_Click(object? sender, RoutedEventArgs e) {
            if (sender is MenuItem { Tag: string pack }) IconPack.Set(pack);
            UpdateIconMenu();
        }

        private void UpdateIconMenu() {
            foreach (var item in new[] { mniIconIngame, mniIconClassic })
                item.Icon = (string?)item.Tag == IconPack.Current ? new TextBlock { Text = "✓" } : null;
        }

        private void mniWebsite_Click(object? sender, RoutedEventArgs e) {
            OpenUrl("http://www.scilor.com/mashed-trainer.html");
        }

        private void mniDonate_Click(object? sender, RoutedEventArgs e) {
            OpenUrl("http://www.scilor.com/donate.html");
        }

        private void mniMockToggle_Click(object? sender, RoutedEventArgs e) {
            forceMockMode = !forceMockMode;
            CleanUp();
            var statusText = this.FindControl<TextBlock>("txtStatus");
            if (statusText != null) statusText.Text = forceMockMode ? "Mock mode enabled" : "Waiting for MFL.exe...";
        }

        private static void OpenUrl(string url) {
            try {
                Process.Start(new ProcessStartInfo {
                    FileName = url,
                    UseShellExecute = true
                });
            } catch {
                // Ignore browser launch failure on minimal environments
            }
        }
    }
}

