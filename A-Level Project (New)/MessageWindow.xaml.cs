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
using System.Windows.Shapes;

namespace A_Level_Project__New_
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        public MessageWindow(string message)
        {
            InitializeComponent();
            this.Background = GameConstants.BackgroundColour;
            MessageTxtBox.Background = GameConstants.BackgroundColour;
            MessageTxtBox.Foreground = GameConstants.ForegroundColour;
            ContinueBtn.Background = GameConstants.BackgroundColour;
            ContinueBtn.Foreground = GameConstants.ForegroundColour;

            MessageTxtBox.Text = message;
        }

        private void ContinueBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
