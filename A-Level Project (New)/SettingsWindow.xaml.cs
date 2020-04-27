using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace A_Level_Project__New_
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        int[] MazeDimRange = new int[2] { 5, 30 };
        //the range of values that are allowed to be inputted
        int[] defMazeDimensions = new int[2] { 15, 15 };
        //the default values to use if the user checks the box

        int[] MazeDimensions = new int[2];
        bool TwoPlayers = false;
        bool DisableEnemies = false;

        double EnemyDifficulty;
        //used to set the speed of the enemy

        Image[] EntitySpriteImages = new Image[3];
        BitmapImage[] EntitySpriteSources = new BitmapImage[3];
        Label[] EntityLabels = new Label[3];

        BitmapImage[] EnemyIMGSources = new BitmapImage[GameConstants.NumOfEnemyColours];
        int EnemyColourIndex = 0;

        public SettingsWindow()
        {
            InitializeComponent();

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            AddObjectsToWindow();
            //adds sprites and labels (i.e. the entities/powerups/scorepoints and the key the user sees)
            SetColours();

            BitmapImage SettingsIconSource = new BitmapImage();
            SettingsIconSource.BeginInit();
            SettingsIconSource.UriSource = new Uri(Environment.CurrentDirectory + "/SettingsIcon.png");
            SettingsIconSource.EndInit();
            this.Icon = SettingsIconSource;

            DefaultCheck.IsChecked = true;
            MedRBtn.IsChecked = true;
            //sets the default values for ease (simply press start)

            for (int i = 0; i < EnemyIMGSources.Length; i++)
            {
                EnemyIMGSources[i] = new BitmapImage();
                EnemyIMGSources[i].BeginInit();

                EnemyIMGSources[i].UriSource = new Uri(Environment.CurrentDirectory + "/Ghost" + (i + 1) + "-R.png");

                EnemyIMGSources[i].EndInit();
            }

            EntitySpriteImages.Last().Source = EnemyIMGSources[EnemyColourIndex];
        }

        private void SetColours()
        {
            Brush foregroundColour = GameConstants.ForegroundColour;
            Brush backgroundColour = GameConstants.BackgroundColour;

            SettingsCanvas.Background = backgroundColour;

            MazeDimExplainText.Foreground = foregroundColour;
            MazeDimExplainText.Background = backgroundColour;

            WidthLbl.Foreground = foregroundColour;
            HeightLbl.Foreground = foregroundColour;
            WidthTxt.Foreground = foregroundColour;
            WidthTxt.Background = backgroundColour;
            HeightTxt.Foreground = foregroundColour;
            HeightTxt.Background = backgroundColour;

            TwoPlayersCheck.Foreground = foregroundColour;
            DefaultCheck.Foreground = foregroundColour;

            ExclamationMarkLbl.Foreground = Brushes.Red;
            ExclamationMark2Lbl.Foreground = Brushes.Red;

            MazeDimText.Foreground = foregroundColour;
            MazeDimText.Background = backgroundColour;
            EnemyDifficultyText.Foreground = foregroundColour;
            EnemyDifficultyText.Background = backgroundColour;

            EasyLbl.Foreground = foregroundColour;
            MediumLbl.Foreground = foregroundColour;
            HardLbl.Foreground = foregroundColour;

            StartBtn.Background = backgroundColour;
            StartBtn.Foreground = foregroundColour;
            BackBtn.Foreground = foregroundColour;
            BackBtn.Background = backgroundColour;

            OtherOptionsText.Foreground = foregroundColour;
            OtherOptionsText.Background = backgroundColour;

            ControlsExplainText.Foreground = foregroundColour;

            PowerupsLbl.Foreground = foregroundColour;
            PlayersLbl.Foreground = foregroundColour;
            PointsLbl.Foreground = foregroundColour;
            ControlsLbl.Foreground = foregroundColour;

            DisableEnemiesChk.Foreground = foregroundColour;
        }

        private void AddObjectsToWindow()
        {
            #region Player Shapes + Labels

            int NumOfEntitySprites = 3;

            for (int i = 0; i < NumOfEntitySprites; i++)
            {
                EntitySpriteSources[i] = new BitmapImage();
                EntitySpriteSources[i].BeginInit();

                if (i < 2)
                {
                    //the player sprites are set here
                    EntitySpriteSources[i].UriSource = new Uri(Environment.CurrentDirectory + "/P" + (i + 1) + "-R.png");
                }
                else
                {
                    //the third, enemy sprite is set here
                    EntitySpriteSources[i].UriSource = new Uri(Environment.CurrentDirectory + "/Ghost1-R.png");
                }

                EntitySpriteSources[i].EndInit();

                EntitySpriteImages[i] = new Image();
                EntitySpriteImages[i].Source = EntitySpriteSources[i];

                EntityLabels[i] = new Label()
                {
                    Width = 70,
                    Height = 30,
                    Foreground = GameConstants.ForegroundColour,
                };

                SettingsCanvas.Children.Add(EntitySpriteImages[i]);
                Canvas.SetRight(EntitySpriteImages[i], 240);
                Canvas.SetTop(EntitySpriteImages[i], 52 + i * 50);

                SettingsCanvas.Children.Add(EntityLabels[i]);
                Canvas.SetRight(EntityLabels[i], 170);
                Canvas.SetTop(EntityLabels[i], 52 + i * 50);
            }

            EntityLabels[0].Content = "= Player 1";
            EntityLabels[1].Content = "= Player 2";
            EntityLabels[2].Content = "= Enemy";

            #endregion

            #region Powerup Shapes + Labels

            int NumOfPowerupSprites = 4;

            Image[] PowerupSpriteImages = new Image[NumOfPowerupSprites];
            BitmapImage[] PowerupSpriteSources = new BitmapImage[NumOfPowerupSprites];
            Label[] PowerupLabels = new Label[NumOfPowerupSprites];

            for (int i = 0; i < NumOfPowerupSprites; i++)
            {
                PowerupSpriteSources[i] = new BitmapImage();

                PowerupSpriteSources[i].BeginInit();
                PowerupSpriteSources[i].UriSource = new Uri(Environment.CurrentDirectory + "/Powerup-" + (i) + ".png");
                PowerupSpriteSources[i].EndInit();

                PowerupSpriteImages[i] = new Image() { Width = 20, Height = 20};
                PowerupSpriteImages[i].Source = PowerupSpriteSources[i];

                PowerupLabels[i] = new Label()
                {
                    Width = 110,
                    Height = 30,
                    Foreground = GameConstants.ForegroundColour,
                };

                SettingsCanvas.Children.Add(PowerupSpriteImages[i]);
                Canvas.SetRight(PowerupSpriteImages[i], 125);
                Canvas.SetTop(PowerupSpriteImages[i], 55 + i * 35);

                SettingsCanvas.Children.Add(PowerupLabels[i]);
                Canvas.SetRight(PowerupLabels[i], 15);
                Canvas.SetTop(PowerupLabels[i], 50 + i * 35);
                
            }

            PowerupLabels[0].Content = "= Speed Up";
            PowerupLabels[1].Content = "= Speed Down";
            PowerupLabels[2].Content = "= Freeze";
            PowerupLabels[3].Content = "= Points Multiplier";

            #endregion

            #region Point Shapes + Labels

            int NumOfPointSprites = 3;         
            ///specifies how many different images should be displayed for the points
            ///only change if more sprites are added

            Image[] PointSpriteImages = new Image[NumOfPointSprites];
            BitmapImage[] PointSpriteSources = new BitmapImage[NumOfPointSprites];
            Label[] PointLabels = new Label[NumOfPointSprites];

            for (int i = 0; i < NumOfPointSprites; i++)
            {
                PointSpriteSources[i] = new BitmapImage();

                PointSpriteSources[i].BeginInit();
                PointSpriteSources[i].UriSource = new Uri(Environment.CurrentDirectory + "/SP-" + i + ".png");
                PointSpriteSources[i].EndInit();

                PointSpriteImages[i] = new Image();
                PointSpriteImages[i].Source = PointSpriteSources[i];

                PointSpriteImages[i].Width = 20;
                PointSpriteImages[i].Height = 20;

                PointLabels[i] = new Label()
                {
                    Width = 70,
                    Height = 30,
                    Foreground = GameConstants.ForegroundColour,
                };

                SettingsCanvas.Children.Add(PointSpriteImages[i]);
                Canvas.SetRight(PointSpriteImages[i], 240);
                Canvas.SetTop(PointSpriteImages[i], 240 + i * 40);

                SettingsCanvas.Children.Add(PointLabels[i]);
                Canvas.SetRight(PointLabels[i], 170);
                Canvas.SetTop(PointLabels[i], 235 + i * 40);
                
            }

            PointLabels[0].Content = "= 0 Points";
            PointLabels[1].Content = "= 1 Point";
            PointLabels[2].Content = "= 2 Points";

            #endregion

            Image ControlsIMG = new Image();
            BitmapImage ControlsIMGSource = new BitmapImage();

            ControlsIMGSource.BeginInit();
            ControlsIMGSource.UriSource = new Uri(Environment.CurrentDirectory + "/ControlsIMG.png");
            ControlsIMGSource.EndInit();

            ControlsIMG.Source = ControlsIMGSource;

            ControlsIMG.Width = 130;
            ControlsIMG.Height = 130;

            SettingsCanvas.Children.Add(ControlsIMG);
            Canvas.SetRight(ControlsIMG, 10);
            Canvas.SetTop(ControlsIMG, 225);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Source == EntitySpriteImages.Last())
            {
                ChangeEnemyColour();
            }
        }

        private void ChangeEnemyColour()
        {
            EnemyColourIndex++;
            EnemyColourIndex = EnemyColourIndex % EnemyIMGSources.Length;
            EntitySpriteImages.Last().Source = EnemyIMGSources[EnemyColourIndex];
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

            DefaultCheck.IsChecked = (WidthTxt.Text == Convert.ToString(defMazeDimensions[0]) && HeightTxt.Text == Convert.ToString(defMazeDimensions[1]));

            return false;
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            DisableEnemies = (bool)DisableEnemiesChk.IsChecked;

            if (CheckText(WidthTxt.Text, 0) && CheckText(HeightTxt.Text, 1))
            {
                TwoPlayers = (bool)TwoPlayersCheck.IsChecked;
                new GameWindow(MazeDimensions, TwoPlayers, EnemyDifficulty, EnemyColourIndex + 1, DisableEnemies).Show();
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

        private void DefaultCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            WidthTxt.Text = "";
            HeightTxt.Text = "";
        }

        #region DifficultyButtons

        private void EasyRBtn_Checked(object sender, RoutedEventArgs e)
        {
            EnemyDifficulty = GameConstants.difficulties[0];
        }

        private void MedRBtn_Checked(object sender, RoutedEventArgs e)
        {
            EnemyDifficulty = GameConstants.difficulties[1];
        }

        private void HardRBtn_Checked(object sender, RoutedEventArgs e)
        {
            EnemyDifficulty = GameConstants.difficulties[2];
        }

        #endregion

        private void WidthTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CheckText(WidthTxt.Text, 0))
            {
                ExclamationMarkLbl.Foreground = Brushes.Green;
                ExclamationMarkLbl.Content = "✓";
            }
            else
            {
                ExclamationMarkLbl.Foreground = Brushes.Red;
                ExclamationMarkLbl.Content = "X";
            }
        }

        private void HeightTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CheckText(HeightTxt.Text, 1))
            {
                ExclamationMark2Lbl.Foreground = Brushes.Green;
                ExclamationMark2Lbl.Content = "✓";
            }
            else
            {
                ExclamationMark2Lbl.Foreground = Brushes.Red;
                ExclamationMark2Lbl.Content = "X";
            }
        }
    }
}