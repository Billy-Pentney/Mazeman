using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Mazeman
{

    public partial class GameWindow : Window
    {
        private Game NewGame;

        public GameWindow(int[] MazeDimensions, bool TwoPlayers, double EnemyDifficulty, int EnemyColourIndex, bool DisableEnemies)
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            int MazeArea = MazeDimensions[0] * MazeDimensions[1];
            int NumOfEnemies = (int)Math.Floor(4 * MazeArea / 800.0f);

            if (DisableEnemies)
            {
                NumOfEnemies = 0;
            }

            BitmapImage WindowIconSource = new BitmapImage();
            WindowIconSource.BeginInit();
            WindowIconSource.UriSource = new Uri(GameConstants.SpriteFolderAddress + "/P1" + GameConstants.FileNameSuffixes[2]);
            WindowIconSource.EndInit();

            this.Icon = WindowIconSource;

            // Scales the window dimensions, proportional to the maze, with an indent and the width of 3 cells as spacing
            this.Width = GameConstants.WinIndent[0] + (MazeDimensions[0] + 3) * GameConstants.CellDimensions[0];
            this.Height = GameConstants.WinIndent[1] + (MazeDimensions[1] + 3) * GameConstants.CellDimensions[1];

            InfoTitleLbl.Foreground = GameConstants.ForegroundColour;
            ScoreLbl.Foreground = GameConstants.ForegroundColour;
            TimeLbl.Foreground = GameConstants.ForegroundColour;
            PowerupsBoxLbl.Foreground = GameConstants.ForegroundColour;
            GameCanvas.Background = GameConstants.BackgroundColour;

            SpeedUpLbl.Foreground = GameConstants.ForegroundColour;
            SpeedDownLbl.Foreground = GameConstants.ForegroundColour;
            FreezeLbl.Foreground = GameConstants.ForegroundColour;
            PointsLbl.Foreground = GameConstants.ForegroundColour;

            PowerupsKeyLbl.Foreground = GameConstants.ForegroundColour;
            
            // Scale the Information about the powerups, based on the height of the window
            PowerupInfoBlock.Height = this.Height - (3 * GameConstants.CellDimensions[1] + Canvas.GetTop(PowerupInfoBlock));

            AddPowerupShapesToKey();

            // Create a game with the given settings
            NewGame = new Game(this, MazeDimensions, TwoPlayers, EnemyDifficulty, EnemyColourIndex, NumOfEnemies);
        }

        // Handle user key presses - to change direction, or pause the game
        protected override void OnKeyDown(KeyEventArgs e)
        {
            NewGame.UpdatePlayerDirection(e.Key);

            if (e.Key == Key.Space)
            {
                NewGame.PauseByUser();
            }
        }

        // Display the powerups in the corresponding section of the left side-bar
        private void AddPowerupShapesToKey()
        {
            Image[] PowerupSprites = new Image[4];
            BitmapImage[] PowerupSpriteSources = new BitmapImage[4];

            for (int i = 0; i < PowerupSpriteSources.Length; i++)
            {
                // Load each powerup's image
                PowerupSpriteSources[i] = new BitmapImage();
                PowerupSpriteSources[i].BeginInit();
                PowerupSpriteSources[i].UriSource = new Uri(GameConstants.SpriteFolderAddress + "/Powerup-" + i + ".png");
                PowerupSpriteSources[i].EndInit();

                // Set the width/height of the image, proportional to the size of the maze cells
                double SpriteSize = GameConstants.CellDimensions[0] / 2;

                PowerupSprites[i] = new Image() { Width = SpriteSize, Height = SpriteSize };
                PowerupSprites[i].Source = PowerupSpriteSources[i];

                GameCanvas.Children.Add(PowerupSprites[i]);

                // Position the sprites in the side-bar as appropriate
                Canvas.SetLeft(PowerupSprites[i], 15);
                Canvas.SetTop(PowerupSprites[i], 147 + i * 20);
            }
        }

        private void MenuBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseGameReturnToMenu();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseGameReturnToMenu();
        }

        private void CloseGameReturnToMenu()
        {
            // Clear the bindings (Stop timers... etc.) in the current game
            NewGame.Destroy();

            // 
            SettingsWindow SW = new SettingsWindow();
            SW.Show();
        }
    }
}
