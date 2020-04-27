using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace A_Level_Project__New_
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            BitmapImage WindowIconSource = new BitmapImage();
            WindowIconSource.BeginInit();
            WindowIconSource.UriSource = new Uri(Environment.CurrentDirectory + "/P1" + GameConstants.FileNameSuffixes[2]);
            WindowIconSource.EndInit();

            this.Icon = WindowIconSource;
            SetColours();
        }

        private void NewGameBtn_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow().Show();
            Close();
        }

        private void ViewHistoryBtn_Click(object sender, RoutedEventArgs e)
        {
            new ViewHistoryWindow().Show();

        }

        private void SwapColoursBtn_Click(object sender, RoutedEventArgs e)
        {
            GameConstants.SwapColours();
            SetColours();
        }

        private void SetColours()
        {
            Brush foreground = GameConstants.ForegroundColour;
            Brush background = GameConstants.BackgroundColour;

            MainCanvas.Background = background;

            NewGameBtn.Foreground = foreground;
            NewGameBtn.Background = background;

            ViewHistoryBtn.Foreground = foreground;
            ViewHistoryBtn.Background = background;

            SwapColoursBtn.Foreground = foreground;
            SwapColoursBtn.Background = background;

            VersionNumLbl.Foreground = foreground;
            IdentifierLbl.Foreground = foreground;
        }
    }
}
