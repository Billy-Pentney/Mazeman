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
        public const int indent = 80;
        //indents map in from left of canvas

        public int[] MazeDimensions { get; set; } = new int[2];
        public int[] CellDimensions { get; set; } = new int[2];
        public int WallThickness { get; set; }

        private int minMazeDim = 10;
        private int minCellDim = 20;
        //minimum values for maze and cells that is accepted by the program


        public int[] mDimensionsDefault { get; } = new int[2] { 10, 10 };
        //default values for maze size (number of cells in the grid) if inputted values are invalid

        public int[] cDimensionsDefault { get; } = new int[2] { 20, 20 };
        //default values for cell size (in pixels) if inputted values are invalid

        public Brush BackgroundColour { get; } = Brushes.White;
        public Brush ForegroundColour { get; } = Brushes.Black;

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
            MazeDimensions = CheckDimensions(mazeDim, minMazeDim, mDimensionsDefault);
            CellDimensions = CheckDimensions(cellDim, minCellDim, cDimensionsDefault);
            //ensures that maze and cell dimensions are valid (i.e. int above 5)

            WallThickness = Convert.ToInt32(CellDimensions[0] / 10);
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
        private Point GridPt;
        private Point ShapePt;

        private Rectangle Shape = new Rectangle();

        public Cell(Point MazeLoc, Point CanvasLoc, int[] cellDimensions)
        {
            Shape.Width = cellDimensions[0];
            Shape.Height = cellDimensions[1];
            GridPt = MazeLoc;

            ShapePt = CanvasLoc;
        }

        public List<Edge> Edges = new List<Edge>();
    }

    class Edge
    {
        public Point StartPoint { get; set; }
        public Point TargetPoint { get; set; }
    }

    class Maze
    {
        private int[] MazeDimensions = new int[2];
        private int[] CellDimensions = new int[2];
        private int Thickness;

        private WallHorizontal[,] AllWallsH;                    
        private WallVertical[,] AllWallsV;

        private Cell[,] Cells;

        public Maze(GameConstants Constants)
        {
            ClearWindow(11);

            ///number of grid spaces in the maze
            MazeDimensions = Constants.MazeDimensions;
            CellDimensions = Constants.CellDimensions;
            Thickness = Constants.WallThickness;

            AllWallsH = new WallHorizontal[MazeDimensions[1] + 1, MazeDimensions[0]];
            AllWallsV = new WallVertical[MazeDimensions[1], MazeDimensions[0] + 1];
            ///add one on right/bottom edge for each array to "close" box

            Cells = new Cell[MazeDimensions[0], MazeDimensions[1]];

            InitialiseMaze();
            //creates instances of each wall/cell

            DrawGrid();
        }

        private void ClearWindow(int StartIndex)
        {
            Game.MW.myCanvas.Children.RemoveRange(StartIndex, Game.MW.myCanvas.Children.Count);
        }

        public bool CheckValidCell(int[] location)
        {
            //determines if the passed coordinates are a valid point in the maze
            //takes int[2], returns bool

            bool valid = false;

            for (int i = 0; i < location.Count(); i++)
            {
                if (location[i] > -1)
                {
                    if (location[i] < MazeDimensions[i])
                    {
                        valid = true;           
                    }
                }  
            }

            return valid;

        }

        private void InitialiseMaze()
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

            for (int i = 0; i < Cells.GetLength(0); i++)
            {
                for (int j = 0; j < Cells.GetLength(1); j++)
                {
                    Point CanvasPoint = AllWallsH[i, j].GetCoordinates();
                    Point MazePoint = new Point(j, i);

                    CanvasPoint.X += Thickness;
                    CanvasPoint.Y += Thickness;

                    Cells[i, j] = new Cell(MazePoint, CanvasPoint, CellDimensions);
                }
            }
        }

        public void DrawGrid()
        {
            //displays all maze walls based on their unique x- and y-coordinates
            
            foreach (var wall in AllWallsV)
            {
                wall.Draw();
            }

            foreach (var wall in AllWallsH)
            {
                wall.Draw();
            }

        }

        public void MazeGeneration()
        {
            int[] currentMazeCell = new int[2] { 0, 0 };
            Cell thisCell = Cells[currentMazeCell[0], currentMazeCell[1]];
        }

        public List<int> GetAdjacentDirections(Cell Current)
        {
            List<int> ValidDirections = new List<int>();
            //stores the directions 0, 1, 2, 3 representing possible movements from the current cell
            //ex. Cell[0,0] is top-left, so it has 1, 2 (right, down)
            //ex-2. Cell[0,1] is one right of [0,0], so it has 1, 2, 3 (right, down, left)
            //ex-3. Cell[1,1] is one below [0,1], so it has 0, 1, 2, 3 (all directions) 




            return ValidDirections;
        }

    }

    class Wall
    {
        //protected allows use in derived Horizontal and Vertical Wall classes
        protected int width;                                                                        
        protected int height;
        protected Rectangle Shape = new Rectangle { Fill = Brushes.Black, };
        protected int XCoord;
        protected int YCoord;
        protected bool hidden = false;                  //used to hide walls
        protected bool hideable = true;                 //used to prevent hiding outer edge walls

        public Wall()
        {
            Game.MW.myCanvas.Children.Add(Shape);
            Game.MW.myCanvas.MouseLeftButtonDown += MyCanvas_MouseLeftButtonDown;
        }

        public Point GetCoordinates()
        {
            return new Point(XCoord, YCoord);
        }

        protected void MyCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ///TESTING ability to remove particular walls
            //gets object that the user clicks on
            //hides the wall underneath the click
            //if already hidden, makes it visible again

            if (e.Source == this.Shape)
            {
                if (!this.hidden)
                {
                    this.Hide();
                }
                else
                {
                    this.Draw();
                }
            }
        }
                                                                                                                                         
        public void Hide()
        {
            if (hideable)
            {
                Shape.Opacity = 0;                  //hides wall when clicked         
                hidden = true;
            }
        }

        public void Draw()
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
            //parameters i and j represent location of wall in AllWallsH or AllWallsV

            width = cellDimensions[0] + thickness;
            height = thickness;
            Shape.Width = this.width;
            Shape.Height = this.height;

            //sets location of wall based on location in array

            XCoord = GameConstants.indent + (j + 1) * cellDimensions[0] + thickness;
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
            //parameters i and j represent location of wall in AllWallsH or AllWallsV

            width = thickness;
            height = cellDimensions[1];
            Shape.Width = this.width;
            Shape.Height = this.height;

            //sets location of wall based on location in array

            XCoord = GameConstants.indent + (j + 1) * cellDimensions[0] + thickness;
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
        public static MainWindow MW { get; set; }                 ////allows access to canvas outside of main window
        private Maze MazeOne;
        private GameConstants Constants;

        public Game()
        {
            MW = (MainWindow)Application.Current.MainWindow;
            //MW.myCanvas.Background = Brushes.Black;
        }

        public void CreateMaze(string[] mazeDimText, string[] cellDimText)
        {
            int[] mDimensions = ConvertDimensionsToInt(mazeDimText);
            //gets size of maze from textboxes

            int[] cDimensions = ConvertDimensionsToInt(cellDimText);
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
                if (!int.TryParse(inputTxtArray[i], out outputArray[i]))
                {
                    //if the input text doesn't convert to an integer
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

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            string[] mazeSizeTXT = new string[2] { MwidthTXT.Text, MheightTXT.Text };
            string[] cellSizeTXT = new string[2] { CwidthTXT.Text, CheightTXT.Text };
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
