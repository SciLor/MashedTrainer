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
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
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

        private IntPtr hMashed = IntPtr.Zero;
        private void timer_Tick(object sender, EventArgs e) {
            Process[] proc = Process.GetProcessesByName("MFL");
            if (proc.Length == 1) {
                if (hMashed == IntPtr.Zero) {
                    hMashed = BaseMemory.OpenProcess((uint)(
                        BaseMemory.ProcessAccessType.PROCESS_VM_READ |
                        BaseMemory.ProcessAccessType.PROCESS_VM_WRITE |
                        BaseMemory.ProcessAccessType.PROCESS_VM_OPERATION)
                        , 1, (uint)proc[0].Id);

                    game = new Game(hMashed);
                    foreach (Player.PlayerId playerId in Enum.GetValues(typeof(Player.PlayerId))) {
                        int id = (int)playerId;
                        players[id] = new Player(game, playerId);
                        playerInfos[id].Player = players[id];
                    }
                } else {
                    foreach (UcPlayerInfo playerInfo in playerInfos) {
                        //Redraw?!
                    }
                    foreach (Player player in players) {
                        player.RaisePropertyChanged();
                    }
                }
            } else if (hMashed != IntPtr.Zero) {
                foreach (UcPlayerInfo playerInfo in playerInfos) {
                    playerInfo.Player = null;
                }
                Array.Clear(players, 0, players.Length);
                game = null;

                BaseMemory.CloseHandle(hMashed);
                hMashed = IntPtr.Zero;
            }
            txtStatus.Text = "Mashed Process Pointer: " + hMashed.ToString();
        }
    }
}
