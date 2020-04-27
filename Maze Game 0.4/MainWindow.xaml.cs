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

        public const int NumOfDirections = 4;
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

        public Dictionary<int, Edge> Edges = new Dictionary<int, Edge>();

        public Point GetMazePt()
        {
            return GridPt;
        }

        public bool AddEdge(Edge newEdge, int Direction)
        {
            int numOfEdges = Edges.Count();

            if (!Edges.ContainsKey(Direction))
            {
                Edges.Add(Direction, newEdge);
                return true;
            }

            return false;
        }
    }

    class Edge
    {
        public Point StartPoint { get; set; }
        public Point TargetPoint { get; set; }

        public Edge(Point StartPt, Point EndPt)
        {
            StartPoint = StartPt;
            TargetPoint = EndPt;
        }
    }

    class Maze
    {
        private int[] MazeDimensions = new int[2];
        private int[] CellDimensions = new int[2];
        private int Thickness;

        private WallHorizontal[,] AllWallsH;
        private WallVertical[,] AllWallsV;

        private Cell[,] Cells;

        ///Maze Generation Recursive Variables
        private Cell thisCell;
        private List<int> ValidMoves;
        private int randMoveSelection;
        private int thisMove;
        private Point nextCellPt = new Point();
        private Edge thisEdge;
        private Edge reverseEdge;
        private Random rand = new Random();
        private Stack<Point> VisitedCells = new Stack<Point>();

        private List<int> ValidDirections = new List<int>();
        private List<int[]> AdjacentPositions;
        private Point cell;
        private int GetDirectionType = 0;
        private int NumOfCellsInPath;
        //

        public Maze(GameConstants Constants)
        {
            ClearWindow(11);

            ///number of grid spaces in the maze
            MazeDimensions = Constants.MazeDimensions;
            CellDimensions = Constants.CellDimensions;
            Thickness = Constants.WallThickness;

            AllWallsH = new WallHorizontal[MazeDimensions[0], MazeDimensions[1] + 1];
            AllWallsV = new WallVertical[MazeDimensions[0] + 1, MazeDimensions[1]];
            ///adds one on right/bottom edge for each array to "close" box

            Cells = new Cell[MazeDimensions[0], MazeDimensions[1]];

            InitialiseMaze();
            //creates instances of each wall/cell and sets their x/y coordinates
            DrawGrid();

            Point startPoint = new Point(0, 0);

            NumOfCellsInPath = MazeDimensions[0] * MazeDimensions[1] - 1;

            MazeGeneration(startPoint, GetDirectionType);

        }

        private void ClearWindow(int StartIndex)
        {
            Game.MW.myCanvas.Children.RemoveRange(StartIndex, Game.MW.myCanvas.Children.Count);
        }

        public bool IsValidCell(int[] location)
        {
            //determines if the passed coordinates are a valid point in the maze
            //takes int[2], returns bool

            bool valid = true;

            for (int i = 0; i < location.Count(); i++)
            {
                if (location[i] < 0 || location[i] >= MazeDimensions[i])
                {
                    valid = false;
                }
            }

            return valid;

        }

        private void InitialiseMaze()
        {
            //creates individuals for each wall in the map

            for (int y = 0; y < AllWallsH.GetLength(1); y++)
            {
                for (int x = 0; x < AllWallsH.GetLength(0); x++)
                {
                    AllWallsH[x, y] = new WallHorizontal(x, y, CellDimensions, MazeDimensions, Thickness);
                }
            }

            for (int y = 0; y < AllWallsV.GetLength(1); y++)
            {
                for (int x = 0; x < AllWallsV.GetLength(0); x++)
                {
                    AllWallsV[x, y] = new WallVertical(x, y, CellDimensions, MazeDimensions, Thickness);
                }
            }

            for (int y = 0; y < Cells.GetLength(1); y++)
            {
                for (int x = 0; x < Cells.GetLength(0); x++)
                {
                    Point CanvasPoint = AllWallsH[x, y].GetCoordinates();
                    Point MazePoint = new Point(x, y);

                    CanvasPoint.X += Thickness;
                    CanvasPoint.Y += Thickness;

                    Cells[x, y] = new Cell(MazePoint, CanvasPoint, CellDimensions);
                }
            }
        }

        private void DrawGrid()
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

        private void MazeGeneration(Point currentCellPt, int GetDirectionType)
        {
            ///RECURSIVE BACKRTRACKER
            ///Depth First Maze Generation Algorithm which uses recursive calls to step through cells

            //stores maze coordinates of maze cell currently being processed by algorithm
            thisCell = Cells[(int)currentCellPt.X, (int)currentCellPt.Y];

            ValidMoves = GetAdjacentDirections(thisCell.GetMazePt(), GetDirectionType);
            //gets all possible moves from the current cell to adjacent ones

            if (ValidMoves.Count > 0)
            {
                VisitedCells.Push(currentCellPt);

                randMoveSelection = rand.Next(0, ValidMoves.Count);
                thisMove = ValidMoves[randMoveSelection];
                //gets a random valid move from the current cell

                nextCellPt = MoveFromPoint(thisCell.GetMazePt(), thisMove);
                thisEdge = new Edge(currentCellPt, nextCellPt);

                Cells[(int)currentCellPt.X, (int)currentCellPt.Y].AddEdge(thisEdge, thisMove);
                //adds edge to the current cell

                reverseEdge = new Edge(thisEdge.TargetPoint, thisEdge.StartPoint);
                //flips the edge so its inverse can be stored in the other cell

                TurnOffWall(nextCellPt, thisMove);

                Cells[(int)nextCellPt.X, (int)nextCellPt.Y].AddEdge(reverseEdge, ReverseMove(thisMove));
                //adds opposite edge to the next cell

                if (VisitedCells.Count < NumOfCellsInPath)
                {
                    GetDirectionType = 0;
                    MazeGeneration(nextCellPt, GetDirectionType);
                }
            }
            else if (VisitedCells.Count > 0)// && VisitedCells.Count < NumOfCellsInPath)
            {
                GetDirectionType = 1;
                MazeGeneration(VisitedCells.Pop(), GetDirectionType);
                //Gets the last cell to be visited and recursively calls algorithm from that cell
            }
        }

        private void TurnOffWall(Point thisCell, int direction)
        {
            switch (direction)
            {
                case 0:
                    AllWallsH[(int)thisCell.X, (int)thisCell.Y + 1].Hide();
                    break;
                case 1:
                    AllWallsV[(int)thisCell.X, (int)thisCell.Y].Hide();
                    break;
                case 2:
                    AllWallsH[(int)thisCell.X, (int)thisCell.Y].Hide();
                    break;
                case 3:
                    AllWallsV[(int)thisCell.X + 1, (int)thisCell.Y].Hide();
                    break;
                default:
                    throw new Exception("Invalid direction when hiding wall");
            }
        }

        public int ReverseMove(int OriginalMove)
        {
            //takes int representing move (0, 1, 2, 3), 
            //0 = up, 1 = right, 2 = down, 3 = left

            //reverses move as follows : 0 -> 2, 1 -> 3, 2 -> 0, 3 -> 1

            return ((OriginalMove + 2) % GameConstants.NumOfDirections);

            //numOfDirections = 4, so    (0 + 2) % 4 = 2,   (1 + 2) % 4 = 3, 
            //                           (2 + 2) % 4 = 0,    (3 + 2) % 4 = 1,

        }

        private List<Point> GetAllUnvisitedCells()
        {
            List<Point> Unvisited = new List<Point>();
            Point thisCellPt = new Point();

            foreach (var cell in Cells)
            {
                thisCellPt = cell.GetMazePt();
                if (cell.Edges.Count == 0)
                {
                    Unvisited.Add(thisCellPt);
                }
            }

            return Unvisited;
        }

        private List<int> GetAdjacentDirections(Point CurrentLoc, int type)
        {
            //ValidDirections stores the directions 0, 1, 2, 3 representing possible movements from the current cell
            //ex. Cell[0,0] is top-left, so it has 1, 2 (right, down)
            //ex-2. Cell[0,1] is one right of [0,0], so it has 1, 2, 3 (right, down, left)
            //ex-3. Cell[1,1] is one below [0,1], so it has 0, 1, 2, 3 (all directions) 

            AdjacentPositions = new List<int[]>()
            {
                new int[2]{(int)CurrentLoc.X, (int)CurrentLoc.Y - 1},
                new int[2]{(int)CurrentLoc.X + 1, (int)CurrentLoc.Y},
                new int[2]{(int)CurrentLoc.X, (int)CurrentLoc.Y + 1},
                new int[2]{(int)CurrentLoc.X - 1, (int)CurrentLoc.Y},
            };

            ValidDirections.Clear();

            if (type == 0)
            {
                for (int i = 0; i < AdjacentPositions.Count; i++)
                {
                    cell = new Point(AdjacentPositions[i][0], AdjacentPositions[i][1]);

                    if (IsValidCell(AdjacentPositions[i]) && !VisitedCells.Contains(cell) && Cells[(int)CurrentLoc.X, (int)CurrentLoc.Y].Edges.Count < 2)
                    {
                        ValidDirections.Add(i);
                        //determines which adjacent cells have not already been explored
                        //creates list of valid directions from current cell
                    }
                }
            }
            else if (type == 1)
            {
                for (int i = 0; i < AdjacentPositions.Count; i++)
                {
                    cell = new Point(AdjacentPositions[i][0], AdjacentPositions[i][1]);

                    if (IsValidCell(AdjacentPositions[i]) && Cells[(int)cell.X, (int)cell.Y].Edges.Count == 0 && Cells[(int)CurrentLoc.X, (int)CurrentLoc.Y].Edges.Count < 3)
                    {
                        ValidDirections.Add(i);
                        //determines which 
                    }
                }
            }

            return ValidDirections;
        }

        public Point MoveFromPoint(Point startPt, int direction)
        {
            switch (direction)
            {
                case 0:
                    startPt.Y -= 1;
                    break;
                case 1:
                    startPt.X += 1;
                    break;
                case 2:
                    startPt.Y += 1;
                    break;
                case 3:
                    startPt.X -= 1;
                    break;
                default:
                    throw new Exception("Attempted to move in invalid direction");
            }

            bool validMove = IsValidCell(new int[2] { (int)startPt.X, (int)startPt.Y });

            if (validMove)
            {
                return startPt;
            }
            else
            {
                throw new Exception("Attempt to move to invalid point");
            }
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

            XCoord = GameConstants.indent + (i + 1) * cellDimensions[0] + thickness;
            YCoord = (j + 1) * cellDimensions[1];

            if (j == 0 || j == mazeDimensions[1])
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

            XCoord = GameConstants.indent + (i + 1) * cellDimensions[0] + thickness;
            YCoord = (j + 1) * cellDimensions[1];

            if (i == 0 || i == mazeDimensions[1])
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
