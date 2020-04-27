﻿using System;
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
        static TextBox InputTxt = new TextBox() { Width = 150, Height = 25 };

        public NamePlayers(int PlayerNum)
        {
            InitializeComponent();
            Title = "Player " + Convert.ToString(PlayerNum);
            ExplainTxt.Text = "Input a name for player " + PlayerNum;    

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

            string name;
            bool valid = false;

            do
            {
                name = InputTxt.Text;
                valid = true;

                if (name.Length < 1 || name.ToLower() == "anonymous")
                {
                    name = "Anonymous";
                    valid = true;
                }
                else if (PreviousNames.Contains(name))
                {
                    valid = false;
                    MessageBox.Show("Name already used by other player for this game. Please try again with a different name.");
                }

            } while (valid == false);

            NameWindow.myCanvas.Children.Remove(InputTxt);
            return name;
        }

        private void DoneBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}