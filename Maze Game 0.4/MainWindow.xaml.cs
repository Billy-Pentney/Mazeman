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
using System.Windows.Threading;
using System.Windows.Shapes;
using Priority_Queue;

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
        private int maxMazeDim = 30;
        private int maxCellDim = 30;
        //minimum values for maze and cells that is accepted by the program

        private int[] mDimensionsDefault;
        private int[] cDimensionsDefault;

        //public Brush BackgroundColour { get; } = Brushes.White;
        //public Brush ForegroundColour { get; } = Brushes.Black;

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

    class PowerUp
    {
        private DispatcherTimer ActiveTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };

        private static double[] FriendlyEffects = new double[] { 2, 1, 1, 1 };
        //scalar multiples applied to speed of all players speed if collected by player
        private static double[] EnemyEffects = new double[] { 1, 0.5, 0, 1 };
        //scalar multiples applied to speed of all enemies speed if collected by player
        private static double[] PointEffects = new double[] { 1, 1, 1, 5 };
        //scalar multiples applied to value of the points if collected by player
        private static Brush[] PossibleColours = new Brush[] { Brushes.Orange, Brushes.LimeGreen, Brushes.Purple, Brushes.Aqua };

        private double[] Effects = new double[2];
        private Brush Colour;
        private bool visible = false;
        private Rectangle Shape = new Rectangle();
        private int type = 0;
        private int duration = 10;
        private int CurrentActiveTime = 0;
        public PowerUp(int[] cellDimensions, int typeParam, Point PixelPt)
        {
            this.type = typeParam;
            SetType();

            Shape.Width = cellDimensions[0] * 0.8;
            Shape.Height = cellDimensions[0] * 0.8;
            Shape.Fill = Colour;

            Game.MW.myCanvas.Children.Add(Shape);
            Canvas.SetLeft(Shape, PixelPt.X);
            Canvas.SetTop(Shape, PixelPt.Y);
            visible = true;
        }

        private void StartTimer()
        {
            ActiveTimer.Start();
            CurrentActiveTime += 1;
            ActiveTimer.Tick += ActiveTimer_Tick;
        }

        private void ActiveTimer_Tick(object sender, EventArgs e)
        {
            IsExpired();
        }

        private void IsExpired()
        {
            if (CurrentActiveTime >= duration)
            {
                ActiveTimer.Stop();
                CurrentActiveTime = 0;

                if (visible)
                {
                    visible = false;
                    Game.MW.myCanvas.Children.Remove(Shape);
                }
                else
                {
                    for (int i = 0; i < Effects.Length; i++)
                    {
                        Effects[i] = 0;
                    }
                    //when powerup has ran out of time, all effects are taken away
                }
            }
        }

        private void SetType()
        {
            Effects[0] = FriendlyEffects[type];
            Effects[1] = EnemyEffects[type];
            Colour = PossibleColours[type];
            //this sets the effects based on the randomly generated type
            //e.g. if type is 1, the friendly effect at [0] is x2, the enemy effect at [1] is x1

        }

        public double GetEffect(int index)
        {
            return Effects[index];
        }
    }

    class ScorePt
    {
        private Rectangle Shape = new Rectangle();
        private Point PixelPt = new Point();

        public ScorePt(Point PointParam, int[] cellDimensions)
        {
            Shape.Width = (int)cellDimensions[0] / 7;
            Shape.Height = (int)cellDimensions[1] / 7;
            Shape.Fill = Brushes.Orange;

            Game.MW.myCanvas.Children.Add(Shape);

            PixelPt = new Point(PointParam.X + 0.5 * (cellDimensions[0] - Shape.Width), PointParam.Y + 0.5 * (cellDimensions[1] - Shape.Height));
            Draw();
        }

        public void Hide()
        {
            Shape.Opacity = 0;
        }

        public void Draw()
        {
            Shape.Opacity = 255;
            Canvas.SetLeft(Shape, PixelPt.X);
            Canvas.SetTop(Shape, PixelPt.Y);
        }

        public bool GetVisible()
        {
            if (Shape.Opacity < 255)
            {
                return false;
            }
            return true;
        }
    }

    class Player
    {
        protected double CurrentMovementSpeed;
        protected static double DefaultMovementSpeed = 1;
        protected Ellipse Shape = new Ellipse();
        protected Point CurrentMazePt = new Point();
        protected Point PixelPt = new Point();
        protected int CurrentDirection;
        private int Score = 0;
        private PowerUp ActivePowerUp;
        private int PlayerNum;
        protected static Brush[] Colours = new Brush[] { Brushes.Yellow, Brushes.Blue, Brushes.Red };

        public Player(Point StartMazePt, Point StartPixelPt, int[] cellDimensions, int Thickness, int Number)
        {
            //takes position of enemy in maze, and the corresponding pixelpoint

            SetCurrentCellPt(StartMazePt);
            SetPixelPt(StartPixelPt);

            CurrentMovementSpeed = DefaultMovementSpeed;
            PlayerNum = Number;     //player 1 has number 0, player 2 has number 1, Enemy has number 2

            if (PlayerNum > -1 && PlayerNum < Colours.Count())
            {
                Shape.Fill = Colours[PlayerNum];
            }

            Shape.Width = cellDimensions[0] - Thickness;
            Shape.Height = cellDimensions[1] - Thickness;

            Game.MW.myCanvas.Children.Add(Shape);
            Draw();
        }

        public void IncrementScore(int increment)
        {
            Score += increment;
        }

        public void RemoveFromMap()
        {
            Game.MW.myCanvas.Children.Remove(Shape);
        }

        public int GetScore()
        {
            return Score;
        }

        public void SetScore(int newScore)
        {
            if (newScore > -1)
            {
                Score = newScore;
            }
        }

        public double GetSpeed()
        {
            return CurrentMovementSpeed;
        }

        public void SetCurrentCellPt(Point MazePt)
        {
            CurrentMazePt = MazePt;
            //SetPowerUpEffects();
        }

        private void SetPowerUpEffects()
        {
            CurrentMovementSpeed *= ActivePowerUp.GetEffect(0);
        }

        public void PowerUpCollected(PowerUp Collected)
        {
            ActivePowerUp = Collected;
        }

        public void SetPixelPt(Point NewPt)
        {
            PixelPt = NewPt;
        }

        public Point GetCurrentLoc()
        {
            return CurrentMazePt;
        }

        public int GetDirection()
        {
            return CurrentDirection;
        }

        public void Draw()
        {
            Canvas.SetLeft(Shape, PixelPt.X);
            Canvas.SetTop(Shape, PixelPt.Y);
        }

        public virtual void UpdateDirection(Key thisKey)
        {
            if (PlayerNum == 0)
            {
                switch (thisKey)
                {
                    case Key.W:
                        CurrentDirection = 0;
                        break;
                    case Key.D:
                        CurrentDirection = 1;
                        break;
                    case Key.S:
                        CurrentDirection = 2;
                        break;
                    case Key.A:
                        CurrentDirection = 3;
                        break;
                    default:
                        break;
                }
            }
            else if (PlayerNum == 1)
            {
                switch (thisKey)
                {
                    case Key.I:
                        CurrentDirection = 0;
                        break;
                    case Key.L:
                        CurrentDirection = 1;
                        break;
                    case Key.K:
                        CurrentDirection = 2;
                        break;
                    case Key.J:
                        CurrentDirection = 3;
                        break;
                    default:
                        break;
                }
            }
        }

    }

    class Cell
    {
        private Point GridPt;
        private Point ShapePt;
        private Dictionary<int, Edge> Edges = new Dictionary<int, Edge>();
        private ScorePt thisPoint;

        public Cell(Point MazeLoc, Point CanvasLoc, int[] cellDimensions)
        {
            GridPt = MazeLoc;
            ShapePt = CanvasLoc;
            thisPoint = new ScorePt(ShapePt, cellDimensions);
        }

        public Edge GetEdgeFromDirection(int key)
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

        public bool TogglePoint()
        {
            if (thisPoint.GetVisible())
            {
                thisPoint.Hide();
                return true;
                //returns true if the point had not been collected before
            }
            //returns false if it was already hidden
            return false;
        }

        public void RedrawPoint()
        {
            thisPoint.Draw();
        }

        public bool IsPointVisible()
        {
            return thisPoint.GetVisible();
        }
    }

    class Enemy : Player
    {
        private Point Target;
        private Queue<int> DirectionsToFollow = new Queue<int>();

        public Enemy(Point StartMazePt, Point StartPixelPt, int[] cellDimensions, int Thickness, int Number) : base(StartMazePt, StartPixelPt, cellDimensions, Thickness, Number)
        {
            CurrentMovementSpeed = 0.5;
        }

        public int GetNextDirection(int StartIndex)
        {
            if (StartIndex < DirectionsToFollow.Count())
            {
                return DirectionsToFollow.ElementAt(StartIndex);
            }
            else
            {
                throw new Exception("Index refers to a later direction than is available");
            }
        }

        public int GetPathLength()
        {
            return DirectionsToFollow.Count();
        }

        public Point GetTarget()
        {
            return Target;
        }

        public void SetTarget(Point newTarget)
        {
            Target = newTarget;
        }

        //public void UpdatePath(int StartIndex, List<int> newDirections)
        //{
        //    if (DirectionsToFollow.Count > StartIndex)
        //    {
        //        //replaces elements from a certain position onwards with a specific list (parameter)
        //        DirectionsToFollow.RemoveRange(StartIndex, DirectionsToFollow.Count - StartIndex);
        //        DirectionsToFollow.AddRange(newDirections);
        //    }
        //    else
        //    {
        //        throw new Exception("Attempt to update path outside maximum value");
        //    }
        //}

        //public List<int> GetPath()
        //{
        //    if (GetPathLength() > 0)
        //    {
        //        return DirectionsToFollow.ToList();
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        public void UpdatePath(Queue<int> newDirections)
        {
            //adds the list of instructions to the path for the enemy

            if (DirectionsToFollow.Count() > 0)
            {
                DirectionsToFollow.Clear();
            }

            DirectionsToFollow = newDirections;
        }

        public void UpdateDirection()
        {
            if (DirectionsToFollow.Count > 0)
            {
                CurrentDirection = DirectionsToFollow.Dequeue();
            }
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
        private int NumOfActivePoints;

        #region Maze Generation/Recursive Backtracker Variables
        private List<int> ValidMoves;
        private int randMoveSelection;
        private int thisMove;
        private Point nextCellPt = new Point();
        private Edge thisEdge;
        private Edge reverseEdge;
        private Random rand = new Random();
        private Stack<Point> VisitedCells = new Stack<Point>();
        private int randTakeTwoWalls;

        private List<int> ValidDirections = new List<int>();
        private int GetDirectionType = 0;
        private int NumOfCellsInPath;
        #endregion

        public Maze(GameConstants Constants)
        {
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
            NumOfCellsInPath = MazeDimensions[0] * MazeDimensions[1];
            MazeGeneration(GetFirstCellInMaze(0), GetDirectionType);
            NumOfActivePoints = MazeDimensions[0] * MazeDimensions[1];
        }

        public Point GetLastCellInMaze(int PtType)
        {
            if (PtType == 1)
            {
                //returns the top left pixel for that cell if specified
                return Cells[MazeDimensions[0] - 1, MazeDimensions[1] - 1].GetPixelPt();
            }
            else
            {
                //otherwise, returns the position of that cell in the maze
                return new Point(MazeDimensions[0] - 1, MazeDimensions[1] - 1);
            }
        }

        public Point GetFirstCellInMaze(int PtType)
        {
            if (PtType == 1)
            {
                //returns the top left pixel for that cell if specified
                return Cells[0, 0].GetPixelPt();
            }
            else
            {
                //otherwise, returns the position of that cell in the maze
                return new Point(0, 0);
            }
        }

        public Point GetBottomLeftCell(int PtType)
        {
            if (PtType == 1)
            {
                //returns the top left pixel for that cell if specified
                return Cells[0, MazeDimensions[1] - 1].GetPixelPt();
            }
            else
            {
                //otherwise, returns the position of that cell in the maze
                return new Point(0, MazeDimensions[1] - 1);
            }
        }

        public int[] GetCellDimensions()
        {
            return CellDimensions;
        }

        public int GetWallThickness()
        {
            return Thickness;
        }

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

        public Point GetPixelPoint(Point location)
        {
            return Cells[(int)location.X, (int)location.Y].GetPixelPt();
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
            //thisCell = Cells[(int)currentCellPt.X, (int)currentCellPt.Y];

            ValidMoves = GetAdjacentDirections(currentCellPt, GetDirectionType);
            //gets all possible moves from the current cell to adjacent ones

            if (ValidMoves.Count > 0)
            {
                VisitedCells.Push(currentCellPt);

                randTakeTwoWalls = rand.Next(1, ValidMoves.Count());

                for (int i = 0; i < randTakeTwoWalls; i++)
                {
                    randMoveSelection = rand.Next(0, ValidMoves.Count);
                    thisMove = ValidMoves[randMoveSelection];
                    //gets a random valid move from the current cell to one of its neighbours

                    ValidMoves.RemoveAt(randMoveSelection);

                    nextCellPt = MoveFromPoint(currentCellPt, thisMove);
                    thisEdge = new Edge(currentCellPt, nextCellPt);

                    Cells[(int)currentCellPt.X, (int)currentCellPt.Y].AddEdge(thisEdge, thisMove);
                    //adds edge to the current cell

                    reverseEdge = new Edge(thisEdge.GetPoint(1), thisEdge.GetPoint(0));
                    //flips the edge so its inverse can be stored in the other cell

                    TurnOffWall(nextCellPt, thisMove);

                    Cells[(int)nextCellPt.X, (int)nextCellPt.Y].AddEdge(reverseEdge, ReverseMove(thisMove));
                    //adds opposite edge to the next cell
                }

                if (VisitedCells.Count < NumOfCellsInPath)
                {
                    GetDirectionType = 0;
                    MazeGeneration(nextCellPt, GetDirectionType);
                }

            }
            else if (VisitedCells.Count > 0)
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

            return (OriginalMove + 2) % 4;

            //numOfDirections = 4, so    (0 + 2) % 4 = 2,    (1 + 2) % 4 = 3,     (2 + 2) % 4 = 0,    (3 + 2) % 4 = 1,

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
                    //if a cell has no edges, it has not been visited by the maze-generator
                    Unvisited.Add(thisCellPt);
                }
            }

            return Unvisited;
        }

        private List<int> GetAdjacentDirections(Point Current, int type)
        {
            ///Note: this method is for the backtracker algorithm to find the possible directions it can move in

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

            ValidDirections = new List<int>();

            if (type == 0)
            {
                //enters this statement when following new path of unvisited cells

                for (int i = 0; i < AdjacentPoints.Count; i++)
                {
                    if (IsValidCell(AdjacentPoints[i]) && !VisitedCells.Contains(AdjacentPoints[i]) && Cells[(int)Current.X, (int)Current.Y].GetEdgesCount() < 4)
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
                    if (IsValidCell(AdjacentPoints[i]) && Cells[(int)Current.X, (int)Current.Y].GetEdgeFromDirection(i) == null && Cells[(int)AdjacentPoints[i].X, (int)AdjacentPoints[i].Y].GetEdgesCount() == 0 && Cells[(int)Current.X, (int)Current.Y].GetEdgesCount() < 4)
                    {
                        ValidDirections.Add(i);
                        //determines which adjacent cells (if any) have not already been explored
                    }
                }
            }
            //else if (type == 2)
            //{
            //    //enters this statement when backtracking along path

            //    for (int i = 0; i < AdjacentPoints.Count; i++)
            //    {
            //        if (IsValidCell(AdjacentPoints[i]) && Cells[(int)AdjacentPoints[i].X, (int)AdjacentPoints[i].Y].GetEdgesCount() == 0 && Cells[(int)Current.X, (int)Current.Y].GetEdgesCount() < 4)
            //        {
            //            ValidDirections.Add(i);
            //            //determines which adjacent cells (if any) have not already been explored
            //        }
            //    }
            //}


            return ValidDirections;
        }

        public List<Point> GetAdjacentPoints(Point Current)
        {
            //Note: this method is for getting the adjacent cells via the graph
            //it determines which points are adjacent based off of the edges of the current cell

            List<Point> PointsList = new List<Point>();
            Edge currentEdge;

            for (int i = 0; i < 4; i++)
            {
                currentEdge = Cells[(int)Current.X, (int)Current.Y].GetEdgeFromDirection(i);

                if (currentEdge != null)
                {
                    PointsList.Add(currentEdge.GetPoint(1));
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

        public bool CheckEdgeInDirection(Point position, int direction)
        {
            if (Cells[(int)position.X, (int)position.Y].GetEdgeFromDirection(direction) != null)
            {
                return true;
            }

            return false;
        }

        public Queue<int> GeneratePathToTarget(Point Target, Point Current)
        {
            SimplePriorityQueue<Point, double> SearchCells = new SimplePriorityQueue<Point, double>();
            //search cells is the list of cells to be checked by the algorithm
            SearchCells.Enqueue(Current, 0);

            Dictionary<Point, double> CostToReach = new Dictionary<Point, double>();
            //for each key, there is a value (cost) associated with moving from the start to that cell

            Dictionary<Point, Point> CameFrom = new Dictionary<Point, Point>();
            //each point directs back to the previous point in the path
            //e.g. if the Enemy has to move from 1,1 to 0,1 and then from 0,1 to 0,0
            //CameFrom[0,1] = [1,1]
            //CameFrom[0,0] = [0,1]

            CostToReach.Add(Current, 0);
            CameFrom.Add(Current, new Point(-1, -1));

            List<Point> AdjacentCells = new List<Point>();
            List<Point> NewPath = new List<Point>();

            double newCost = 0;
            double thisPriority = 0;

            bool pathFound = false;

            while (SearchCells.Count() != 0 && pathFound == false)
            {
                Current = SearchCells.Dequeue();
                //takes out next cell to be checked (based on priority)

                if (Current == Target)
                {
                    //exits loop if path reaches the target cell
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
                        //(moving along the path increases its length by 1)

                        if (!CostToReach.ContainsKey(NextCell) || newCost < CostToReach[NextCell])
                        {
                            //if this path now includes a cell that hadn't been in explored before 
                            //or it is shorter than the previous best path to that cell,

                            CostToReach[NextCell] = newCost;
                            thisPriority = newCost + GetApproximateDistance(NextCell, Target);
                            //the priority associated with moving to that cell is based on the approximate distance from it to the target

                            SearchCells.Enqueue(NextCell, thisPriority);
                            CameFrom[NextCell] = Current;
                            //points from the new cell to the last one so that a path can be followed
                        }
                    }
                }
            }

            //at the end of the loop, CameFrom points from each cell to the one before it on the path
            //now we need to remove unnecessary points (those that don't lead to the goal)
            //and reverse the list, before converting it to a set of instructions

            Point newkey = CameFrom[Target];
            NewPath.Add(Target);

            while (newkey != new Point(-1, -1))
            {
                //at the start we said that the value for the start cell was [-1,-1]
                //if the key is [-1,-1], we must be at the start of the path

                NewPath.Add(newkey);
                newkey = CameFrom[newkey];

                //creates list of points leading from the target point to the start point
            }

            NewPath.Reverse();
            //reverses the list so that the path is from the start to the target

            Queue<int> DirectionsToFollow = new Queue<int>();

            for (int i = 0; i < NewPath.Count - 1; i++)
            {
                DirectionsToFollow.Enqueue(ConvertMovementToDirection(NewPath[i], NewPath[i + 1]));
                //converts the list of points to instructions for the enemy to follow
            }

            return DirectionsToFollow;
        }

        private double GetApproximateDistance(Point current, Point target)
        {
            //finds manhattan (direct) distance from current point to target

            double dx = target.X - current.X;
            double dy = target.Y - current.Y;

            double distSquared = Math.Pow(dx, 2) + Math.Pow(dy, 2);
            return Math.Sqrt(distSquared);
        }

        private int ConvertMovementToDirection(Point start, Point target)
        {
            if (target.Y == start.Y - 1)
            {
                return 0;
            }
            else if (target.X == start.X + 1)
            {
                return 1;
            }
            else if (target.Y == start.Y + 1)
            {
                return 2;
            }
            else if (target.X == start.X - 1)
            {
                return 3;
            }

            throw new Exception("Attempt to convert two invalid points to a movement");
        }

        public bool PointCrossed(Point newPoint)
        {
            bool Unvisited = Cells[(int)newPoint.X, (int)newPoint.Y].TogglePoint();

            if (Unvisited)
            {
                NumOfActivePoints -= 1;
            }

            if (NumOfActivePoints <= 0)
            {
                RedrawAllPoints();
                NumOfActivePoints = MazeDimensions[0] * MazeDimensions[1];
            }

            return Unvisited;
        }

        private void RedrawAllPoints()
        {
            foreach (var cell in Cells)
            {
                cell.RedrawPoint();
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

    class Game
    {
        public static MainWindow MW { get; set; }                 ////allows access to canvas outside of main window
        private Maze MazeOne;
        private DispatcherTimer GameTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1), };
        private DispatcherTimer MovementTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 150), };
        private int NumOfPlayers = 1;
        private List<Player> ActivePlayers = new List<Player>();
        private List<Enemy> ActiveEnemies = new List<Enemy>();
        private int currentTime = 0;
        private int MovementCount = 0;
        private GameConstants Constants;
        private int PointValue = 1;
        private TextBlock TimeDisplayTXT = new TextBlock();
        private TextBlock ScoreDisplayTXT = new TextBlock();
        private List<Player> RemovedPlayers = new List<Player>();

        public Game()
        {
            MW = (MainWindow)Application.Current.MainWindow;

            NumOfPlayers = 2;

            GameTimer.Tick += GameTimer_Tick;
            MovementTimer.Tick += MovementTimer_Tick;

            TimeDisplayTXT.Width = 30;
            MW.myCanvas.Children.Add(TimeDisplayTXT);
            Canvas.SetLeft(TimeDisplayTXT, 55);
            Canvas.SetTop(TimeDisplayTXT, 225);

            ScoreDisplayTXT.Width = 30;
            MW.myCanvas.Children.Add(ScoreDisplayTXT);
            Canvas.SetLeft(ScoreDisplayTXT, 55);
            Canvas.SetTop(ScoreDisplayTXT, 250);
        }

        public void UpdatePlayerMovement(Key thisKey)
        {
            foreach (var Player in ActivePlayers)
            {
                Player.UpdateDirection(thisKey);
            }
        }

        private void MovementTimer_Tick(object sender, EventArgs e)
        {
            MovementCount += 1;
            Point target = new Point(0, 0);
            int PlayerToRemove;

            foreach (var Player in ActivePlayers)
            {
                //cycles through each player and "collects"/hides the point that they are on

                if (MazeOne.IsValidCell(Player.GetCurrentLoc()))
                {
                    if (MazeOne.PointCrossed(Player.GetCurrentLoc()))
                    {
                        Player.IncrementScore(PointValue);
                    }
                    UpdatePlayerPosition(Player);
                }
            }

            foreach (var Enemy in ActiveEnemies)
            {
                PlayerToRemove = CheckTouches(Enemy);
                Enemy.UpdateDirection();
                UpdatePlayerPosition(Enemy);
                FindShortestPath(Enemy);
            }

            UpdateScoreAndTime();
        }

        private int CheckTouches(Enemy EnemyToCheck)
        {
            for (int i = 0; i < ActivePlayers.Count(); i++)
            {
                //cycles through the locations of all players

                if (EnemyToCheck.GetCurrentLoc() == ActivePlayers[i].GetCurrentLoc())
                {
                    ActivePlayers[i].RemoveFromMap();
                    //removes the visual aspect of the player
                    RemovedPlayers.Add(ActivePlayers[i]);
                    //stores the player in another list for score access

                    ActivePlayers.RemoveAt(i);      
                    ///removes that player from the game so it cannot be tracked by enemy
                    return i;
                    //if the enemy is on the same cell as the player
                }
            }

            if (ActivePlayers.Count() < 1)
            {
                //EndGame()
            }

            return -1;

        }

        private void UpdatePlayerPosition(Player Entity)
        {
            Point currentPoint = Entity.GetCurrentLoc();
            int direction = Entity.GetDirection();

            int ChanceOfMoving = MovementCount % (int)(1 / Entity.GetSpeed());

            if (MazeOne.CheckEdgeInDirection(currentPoint, direction) && ChanceOfMoving == 0)
            {
                //checks that there is an edge between those cells
                Point newPoint = MazeOne.MoveFromPoint(currentPoint, direction);
                Entity.SetCurrentCellPt(newPoint);
                //moves in that direction

                newPoint = MazeOne.GetPixelPoint(newPoint);
                Entity.SetPixelPt(newPoint);
                //gets the new position in the window

                Entity.Draw();
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            currentTime += 1;


            //updates the length of time the game has been open, once per second
        }

        private void UpdateScoreAndTime()
        {
            TimeDisplayTXT.Text = Convert.ToString(currentTime);
            if (ActivePlayers.Count() == 1)
            {
                ScoreDisplayTXT.Text = Convert.ToString(ActivePlayers[0].GetScore());
            }
            else if (ActivePlayers.Count() == 2)
            {
                ScoreDisplayTXT.Text = Convert.ToString(ActivePlayers[0].GetScore() + ActivePlayers[1].GetScore());
            }

        }

        public void CreateMaze(string[] mazeDimText, string[] cellDimText)
        {
            int[] mDimensions = ConvertDimensionsToInt(mazeDimText);
            int[] cDimensions = ConvertDimensionsToInt(cellDimText);
            //gets size of maze/cells from textboxes

            ClearWindow(15);
            currentTime = 0;

            Constants = new GameConstants(mDimensions, cDimensions);
            MazeOne = new Maze(Constants);

            AddEntities();
            GameTimer.Start();
            MovementTimer.Start();
        }

        private void AddEntities()
        {
            Point StartPoint = MazeOne.GetFirstCellInMaze(0);
            Point StartPointPixelPt = MazeOne.GetFirstCellInMaze(1);
            Point LastPoint = MazeOne.GetLastCellInMaze(0);
            Point LastPointPixelPt = MazeOne.GetLastCellInMaze(1);

            ActivePlayers.Clear();
            ActiveEnemies.Clear();

            ActivePlayers.Add(new Player(StartPoint, StartPointPixelPt, MazeOne.GetCellDimensions(), MazeOne.GetWallThickness(), 0));
            ActiveEnemies.Add(new Enemy(LastPoint, LastPointPixelPt, MazeOne.GetCellDimensions(), MazeOne.GetWallThickness(), 2));

            if (NumOfPlayers == 2)
            {
                StartPoint = MazeOne.GetBottomLeftCell(0);
                StartPointPixelPt = MazeOne.GetBottomLeftCell(1);
                ActivePlayers.Add(new Player(StartPoint, StartPointPixelPt, MazeOne.GetCellDimensions(), MazeOne.GetWallThickness(), 1));
                //ActiveEntities.Add(new Player(StartPoint, StartPointPixelPt, MazeOne.GetCellDimensions(), MazeOne.GetWallThickness(), Brushes.Blue));
            }

            FindShortestPath(ActiveEnemies[0]);
        }

        private void FindShortestPath(Enemy enemyParam)
        {
            Queue<int> ShortestPath;
            Queue<int> AlternatePath;
            Point target = new Point(0, 0);

            if (ActivePlayers.Count > 0)
            {
                ShortestPath = MazeOne.GeneratePathToTarget(ActivePlayers[0].GetCurrentLoc(), enemyParam.GetCurrentLoc());

                for (int i = 1; i < ActivePlayers.Count; i++)
                {
                    AlternatePath = MazeOne.GeneratePathToTarget(ActivePlayers[i].GetCurrentLoc(), enemyParam.GetCurrentLoc());

                    if (AlternatePath.Count() < ShortestPath.Count())
                    {
                        ShortestPath = AlternatePath;
                        target = ActivePlayers[i].GetCurrentLoc();
                    }
                }

                enemyParam.UpdatePath(ShortestPath);
                enemyParam.SetTarget(target);
            }
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

        private void ClearWindow(int StartIndex)
        {
            MW.myCanvas.Children.RemoveRange(StartIndex, MW.myCanvas.Children.Count);
        }
    }

    public partial class MainWindow : Window
    {
        Game gameOne;

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            string[] mazeSizeTXT = new string[2] { MwidthTXT.Text, MheightTXT.Text };
            string[] cellSizeTXT = new string[2] { CwidthTXT.Text, CheightTXT.Text };

            gameOne.CreateMaze(mazeSizeTXT, cellSizeTXT);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            gameOne.UpdatePlayerMovement(e.Key);
        }

        public MainWindow()
        {
            InitializeComponent();
            gameOne = new Game();
        }
    }
}
