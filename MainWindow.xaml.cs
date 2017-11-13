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
using System.Windows.Threading;
using System.Diagnostics;
using SciLors_Mashed_Trainer.Controls;
using Binarysharp.Assemblers.Fasm;

namespace SciLors_Mashed_Trainer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        Grid playersGrid;
        DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();

        Game game;
        Player[] players = new Player[4];
        UcPlayerInfo[] playerInfos = new UcPlayerInfo[4];

        public MainWindow() {
            InitializeComponent();
            initializePlayerGrid();

            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timer.Start();

        }
        public void initializePlayerGrid() {
            playersGrid = (Grid)FindName("grdPlayers");
            playersGrid.Children.Clear(); 

            for (int i=0; i<playerInfos.Length; i++) {
                UcPlayerInfo playerInfo = new UcPlayerInfo();
                playerInfo.Header = "Player " + (i+1);
                playerInfo.SetValue(Grid.ColumnProperty, i);

                playerInfos[i] = playerInfo;
                playersGrid.Children.Add(playerInfo);
            }
        }
        
        private void timer_Tick(object sender, EventArgs e) {
            Process[] proc = Process.GetProcessesByName("MFL");
            if (proc.Length == 1) {
                if (game == null) {
                    game = new Game(proc[0]);
                    foreach (Player.PlayerId playerId in Enum.GetValues(typeof(Player.PlayerId))) {
                        int id = (int)playerId;
                        players[id] = new Player(game, playerId);
                        playerInfos[id].Player = players[id];
                    }
                    ucGameInfo.Game = game;
                } else {
                    game.Update();
                    txtStatus.Text = "Mashed Process Pointer: " + game.Process.Handle.DangerousGetHandle();
                }
            } else if (game != null) {
                cleanUp();
            }
        }

        private void cleanUp() {
            if (game != null) {
                foreach (UcPlayerInfo playerInfo in playerInfos) {
                    playerInfo.Player = null;
                }
                Array.Clear(players, 0, players.Length);
                ucGameInfo.Game = null;
                game = null;
                txtStatus.Text = "Mashed Process Pointer: 0";
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            timer.Stop();
            cleanUp();
        }
    }
}
