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

namespace Project_Practice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    class GameConstants
    {
        public int[] MazeDimensions { get; set; } = new int[2];
        public int[] CellDimensions { get; set; } = new int[2];
        public int WallThickness { get; set; }

        public int[] mDimensionsDefault { get; } = new int[2] { 10, 10 };
        //default values for maze size (number of cells in the grid) if inputted values are invalid

        public int[] cDimensionsDefault { get; } = new int[2] { 20, 20 };
        //default values for cell size (in pixels) if inputted values are invalid

        public Brush BackgroundColour { get; } = Brushes.Black;
        public Brush ForegroundColour { get; } = Brushes.White;

        //SetMDimensions - REDUNDANT AT THE MOMENT
        //may be required if CheckDimensions is not used
        //private void SetMDimensions(int[] mDimensions)
        //{
        //    for (int i = 0; i < mDimensions.Length; i++)
        //    {
        //        if (mDimensions[i] < 1)
        //        {
        //            mDimensions[i] = this.mDimensionsDefault[i];
        //            //defaults to standard width if current dimensions are invalid
        //        }
        //        MazeDimensions[i] = mDimensions[i];
        //        //stores the dimensions as a separate array in this class
        //    }
        //}

        public GameConstants(int[] mazeDim, int[] cellDim)
        {
            MazeDimensions = CheckDimensions(mazeDim, 1, mDimensionsDefault);
            CellDimensions = CheckDimensions(cellDim, 11, cDimensionsDefault);
            //ensures that maze and cell dimensions are valid (i.e. int above 5)

            WallThickness = (int)(CellDimensions[0] / 10);
            //sets thickness of walls based on width of cells
        }

        private int[] CheckDimensions(int[] array, int minValue, int[] defaultValues)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] < minValue)
                {
                    array[i] = defaultValues[i];
                    //defaults to standard width if current dimensions are invalid
                }
            }

            return array;
            //returns the values so they can be stored in a variable
        }

    }

    class Cell
    {
        public Point GridPt { get; set; }

        private Rectangle shape = new Rectangle();

        public Cell(Point currentCell, int[] cellDimensions)
        {
            shape.Width = cellDimensions[0];
            shape.Height = cellDimensions[1];
            GridPt = currentCell;
        }

        //public List<Edge> Edges = new List<Edge>();

    }

    //class Edge
    //{
    //    Cell targetVertex;
    //}

    class Maze
    {
        /// <summary>
        /// GAME CONSTANTS
        /// </summary>

        public int[] MazeDimensions = new int[2];
        public int[] CellDimensions = new int[2];
        public int Thickness;

        private WallHorizontal[,] AllWallsH;                    ///additional walls on edge for each array to "close" box
        private WallVertical[,] AllWallsV;

        public Maze(GameConstants Constants)
        {
            ClearWindow(3);

            ///number of grid spaces in the maze
            MazeDimensions = Constants.MazeDimensions;
            CellDimensions = Constants.CellDimensions;
            Thickness = Constants.WallThickness;

            AllWallsH = new WallHorizontal[MazeDimensions[1] + 1, MazeDimensions[0]];
            AllWallsV = new WallVertical[MazeDimensions[1], MazeDimensions[0] + 1];

            initialiseWalls();
            drawGrid();
        }

        private void ClearWindow(int StartIndex)
        {
            Game.mw.myCanvas.Children.RemoveRange(StartIndex, Game.mw.myCanvas.Children.Count);

        }

        private void initialiseWalls()
        {
            //creates individuals for each wall in the map

            for (int i = 0; i < AllWallsH.GetLength(0); i++)
            {
                for (int j = 0; j < AllWallsH.GetLength(1); j++)
                {
                    AllWallsH[i, j] = new WallHorizontal(i, j, CellDimensions, MazeDimensions, Thickness);
                }
            }

            for (int i = 0; i < AllWallsV.GetLength(0); i++)
            {
                for (int j = 0; j < AllWallsV.GetLength(1); j++)
                {
                    AllWallsV[i, j] = new WallVertical(i, j, CellDimensions, MazeDimensions, Thickness);
                }
            }
        }

        public void drawGrid()
        {
            //displays all maze walls based on their unique x- and y-coordinates
            
            foreach (var wall in AllWallsV)
            {
                wall.draw();
            }

            foreach (var wall in AllWallsH)
            {
                wall.draw();
            }

        }

        public void MazeGeneration()
        {
            Point currentCell = new Point(0,0);
            Cell thisCell = new Cell(currentCell, CellDimensions);
        }

        public List<int> GetAdjacentDirections(Cell Current)
        {
            return new List<int>();
        }

    }

    class Wall
    {
        //protected allows use in derived Horizontal and Vertical Wall classes
        protected int width;                                                                        
        protected int height;
        protected Rectangle Shape = new Rectangle { Fill = Brushes.LightYellow, };
        protected int XCoord { get; set; }
        protected int YCoord { get; set; }
        protected bool hidden = false;
        protected bool hideable = true;

        public Wall()
        {
            Game.mw.myCanvas.Children.Add(Shape);
            Game.mw.myCanvas.MouseLeftButtonDown += MyCanvas_MouseLeftButtonDown;
        }

        private void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ///TESTING ability to remove particular walls
            //gets object that the user clicks on
            //hides the wall underneath click
            //if already hidden, makes it visible again

            if (e.Source == this.Shape && this.hidden == false)
            {
                this.hide();
                //MessageBox.Show("Clicked");           //testing click
            }
            else if (e.Source == this.Shape && this.hidden)
            {
                this.draw();
            }
        }
                                                                                                                                         
        public void hide()
        {
            if (hideable)
            {
                Shape.Opacity = 0;                  //hides wall when clicked         
                hidden = true;
            }
        }

        public void draw()
        {
            Shape.Opacity = 1;
            Canvas.SetLeft(Shape, XCoord);
            Canvas.SetTop(Shape, YCoord);
            hidden = false;
        }

    }

    class WallHorizontal : Wall
    {
        public WallHorizontal(int i, int j, int[] cellDimensions, int[] mazeDimensions, int thickness) : base()
        {
            width = cellDimensions[0] + thickness;
            height = thickness;
            Shape.Width = this.width;
            Shape.Height = this.height;

            //sets location of wall based on location in array

            XCoord = 40 + (j + 1) * cellDimensions[0] + thickness;
            YCoord = (i + 1) * cellDimensions[1];

            if (i == 0 || i == mazeDimensions[0])
            {
                //walls can be hidden unless they are on the outer edge of the maze
                hideable = false;
            }
        }
    }

    class WallVertical : Wall
    {
        public WallVertical(int i, int j, int[] cellDimensions, int[] mazeDimensions, int thickness) : base()
        {
            width = thickness;
            height = cellDimensions[1];
            Shape.Width = this.width;
            Shape.Height = this.height;

            //sets location of wall based on location in array

            XCoord = 40 + (j + 1) * cellDimensions[0] + thickness;
            YCoord = (i + 1) * cellDimensions[1];

            if (j == 0 || j == mazeDimensions[1])
            {
                //walls can be hidden unless they are on the outer edge of the maze
                hideable = false;
            }
        }
    }

    class Game
    {
        public static MainWindow mw { get; set; }                 ////allows access to canvas outside of main window
        private Maze MazeOne;
        private GameConstants Constants;

        public Game()
        {
            mw = (MainWindow)Application.Current.MainWindow;
            mw.myCanvas.Background = Brushes.Black;
        }

        public void CreateMaze(string[] mazeDimText, string[] cellDimText)
        {
            int[] mDimensions = new int[2];
            int[] cDimensions = new int[2];

            mDimensions = ConvertDimensionsToInt(mazeDimText);
            //gets size of maze from textboxes

            cDimensions = ConvertDimensionsToInt(cellDimText);
            //gets size of cell from text (can be amended to be from textboxes later)

            Constants = new GameConstants(mDimensions, cDimensions);
            MazeOne = new Maze(Constants);
        }

        public int[] ConvertDimensionsToInt(string[] inputTxtArray)
        {
            //procedure to take text and return the corresponding int
            //output used as dimensions for the maze
            //if input text is invalid (non-integer/0 or less), returns -1

            int[] outputArray = new int[inputTxtArray.Length];

            for (int i = 0; i < inputTxtArray.Length; i++)
            {
                int.TryParse(inputTxtArray[i], out outputArray[i]);

                if (outputArray[i] < 1)
                {
                    //if the text converts to a negative, or doesn't convert to an integer
                    outputArray[i] = -1;
                }
            }

            return outputArray;
            //returns -1 if text is invalid, and the actual int if valid
        }
    }

    public partial class MainWindow : Window
    {
        Game gameOne;

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            string[] mazeSizeTXT = new string[2] { widthTXT.Text, heightTXT.Text };
            string[] cellSizeTXT = new string[2] { "-1", "-1" };
            //default values - can be appended at later date to add text boxes for cell sizes

            gameOne.CreateMaze(mazeSizeTXT, cellSizeTXT);
        }

        public MainWindow()
        {
            InitializeComponent();
            gameOne = new Game();
        }
    }
}
