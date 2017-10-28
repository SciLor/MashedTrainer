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

namespace SciLors_Mashed_Trainer.Controls {
    /// <summary>
    /// Interaction logic for UcPlayerInfo.xaml
    /// </summary>
    public partial class UcPlayerInfo : UserControl {
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(String), typeof(UcPlayerInfo), new PropertyMetadata("Header Text"));
        public String Header {
            get { return (String)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty PlayerProperty =
            DependencyProperty.Register("Player", typeof(Player), typeof(UcPlayerInfo), new PropertyMetadata(null));
        public Player Player {
            get { return (Player)GetValue(PlayerProperty); }
            set { SetValue(PlayerProperty, value); }
        }

        public UcPlayerInfo() {
            InitializeComponent();
        }

        private void sldPoints_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            lblPoints.Content = sldPoints.Value + " / 8";
        }
    }
}
