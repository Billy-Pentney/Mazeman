using System;
using System.Windows.Media;

namespace Mazeman
{
    /// <summary>
    ///     A container for the global constants used throughout the program.
    /// </summary>
     
    static class GameConstants
    {
        public static int[] CellDimensions { get; } = new int[2] { 25, 25 };

        // Width of the maze walls, as a fraction of the cell size
        public static double WallThicknessProportion = 0.1;

        // The pixel values describing the distance between the maze and the top-left corner of the window
        // [0] is horizontal, [1] is vertical
        public static int[] WinIndent { get; } = new int[2] { 85, 20 };
        public static int[] MazeIndent { get; } = new int[2] { 85, 0 };

        // The name/address of the file where scores should be written to/read from
        public const string FileName = "Data/History.txt";

        // Difficulty corresponds to the movement speed of both players (higher is more difficult)
        public static double[] Difficulties { get; } = new double[] { 1, 2, 3 };

        // Colour for the main background of the game window
        public static Brush BackgroundColour { get; set; } = Brushes.Black;

        // Colour for the main background of the game windows
        public static Brush ForegroundColour { get; set; } = Brushes.White;
        public static Brush[] WallColours { get; set; } = new Brush[] { Brushes.Red, Brushes.Black };

        // Suffixes added to the end of the Character Sprite Filenames, corresponding to each direction
        //  e.g. Suffixes[0] is the filename for the Up image
        public static string[] FileNameSuffixes { get; } = new string[] { "-U.png", "-R.png", "-D.png", "-L.png" };

        public static string SpriteFolderAddress { get; } = Environment.CurrentDirectory + "/Data/Sprites";

        public const int NumOfUniquePowerupTypes = 4;

        public const int NumOfEnemyColours = 5;

        // Swaps the background/foreground colours to switch between light/dark theme
        public static void SwapColours()
        {
            Brush temporary = BackgroundColour;
            BackgroundColour = ForegroundColour;
            ForegroundColour = temporary;

            temporary = WallColours[0];
            WallColours[0] = WallColours[1];
            WallColours[1] = temporary;
        }
    }
}
