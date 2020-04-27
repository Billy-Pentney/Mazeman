using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace A_Level_Project__New_
{
    /// <summary>
    /// Interaction logic for NamePlayers.xaml
    /// </summary>
    public partial class NamePlayers : Window
    {
        static TextBox InputTxt = new TextBox() { Width = 150, Height = 25 };

        public NamePlayers(int PlayerNum)
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            SetColours();

            Title = "Player " + Convert.ToString(PlayerNum);
            ExplainTxt.Text = "Input a name for player " + PlayerNum;
            InputTxt.Text = "";

            if (!myCanvas.Children.Contains(InputTxt))
            {
                myCanvas.Children.Add(InputTxt);
            }

            Canvas.SetLeft(InputTxt, 10);
            Canvas.SetTop(InputTxt, 45);
        }

        public static string GetName(int PlayerNum, List<string> PreviousNames)
        {
            NamePlayers NameWindow = new NamePlayers(PlayerNum);
            NameWindow.ShowDialog();

            string name = InputTxt.Text;

            NameWindow.myCanvas.Children.Remove(InputTxt);
            return name;
        }

        private void DoneBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SetColours()
        {
            myCanvas.Background = GameConstants.BackgroundColour;
            ExplainTxt.Foreground = GameConstants.ForegroundColour;
            DoneBtn.Foreground = GameConstants.ForegroundColour;
            DoneBtn.Background = GameConstants.BackgroundColour;
            InputTxt.Background = GameConstants.BackgroundColour;
            InputTxt.Foreground = GameConstants.ForegroundColour;
        }
    }
}
