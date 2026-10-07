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

        public string ProgramVersion => "v0.2.0";
        public string ProgramName => "mashed-trainer";
        public string ProgramLongName => "SciLor's Mashed Trainer";
        public string ProgramTitle => $"{ProgramLongName} {ProgramVersion}";

        public MainWindow() {
            InitializeComponent();
            Title = ProgramTitle;
            InitializePlayerGrid();

            // On non-Windows platforms (e.g. Linux development/testing), default to Mock mode if no MFL process
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                forceMockMode = true;
            }

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

        private void Timer_Tick(object? sender, EventArgs e) {
            var statusText = this.FindControl<TextBlock>("txtStatus");
            var ucGameInfo = this.FindControl<UcGameInfo>("ucGameInfo");

            if (forceMockMode) {
                if (game == null) {
                    var mock = new MockMemoryTarget();
                    AttachToTarget(mock);
                    if (statusText != null) statusText.Text = "Running with Mock Game Target (Simulator)";
                } else {
                    game.Update();
                }
                return;
            }

            Process[] proc = Process.GetProcessesByName("MFL");
            if (proc.Length == 1) {
                if (game == null) {
                    try {
                        var win32Target = new Win32MemoryTarget(proc[0]);
                        AttachToTarget(win32Target);
                        if (statusText != null) statusText.Text = $"Attached to Mashed PID {proc[0].Id}";
                    } catch (Exception ex) {
                        if (statusText != null) statusText.Text = $"Failed to attach: {ex.Message}";
                    }
                } else {
                    game.Update();
                    if (statusText != null) statusText.Text = $"Mashed Process PID: {proc[0].Id}";
                }
            } else if (game != null) {
                CleanUp();
                if (statusText != null) statusText.Text = "Mashed disconnected. Waiting for MFL.exe...";
            }
        }

        private void AttachToTarget(IMemoryTarget target) {
            var ucGameInfo = this.FindControl<UcGameInfo>("ucGameInfo");
            game = new Game(target);

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

