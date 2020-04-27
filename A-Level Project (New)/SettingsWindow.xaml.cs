using System;
using System.Windows;
using System.Windows.Controls;

namespace A_Level_Project__New_
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        int[] MazeDimRange = new int[2] { 10, 30 };
        //the range of values that are allowed to be inputted
        int[] defMazeDimensions = new int[2] { 15, 15 };
        //the default values to use if the user checks the box

        int[] MazeDimensions = new int[2];
        bool TwoPlayers = false;

        double[] difficulties = new double[3] { 1, 2, 3 };

        double EnemyDifficulty;               
        //used to set the speed of the enemy

        public SettingsWindow()
        {
            InitializeComponent();
            DefaultCheck.IsChecked = true;
            MedRBtn.IsChecked = true;
            //sets the default values for ease (simply press start)
        }

        private bool CheckText(string TextToCheck, int axis)
        {
            int intResult = -1;

            int.TryParse(TextToCheck, out intResult);

            if (intResult >= MazeDimRange[0] && intResult <= MazeDimRange[1])
            {
                MazeDimensions[axis] = intResult;
                return true;
            }

            return false;
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CheckText(WidthTxt.Text, 0) && CheckText(HeightTxt.Text, 1))
            {
                //MessageBox.Show("NEW GAME, size [" + MazeDimensions[0] + "," + MazeDimensions[1] + "]");
                TwoPlayers = (bool)TwoPlayersCheck.IsChecked;
                new GameWindow(MazeDimensions, TwoPlayers, EnemyDifficulty).Show();
                Close();
            }
            else
            {
                MessageBox.Show("Please input two whole numbers between 10 and 30");
            }
        }

        private void DefaultCheck_Checked(object sender, RoutedEventArgs e)
        {
            //if required, (bool)DefaultCheck.IsChecked returns whether that option is selected

            WidthTxt.Text = Convert.ToString(defMazeDimensions[0]);
            HeightTxt.Text = Convert.ToString(defMazeDimensions[1]);

            //WidthTxt.IsReadOnly = true;
            //HeightTxt.IsReadOnly = true;
            //sets the textboxes to be the default values
        }

        private void DefaultCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            //WidthTxt.Text = null;
            //HeightTxt.Text = null;

            //WidthTxt.IsReadOnly = false;
            //HeightTxt.IsReadOnly = false;
            //sets the textboxes to be empty
        }

        #region DifficultyButtons

        private void EasyRBtn_Checked(object sender, RoutedEventArgs e)
        {
            EnemyDifficulty = difficulties[0];
        }

        private void MedRBtn_Checked(object sender, RoutedEventArgs e)
        {
            EnemyDifficulty = difficulties[1];
        }

        private void HardRBtn_Checked(object sender, RoutedEventArgs e)
        {
            EnemyDifficulty = difficulties[2];
        }

        #endregion

        private void WidthTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (WidthTxt.Text != "15")
            {
                DefaultCheck.IsChecked = false;
            }
        }

        private void HeightTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (HeightTxt.Text != "15")
            {
                DefaultCheck.IsChecked = false;
            }
        }
    }
}
