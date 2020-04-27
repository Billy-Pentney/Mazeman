using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;


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
        bool ClassicControls = true;

        double EnemyDifficulty;
        //used to set the speed of the enemy

        public SettingsWindow()
        {
            InitializeComponent();

            AddObjectsToWindow();

            DefaultCheck.IsChecked = true;
            MedRBtn.IsChecked = true;
            ClassicControlsCheckBox.IsChecked = true;
            //sets the default values for ease (simply press start)
        }

        private void AddObjectsToWindow()
        {
            int ShapeSize = 24;

            TextBlock ExplainControlsText = new TextBlock() { Width = 250, Height = 130 };
            ExplainControlsText.TextAlignment = TextAlignment.Justify;
            ExplainControlsText.TextWrapping = TextWrapping.Wrap;
            ExplainControlsText.Inlines.Add("Player 1 is controlled by the W,A,S,D keys." + Environment.NewLine);
            ExplainControlsText.Inlines.Add("Player 2 is controlled by the arrow keys." + Environment.NewLine + Environment.NewLine);
            ExplainControlsText.Inlines.Add("If Classic Controls are enabled, the players will only move while a key is pressed down." + Environment.NewLine);
            ExplainControlsText.Inlines.Add("If Classic Controls are disabled, the players continue to move in the direction of the last key press.");

            SettingsCanvas.Children.Add(ExplainControlsText);
            Canvas.SetBottom(ExplainControlsText, 50);
            Canvas.SetRight(ExplainControlsText, 30);

            #region Entity Shapes + Labels

            Ellipse[] EntityShapes = new Ellipse[3];
            Label[] EntityLabels = new Label[3];

            Label EntityText = new Label() { Width = 80, Height = 40, FontSize = 14, Content = "Players:" };
            SettingsCanvas.Children.Add(EntityText);
            Canvas.SetRight(EntityText, 170);
            Canvas.SetTop(EntityText, 20);

            for (int i = 0; i < EntityShapes.Length; i++)
            {
                EntityShapes[i] = new Ellipse()
                {
                    Width = ShapeSize,
                    Height = ShapeSize,
                    Fill = GameConstants.PlayerColours[i],
                };

                EntityLabels[i] = new Label()
                {
                    Width = 70,
                    Height = 30,
                };

                SettingsCanvas.Children.Add(EntityShapes[i]);
                Canvas.SetRight(EntityShapes[i], 240);
                Canvas.SetTop(EntityShapes[i], 65 + i * 50);

                SettingsCanvas.Children.Add(EntityLabels[i]);
                Canvas.SetRight(EntityLabels[i], 170);
                Canvas.SetTop(EntityLabels[i], 63 + i * 50);
            }

            EntityLabels[0].Content = "= Player 1";
            EntityLabels[1].Content = "= Player 2";
            EntityLabels[2].Content = "= Enemy";

            #endregion

            #region Powerup Shapes + Labels

            Rectangle[] PowerupShapes = new Rectangle[4];
            Label[] PowerupLabels = new Label[4];

            Label PowerupsText = new Label() { Width = 80, Height = 40, FontSize = 14, Content = "Powerups:"};
            SettingsCanvas.Children.Add(PowerupsText);
            Canvas.SetRight(PowerupsText, 40);
            Canvas.SetTop(PowerupsText, 20);

            for (int i = 0; i < PowerupShapes.Length; i++)
            {
                PowerupShapes[i] = new Rectangle()
                {
                    Width = ShapeSize / 3,
                    Height = ShapeSize / 3,
                    Fill = GameConstants.PowerUpColours[i],
                };

                PowerupLabels[i] = new Label()
                {
                    Width = 110,
                    Height = 40,
                };

                SettingsCanvas.Children.Add(PowerupShapes[i]);
                Canvas.SetRight(PowerupShapes[i], 125);
                Canvas.SetTop(PowerupShapes[i], 70 + i * 35);

                SettingsCanvas.Children.Add(PowerupLabels[i]);
                Canvas.SetRight(PowerupLabels[i], 15);
                Canvas.SetTop(PowerupLabels[i], 60 + i * 35);
            }

            PowerupLabels[0].Content = "= Speed Up";
            PowerupLabels[1].Content = "= Speed Down";
            PowerupLabels[2].Content = "= Freeze";
            PowerupLabels[3].Content = "= Points Multiplier";

            #endregion
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
                TwoPlayers = (bool)TwoPlayersCheck.IsChecked;
                ClassicControls = (bool)ClassicControlsCheckBox.IsChecked;
                new GameWindow(MazeDimensions, TwoPlayers, EnemyDifficulty, ClassicControls).Show();
                Close();
            }
            else
            {
                MessageBox.Show("Please input two whole numbers between 10 and 30");
            }
        }

        private void DefaultCheck_Checked(object sender, RoutedEventArgs e)
        {
            WidthTxt.Text = Convert.ToString(defMazeDimensions[0]);
            HeightTxt.Text = Convert.ToString(defMazeDimensions[1]);
        }

        #region DifficultyButtons

        private void EasyRBtn_Checked(object sender, RoutedEventArgs e)
        {
            EnemyDifficulty = GameConstants.Difficulties[0];
        }

        private void MedRBtn_Checked(object sender, RoutedEventArgs e)
        {
            EnemyDifficulty = GameConstants.Difficulties[1];
        }

        private void HardRBtn_Checked(object sender, RoutedEventArgs e)
        {
            EnemyDifficulty = GameConstants.Difficulties[2];
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
