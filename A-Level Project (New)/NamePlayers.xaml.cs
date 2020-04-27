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
    /// Interaction logic for NamePlayers.xaml
    /// </summary>
    public partial class NamePlayers : Window
    {
        static TextBox InputTxt = new TextBox() { Width = 50, };

        public NamePlayers(int PlayerNum)
        {
            InitializeComponent();
            Title = "Player " + Convert.ToString(PlayerNum);
            ExplainTxt.Text = "Input a name for player " + PlayerNum;
            myCanvas.Children.Add(InputTxt);
            Canvas.SetLeft(InputTxt, 10);
            Canvas.SetTop(InputTxt, 50);
        }

        public static string GetName(int PlayerNum)
        {
            NamePlayers NameWindow = new NamePlayers(PlayerNum);
            NameWindow.ShowDialog();
            return InputTxt.Text;
        }

        private void DoneBtn_Click(object sender, RoutedEventArgs e)
        {
            if (InputTxt.Text.Length > 0)
            {
                myCanvas.Children.Remove(InputTxt);
                Close();
            }
            else
            {
                MessageBox.Show("Please input a name to proceed");
            }
        }
    }
}
