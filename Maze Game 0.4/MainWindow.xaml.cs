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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Priority_Queue;

namespace Project_Practice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    class GameConstants
    {
        public const int indent = 80;
        //indents map in from left of canvas
        public const int NumOfDirections = 4;
        //defines number of directions that are possible in maze

        public int[] MazeDimensions { get; set; } = new int[2];
        public int[] CellDimensions { get; set; } = new int[2];
        public int WallThickness { get; set; }

        private int minMazeDim = 10;
        private int minCellDim = 20;
        private int maxMazeDim = 30;
        private int maxCellDim = 30;
        //minimum values for maze and cells that is accepted by the program

        private int[] mDimensionsDefault;
        private int[] cDimensionsDefault;

        public Brush BackgroundColour { get; } = Brushes.White;
        public Brush ForegroundColour { get; } = Brushes.Black;

        public GameConstants(int[] mazeDim, int[] cellDim)
        {
            mDimensionsDefault = new int[] { minMazeDim, minMazeDim };
            cDimensionsDefault = new int[] { minCellDim, minCellDim };
            //sets default values for maze size (number of cells in the grid) 
            //and cell size (in pixels) if inputted values are invalid

            MazeDimensions = CheckDimensions(mazeDim, minMazeDim, maxMazeDim, mDimensionsDefault);
            CellDimensions = CheckDimensions(cellDim, minCellDim, maxCellDim, cDimensionsDefault);
            //ensures that maze and cell dimensions are valid (i.e. int above 5)

            WallThickness = Convert.ToInt32(CellDimensions[0] / 10);
            //sets thickness of walls based on width of cells
        }

        private int[] CheckDimensions(int[] array, int minValue, int maxValue, int[] defaultValues)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] < minValue || array[i] > maxValue)
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
        private Dictionary<int, Edge> Edges = new Dictionary<int, Edge>();
        //private Rectangle Shape = new Rectangle();

        public Cell(Point MazeLoc, Point CanvasLoc, int[] cellDimensions)
        {
            //Shape.Width = cellDimensions[0];
            //Shape.Height = cellDimensions[1];
            GridPt = MazeLoc;
            ShapePt = CanvasLoc;
        }

        public Edge GetEdgeFromDict(int key)
        {

            if (Edges.ContainsKey(key))
            {
                return Edges[key];
            }
            else
            {
                return null;
            }
        }

        public int GetEdgesCount()
        {
            return Edges.Count();
        }

        public Point GetMazePt()
        {
            return GridPt;
        }

        public Point GetPixelPt()
        {
            return ShapePt;
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

    class Entity
    {
        private int CurrentMovementSpeed;
        //private static int DefaultMovementSpeed;
        private Ellipse Shape = new Ellipse();
        private Point CurrentMazePt = new Point();
        private Point PixelPt = new Point();
        private string type;

        public Entity(string typeParam, Point StartMazePt, Point StartPixelPt, int[] cellDimensions, int Thickness, Brush colour)
        {
            //takes position of enemy in maze, and the corresponding pixelpoint

            this.type = typeParam;
            SetCurrentCellPt(StartMazePt);
            SetPixelPt(StartPixelPt);

            Shape.Fill = colour;
            Shape.Width = cellDimensions[0] - Thickness;
            Shape.Height = cellDimensions[1] - Thickness;

            Game.MW.myCanvas.Children.Add(Shape);
            Draw();
        }

        public void SetCurrentCellPt(Point MazePt)
        {
            CurrentMazePt = MazePt;
        }

        public void SetPixelPt(Point NewPt)
        {
            PixelPt = NewPt;
        }

        public Point GetCurrentLoc()
        {
            return CurrentMazePt;
        }

        public void Draw()
        {
            Canvas.SetLeft(Shape, PixelPt.X);
            Canvas.SetTop(Shape, PixelPt.Y);
        }
    }

    class Edge
    {
        private Point StartPoint;
        private Point TargetPoint;

        public Edge(Point StartPt, Point EndPt)
        {
            StartPoint = StartPt;
            TargetPoint = EndPt;
        }

        public Point GetPoint(int type)
        {
            //returns a specified vertex from the edge.

            if (type == 0)
            {
                return StartPoint;
            }
            else
            {
                return TargetPoint;
            }
        }
    }

    class Maze
    {
        private int[] MazeDimensions = new int[2];
        private int[] CellDimensions = new int[2];
        private int Thickness;

        private Wall[,] AllWallsH;
        private Wall[,] AllWallsV;

        private Cell[,] Cells;

        #region Maze Generation/Recursive Backtracker Variables
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
        //private List<int[]> AdjacentPositions;
        //private Point cell;
        private int GetDirectionType = 0;
        private int NumOfCellsInPath;
        #endregion

        private Point StartPoint = new Point(0, 0);
        private Point LastPoint;

        public Maze(GameConstants Constants)
        {
            ClearWindow(11);

            # region SettingUpMazeSection

            ///number of grid spaces in the maze
            MazeDimensions = Constants.MazeDimensions;
            CellDimensions = Constants.CellDimensions;
            Thickness = Constants.WallThickness;

            AllWallsH = new Wall[MazeDimensions[0], MazeDimensions[1] + 1];
            AllWallsV = new Wall[MazeDimensions[0] + 1, MazeDimensions[1]];
            ///adds one on right/bottom edge for each array to "close" box

            Cells = new Cell[MazeDimensions[0], MazeDimensions[1]];

            InitialiseMaze();
            //creates instances of each wall/cell and sets their x/y coordinates
            DrawGrid();

            # endregion

            //Generates recursive maze starting at point 0,0 and continuing until all cells have been visited
            NumOfCellsInPath = MazeDimensions[0] * MazeDimensions[1] - 1;
            MazeGeneration(StartPoint, GetDirectionType);

            #region AddingPlayersToMapSection

            Point playerStartPixelPt = Cells[(int)StartPoint.X, (int)StartPoint.Y].GetPixelPt();
            LastPoint = GetLastCellInMaze();
            Point enemyStartPixelPt = Cells[(int)LastPoint.X, (int)LastPoint.Y].GetPixelPt();

            List<Entity> Entities = new List<Entity>();

            Entities.Add(new Entity("player", StartPoint, playerStartPixelPt, CellDimensions, Thickness, Brushes.Yellow));
            Entities.Add(new Entity("enemy", LastPoint, enemyStartPixelPt, CellDimensions, Thickness, Brushes.Red));

            #endregion  

            GeneratePathToPlayer(Entities[0].GetCurrentLoc(), Entities[1].GetCurrentLoc());

        }

        private Point GetLastCellInMaze()
        {
            return new Point(MazeDimensions[0] - 1, MazeDimensions[1] - 1);
        }

        private void ClearWindow(int StartIndex)
        {
            Game.MW.myCanvas.Children.RemoveRange(StartIndex, Game.MW.myCanvas.Children.Count);
        }

        //public bool IsValidCell(int[] location)
        //{
        //    //determines if the passed coordinates are a valid point in the maze
        //    //takes int[2], returns bool

        //    bool valid = true;

        //    for (int i = 0; i < location.Count(); i++)
        //    {
        //        if (location[i] < 0 || location[i] >= MazeDimensions[i])
        //        {
        //            valid = false;
        //        }
        //    }

        //    return valid;

        //}

        public bool IsValidCell(Point Location)
        {
            //determines if the passed point corresponds to a valid point in the maze
            //takes Point, returns bool

            if (Location.X > -1 && Location.Y > -1 && Location.X < MazeDimensions[0] && Location.Y < MazeDimensions[1])
            {
                return true;
            }

            return false;
        }

        private void InitialiseMaze()
        {
            //creates individuals for each wall in the map

            for (int y = 0; y < AllWallsH.GetLength(1); y++)
            {
                for (int x = 0; x < AllWallsH.GetLength(0); x++)
                {
                    AllWallsH[x, y] = new Wall('h', x, y, CellDimensions, MazeDimensions, Thickness);
                }
            }

            for (int y = 0; y < AllWallsV.GetLength(1); y++)
            {
                for (int x = 0; x < AllWallsV.GetLength(0); x++)
                {
                    AllWallsV[x, y] = new Wall('v', x, y, CellDimensions, MazeDimensions, Thickness);
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

                reverseEdge = new Edge(thisEdge.GetPoint(1), thisEdge.GetPoint(0));
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
                if (cell.GetEdgesCount() == 0)
                {
                    Unvisited.Add(thisCellPt);
                }
            }

            return Unvisited;
        }

        private List<int> GetAdjacentDirections(Point Current, int type)
        {
            //ValidDirections stores the directions 0, 1, 2, 3 representing possible movements from the current cell
            //ex. Cell[0,0] is top-left, so it has 1, 2 (right, down)
            //ex-2. Cell[0,1] is one right of [0,0], so it has 1, 2, 3 (right, down, left)
            //ex-3. Cell[1,1] is one below [0,1], so it has 0, 1, 2, 3 (all directions) 

            List<Point> AdjacentPoints = new List<Point>
            {
                new Point(Current.X, Current.Y - 1),
                new Point(Current.X + 1, Current.Y),
                new Point(Current.X, Current.Y + 1),
                new Point(Current.X - 1, Current.Y),
            };

            ValidDirections.Clear();

            if (type == 0)
            {
                //enters this statement when following new path of unvisited cells

                for (int i = 0; i < AdjacentPoints.Count; i++)
                {
                    if (IsValidCell(AdjacentPoints[i]) && !VisitedCells.Contains(AdjacentPoints[i]) && Cells[(int)Current.X, (int)Current.Y].GetEdgesCount() < 2)
                    {
                        ValidDirections.Add(i);
                        //determines which adjacent cells have not already been explored
                        //creates list of valid directions from current cell
                    }
                }
            }
            else if (type == 1)
            {
                //enters this statement when backtracking along path

                for (int i = 0; i < AdjacentPoints.Count; i++)
                {
                    if (IsValidCell(AdjacentPoints[i]) && Cells[(int)AdjacentPoints[i].X, (int)AdjacentPoints[i].Y].GetEdgesCount() == 0 && Cells[(int)Current.X, (int)Current.Y].GetEdgesCount() < 3)
                    {
                        ValidDirections.Add(i);
                        //determines which adjacent cells (if any) have not already been explored
                    }
                }
            }
            return ValidDirections;
        }

        public List<Point> GetAdjacentPoints(Point Current)
        {
            //Dictionary<int, Point> AdjacentPoints = new Dictionary<int, Point>();

            //AdjacentPoints.Add(0, new Point(Current.X, Current.Y - 1));
            //AdjacentPoints.Add(1, new Point(Current.X + 1, Current.Y));
            //AdjacentPoints.Add(2, new Point(Current.X, Current.Y + 1));
            //AdjacentPoints.Add(3, new Point(Current.X - 1, Current.Y));

            List<Point> PointsList = new List<Point>();

            //finds adjacent cells AFTER maze has been generated (uses edges to determine validity)

            //for (int i = 0; i < AdjacentPoints.Count(); i++)
            //{
            //    if (IsValidCell(AdjacentPoints[i]) && !(Cells[(int)Current.X, (int)Current.Y].GetEdgeFromDict(i) == null))
            //    {
            //        //determines which adjacent cells (if any) have not already been explored and removes them from the list
            //        PointsList.Add(AdjacentPoints[i]);

            //    }
            //}

            Edge current;

            for (int i = 0; i < 4; i++)
            {
                current = Cells[(int)Current.X, (int)Current.Y].GetEdgeFromDict(i);

                if (current != null)
                {
                    PointsList.Add(current.GetPoint(1));
                }  
                
            }

            return PointsList;
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

            bool validMove = IsValidCell(startPt);

            if (validMove)
            {
                return startPt;
            }
            else
            {
                throw new Exception("Attempt to move to invalid point");
            }
        }

        public void GeneratePathToPlayer(Point Target, Point Current)
        {
            SimplePriorityQueue<Point, double> SearchCells = new SimplePriorityQueue<Point, double>();
            SearchCells.Enqueue(Current, 0);
            Dictionary<Point, double> CostToReach = new Dictionary<Point, double>();
            Dictionary<Point, Point> CameFrom = new Dictionary<Point, Point>();

            Dictionary<Point, Point> PathToFollow = new Dictionary<Point, Point>();

            CostToReach.Add(Current, 0);
            CameFrom.Add(Current, new Point(-1, -1));

            List<Point> AdjacentCells;
            double newCost = 0;
            double thisPriority = 0;

            bool pathFound = false;

            while (SearchCells.Count() != 0 && pathFound == false)
            {
                Current = SearchCells.Dequeue();
                //takes out next cell to be checked (based on priority)

                if (Current == Target)
                {
                    pathFound = true;
                }
                else
                {
                    AdjacentCells = GetAdjacentPoints(Current);
                    //gets all valid neighbouring cells to the current cell
                    
                    foreach (var NextCell in AdjacentCells)
                    {
                        newCost = CostToReach[Current] + 1;
                        //adds one to the total cost to reach that cell from the start

                        if (!CostToReach.ContainsKey(NextCell) || newCost < CostToReach[NextCell])
                        {
                            //if this path now includes the next cell or it is shorter than the last path,

                            CostToReach[NextCell] = newCost;
                            thisPriority = newCost + Heuristic(Target, NextCell);
                            //the priority with moving to that cell is based on the approximate distance to the target

                            SearchCells.Enqueue(NextCell, thisPriority);
                            CameFrom[NextCell] = Current;
                            //records all the paths from  of the previous one
                        }
                    }
                }
            }

            Point newkey = CameFrom[Target];
            PathToFollow[Target] = newkey;

            while (newkey != new Point(-1,-1))
            {
                PathToFollow[newkey] = CameFrom[newkey];
                newkey = CameFrom[newkey];
            }

        }

        private double Heuristic(Point target, Point current)
        {
            //finds manhattan (direct) distance from current point to target

            double dx = target.X - current.X;
            double dy = target.Y - current.Y;

            double distSquared = Math.Pow(dx, 2) + Math.Pow(dy, 2);
            return Math.Sqrt(distSquared);
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
        private char type;


        public Wall(char typeParam, int i, int j, int[] cellDimensions, int[] mazeDimensions, int thickness)
        {
            this.type = typeParam;

            //sets size of wall based on whether horizontal or vertical

            if (type == 'h')
            {
                //horizontal wall conditions

                width = cellDimensions[0] + thickness;
                height = thickness;

                if (j == 0 || j == mazeDimensions[1])
                {
                    //walls can be hidden unless they are on the outer edge of the maze
                    hideable = false;
                }
            }
            else if (type == 'v')
            {
                //vertical wall conditions

                width = thickness;
                height = cellDimensions[1];

                if (i == 0 || i == mazeDimensions[0])
                {
                    //walls can be hidden unless they are on the outer edge of the maze
                    hideable = false;
                }
            }

            Shape.Width = this.width;
            Shape.Height = this.height;

            XCoord = GameConstants.indent + (i + 1) * cellDimensions[0] + thickness;
            YCoord = (j + 1) * cellDimensions[1];
            //calculates position of wall based on location in array

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

    //class WallHorizontal : Wall
    //{
    //    public WallHorizontal(int i, int j, int[] cellDimensions, int[] mazeDimensions, int thickness) : base()
    //    {
    //        //parameters i and j represent location of wall in AllWallsH or AllWallsV

           
    //    }
    //}

    //class WallVertical : Wall
    //{
    //    public WallVertical(int i, int j, int[] cellDimensions, int[] mazeDimensions, int thickness) : base()
    //    {
    //        //parameters i and j represent location of wall in AllWallsH or AllWallsV

            
    //    }
    //}

    class Game
    {
        public static MainWindow MW { get; set; }                 ////allows access to canvas outside of main window
        public Maze MazeOne;
        private GameConstants Constants;

        public Game()
        {
            MW = (MainWindow)Application.Current.MainWindow;
            //MW.myCanvas.Background = Brushes.Black;
        }

        public void CreateMaze(string[] mazeDimText, string[] cellDimText)
        {
            int[] mDimensions = ConvertDimensionsToInt(mazeDimText);
            int[] cDimensions = ConvertDimensionsToInt(cellDimText);
            //gets size of maze/cells from textboxes

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