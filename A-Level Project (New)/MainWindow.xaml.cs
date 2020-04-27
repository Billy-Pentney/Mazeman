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
            Application.Current.ShutdownMode = ShutdownMode.OnLastWindowClose;
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
    }
}
