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

namespace SciLors_Mashed_Trainer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();

        public MainWindow() {
            InitializeComponent();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Start();

            //Control baseGroup = (Control)FindName("grpP1");
            //baseGroup.Tag = 0;

        }
        public void ReadPlayer(Player player) {
            GetControl<Label>(player, "lbl", "Points").Content = String.Format("{0} / 8", player.Points);
            GetControl<Slider>(player, "sld", "Points").Value = player.Points;

            GetControl<TextBox>(player, "txt", "PositionX").Text = player.PosX + "";
            GetControl<TextBox>(player, "txt", "PositionY").Text = player.PosY + "";
            GetControl<TextBox>(player, "txt", "PositionZ").Text = player.PosZ + "";

        }
        public T GetControl<T>(Player player, string prefix, string suffix) {
            return (T)FindName(prefix + "P" + ((int)player.Id + 1) + suffix);
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
                } else {
                    Player p1 = new Player(hMashed, Player.PlayerId.ONE);
                    ReadPlayer(p1);
                }
            } else if (hMashed != IntPtr.Zero) {
                    BaseMemory.CloseHandle(hMashed);
                    hMashed = IntPtr.Zero;
            }
        }
    }
}
