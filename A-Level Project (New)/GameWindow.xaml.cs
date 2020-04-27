using Newtonsoft.Json;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace A_Level_Project__New_
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    /// 

    class GameConstants
    {
        //this class contains most of the major Game constants/variables
        //these can be changed to affect the way the game looks/plays

        public static int[] CellDimensions { get; } = new int[2] { 25, 25 };
        public static double WallThicknessProportion = 0.15;

        public static int[] WinIndent { get; } = new int[2] { 85, 20 };
        public static int[] MazeIndent { get; } = new int[2] { 85, 0 };
        //the pixel values used to indent the maze from the left/top of the window

        public const string FileName = "History.txt";
        //the name/address of the file where scores should be written to/read from

        public static double[] difficulties = new double[] { 1, 2, 3 };
        //the player movement speed with no powerups
        
        public static int ClearPointsValue { get; } = 50;

        public static Brush[] PlayerColours { get; } = new Brush[] { Brushes.Yellow, Brushes.Blue, Brushes.Red };
        public static Brush[] PlayerFreezeColours { get; } = new Brush[] { Brushes.Gray, Brushes.DarkGray, Brushes.DarkRed };
        public static Brush[] PowerUpColours { get; } = new Brush[] { Brushes.Orange, Brushes.LimeGreen, Brushes.LightBlue, Brushes.Purple };
        public static Brush[] ScorePointColours { get; } = new Brush[] { Brushes.Gray, Brushes.Orange, Brushes.Purple };

        public static Brush BackgroundColour = Brushes.White;
        public static Brush ForegroundColour = Brushes.Black;
        public static Brush[] WallColours = new Brush[] { Brushes.Black, Brushes.Blue };
        public static Brush[] SecondaryColours = new Brush[] { Brushes.Blue, Brushes.Purple, Brushes.Cyan, Brushes.Lime };

        public static void SwapColours()
        {
            Brush temporary = BackgroundColour;

            BackgroundColour = ForegroundColour;
            ForegroundColour = temporary;

            temporary = WallColours[0];
            WallColours[0] = WallColours[1];
            WallColours[1] = temporary;

            temporary = SecondaryColours[0];
            SecondaryColours[0] = SecondaryColours[2];
            SecondaryColours[2] = temporary;

            temporary = SecondaryColours[1];
            SecondaryColours[1] = SecondaryColours[3];
            SecondaryColours[3] = temporary;
        }
    }

    #region Powerup CLASSES

    class SpeedUpPowerup : Powerup
    {
        private double multiplier = 1.5;            ///speed increased by 50%

        public SpeedUpPowerup(Point MazePt, Point PixelPt): base(MazePt, PixelPt)
        {
            TypeName = "Player Speed is increased for " + maxDuration + " seconds!";
            Effects[0] = multiplier;
            TypeNumber = 0;
            shape.Fill = GameConstants.PowerUpColours[TypeNumber];
        }

        public override void Collect(Player collector)
        {
            base.Collect(collector);

            if (ReverseEffects)
            {
                TypeName = "Enemy Speed is increased for " + maxDuration + " seconds!";
            }
        }
    }

    class SpeedDownPowerup : Powerup
    {
        private double multiplier = 0.5;            ///speed decreased by 50%

        public SpeedDownPowerup(Point MazePt, Point PixelPt) : base(MazePt, PixelPt)
        {
            TypeName = "Enemy Speed is halved for " + maxDuration + " seconds!";
            Effects[1] = multiplier;
            TypeNumber = 1;
            shape.Fill = GameConstants.PowerUpColours[TypeNumber];
        }

        public override void Collect(Player collector)
        {
            base.Collect(collector);

            if (ReverseEffects)
            {
                TypeName = "Player Speed is halved for " + maxDuration + " seconds!";
            }
        }
    }

    class FreezePowerup : Powerup
    {
        public FreezePowerup(Point MazePt, Point PixelPt) : base(MazePt, PixelPt)
        {
            Random rand = new Random();
            TypeName = "Enemy is frozen for " + Convert.ToString(maxDuration) + " seconds!";
            TypeNumber = 2;
            shape.Fill = GameConstants.PowerUpColours[TypeNumber];
            Effects[1] = -1;            //sets speed to -1 to prevent movement, but multiplies out to give original value again
        }

        public override void Collect(Player collector)
        {
            base.Collect(collector);

            if (ReverseEffects)
            {
                TypeName = "Players are frozen for " + Convert.ToString(maxDuration) + " seconds!"; ;
            }
        }

    }

    class PointPowerup : Powerup
    {
        private double multiplier = 2;

        public PointPowerup(Point MazePt, Point PixelPt) : base(MazePt, PixelPt)
        {
            TypeName = "Points are x" + multiplier + " for " + maxDuration + " seconds!";
            Effects[2] = multiplier;
            TypeNumber = 3;
            shape.Fill = GameConstants.PowerUpColours[TypeNumber];
        }

        public override void Collect(Player collector)
        {
            base.Collect(collector);

            if (ReverseEffects)
            {
                //if the enemy collected the powerup, the effect is "flipped"
                Effects[2] = 0;
                TypeName = "Points have no value for " + maxDuration + " seconds!";
            }
        }
    }

    class Powerup
    {
        protected int maxDuration = 10;
        protected int currentActiveTime = 0;
        protected double[] Effects = new double[3] { 1, 1, 1 };
        //effects[0] = friendly speed multiplier
        //effects[1] = enemy speed multiplier
        //effects[2] = the multiplier for the points in the game, as the player collects them

        protected Point MazePt;
        protected Point PixelPt;
        protected Rectangle shape = new Rectangle();
        private bool Visible = true;
        protected bool ReverseEffects = false;
        //used to flip effects if the enemy collects the powerup

        protected string TypeName = "Null Effect";
        protected int TypeNumber = -1;
        protected Random rand = new Random();

        public Powerup(Point mazePt, Point pixelPt)
        {
            shape.Width = GameConstants.CellDimensions[0] / 3;
            shape.Height = GameConstants.CellDimensions[1] / 3;

            this.MazePt = mazePt;
            this.PixelPt = pixelPt;
            this.PixelPt.X += shape.Width;
            this.PixelPt.Y += shape.Height;

            Game.MW.GameCanvas.Children.Add(shape);
            Canvas.SetLeft(shape, this.PixelPt.X);
            Canvas.SetTop(shape, this.PixelPt.Y);

            maxDuration = rand.Next(5, 10);
        }

        public int GetTypeNumber()
        {
            return TypeNumber;
        }

        public string GetTypeOfPowerup()
        {
            return TypeName;
        }

        public void IncrementActiveTime()
        {
            currentActiveTime += 1;
        }

        public Point GetPixelPt()
        {
            return PixelPt;
        }

        public Point GetCurrentMazePt()
        {
            return MazePt;
        }

        public bool IsExpired()
        {
            if (currentActiveTime < maxDuration)
            {
                return false;
            }

            return true;
        }

        public bool IsVisible()
        {
            return Visible;
        }

        public virtual void Collect(Player collector)
        {
            currentActiveTime = 0;
            Visible = false;
            RemoveFromMap();

            ReverseEffects = (collector is Enemy);

            if (ReverseEffects)
            {
                //flips the effects so that the enemy receives the friendly effect, when it collects it
                double temporaryVal = Effects[0];
                Effects[0] = Effects[1];
                Effects[1] = temporaryVal;

                maxDuration = (int)Math.Truncate((maxDuration * 0.5));
            }

        }

        public bool IsReversed()
        {
            return ReverseEffects;
        }

        public double[] GetEffects()
        {
            return Effects;
        }

        public void RemoveFromMap()
        {
            Game.MW.GameCanvas.Children.Remove(shape);
        }

        public int GetMaxDuration()
        {
            return maxDuration;
        }

    }

    #endregion

    class Player
    {
        protected double CurrentMovementSpeed;
        protected static double DefaultMovementSpeed;
        protected Ellipse Shape = new Ellipse();
        protected Point CurrentMazePt = new Point();
        protected Point PixelPt = new Point();
        protected int CurrentDirection = -1;     //moving left at start
        private int Score = 0;
        private int PlayerNum;
        private int ScorePointValue = 1;
        private int DisplayNumber = 0;          ///used to reduce movement to every other frame
        private double UpdateFrequency = 1;

        private Point PixelPtChange = new Point(0, 0);        /// used during movement
        private Point NextPixelPt;
        private Point NextPoint;
        private Point CentrePixelPt = new Point();

        public Player(Point StartMazePt, Point StartPixelPt, GameConstants Constants, int Number)
        {
            //takes position of enemy in maze, and the corresponding pixelpoint

            SetCurrentCellPt(StartMazePt);
            SetPixelPt(StartPixelPt);

            DefaultMovementSpeed = GameConstants.difficulties.Last();
            SetSpeed(DefaultMovementSpeed);

            PlayerNum = Number;
            //player 1 has number 0, player 2 has number 1, Enemy has number 2

            if (PlayerNum > -1 && PlayerNum < GameConstants.PlayerColours.Count())
            {
                Shape.Fill = GameConstants.PlayerColours[PlayerNum];
            }

            StartPixelPt.X += GameConstants.WallThicknessProportion * 0.5 * GameConstants.CellDimensions[0];
            StartPixelPt.Y += GameConstants.WallThicknessProportion * 0.5 * GameConstants.CellDimensions[1];

            Shape.Width = GameConstants.CellDimensions[0] * (1 - GameConstants.WallThicknessProportion);
            Shape.Height = GameConstants.CellDimensions[1] * (1 - GameConstants.WallThicknessProportion);

            CentrePixelPt.X = StartPixelPt.X + Shape.Width / 2;
            CentrePixelPt.Y = StartPixelPt.Y + Shape.Width / 2;

            Game.MW.GameCanvas.Children.Add(Shape);
            Draw();
        }

        public void IncrementDisplayNumber()
        {
            DisplayNumber += 1;
        }

        public void ConvertMoveToChange(Point newPoint, Point newPixelPt)
        {
            NextPoint = newPoint;
            NextPixelPt = newPixelPt;
            PixelPtChange.X = (newPixelPt.X - PixelPt.X) / GameConstants.CellDimensions[0];
            PixelPtChange.Y = (newPixelPt.Y - PixelPt.Y) / GameConstants.CellDimensions[1];
        }

        public bool IsMoving()
        {
            if (PixelPtChange.X != 0 || PixelPtChange.Y != 0)
            {
                return true;
            }
            return false;
        }

        public void SetScoreValue(int value)
        {
            if (value >= 0 && value < 100)
            {
                ScorePointValue = value;
            }
        }

        public void IncrementPixelPt()
        {
            int TimestoIncrement = 0;
            //how many times the pixel position is incremented by 1

            if (CurrentMovementSpeed > 0)
            {
                if (UpdateFrequency == 1)
                {
                    TimestoIncrement = (int)Math.Truncate(CurrentMovementSpeed);
                }
                else if (DisplayNumber % UpdateFrequency == 0)
                {
                    TimestoIncrement = 1;
                }

                for (int i = 0; i < TimestoIncrement; i++)
                {
                    PixelPt.X += PixelPtChange.X;
                    PixelPt.Y += PixelPtChange.Y;

                    if (PixelPt == NextPixelPt)
                    {
                        PixelPtChange.X = 0;
                        PixelPtChange.Y = 0;
                        CurrentMazePt = NextPoint;

                        //if (NextDirection != -1)
                        //{
                        //    CurrentDirection = NextDirection;
                        //    NextDirection = -1;
                        //}
                    }
                }
            }
        }

        public double GetSpeed()
        {
            return CurrentMovementSpeed;
        }

        public int GetScorePointValue()
        {
            return ScorePointValue;
        }

        public void SetSpeed(double newSpeed)
        {
            if (newSpeed != 0)
            {
                CurrentMovementSpeed = newSpeed;
            }

            if (CurrentMovementSpeed < 1 && CurrentMovementSpeed > 0)
            {
                UpdateFrequency = 1 / CurrentMovementSpeed;
            }
            else
            {
                UpdateFrequency = 1;
            }

            DisplayNumber = 0;
        }

        public void IncrementScore(int Increment)
        {
            if (Increment > 0)
            {
                Score += Increment;
            }
        }

        public void RemoveFromMap()
        {
            Game.MW.GameCanvas.Children.Remove(Shape);
        }

        public int GetScore()
        {
            //to be used when storing each player's score
            return Score;
        }

        public int GetPlayerNum()
        {
            //0 = player 1, 1 = player 2...

            return PlayerNum;
        }

        public void SetCurrentCellPt(Point MazePt)
        {
            CurrentMazePt = MazePt;
            //SetPowerUpEffects();
        }

        public void SetPixelPt(Point NewPt)
        {
            PixelPt = NewPt;
        }

        public Point GetCurrentMazePt()
        {
            return CurrentMazePt;
        }

        public Point GetPixelPt()
        {
            return PixelPt;
        }

        public int GetDirection()
        {
            int toReturn = CurrentDirection;

            return toReturn;
        }

        public void Draw()
        {
            IncrementPixelPt();

            Canvas.SetLeft(Shape, PixelPt.X);
            Canvas.SetTop(Shape, PixelPt.Y);
        }

        public void UpdateDirection(Key thisKey, int type)
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
                    case Key.Up:
                        CurrentDirection = 0;
                        break;
                    case Key.Right:
                        CurrentDirection = 1;
                        break;
                    case Key.Down:
                        CurrentDirection = 2;
                        break;
                    case Key.Left:
                        CurrentDirection = 3;
                        break;
                    default:
                        break;
                }
            }

            if (type == 1)
            {
                CurrentDirection = -1;
            }
        }

    }

    class Enemy : Player
    {
        private Point Target;
        private Queue<int> DirectionsToFollow = new Queue<int>();

        public Enemy(Point StartMazePt, Point StartPixelPt, GameConstants Constants, int Number, double Difficulty) : base(StartMazePt, StartPixelPt, Constants, Number)
        {
            SetSpeed(Difficulty);
            DefaultMovementSpeed = Difficulty;
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

    class Cell
    {
        private Point ShapePt;
        private Dictionary<int, Edge> Edges = new Dictionary<int, Edge>();
        private ScorePt ScorePoint;

        public Cell(Point CanvasLoc, int[] cellDimensions)
        {
            ShapePt = CanvasLoc;
            ScorePoint = new ScorePt(ShapePt, cellDimensions);
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

        public bool HideScorePoint()
        {
            if (ScorePoint.GetVisible())
            {
                ScorePoint.Hide();
                return true;
                //returns true if the point had not been collected before
            }

            //returns false if it was already hidden
            return false;
        }

        public void DrawScorePoint()
        {
            ScorePoint.Draw();
        }

        public bool IsPointVisible()
        {
            return ScorePoint.GetVisible();
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

        public Point GetStartPoint()
        {
            //returns a specified vertex from the edge.
            return StartPoint;
        }

        public Point GetTargetPoint()
        {
            return TargetPoint;
        }
    }

    class ScorePt
    {
        private Ellipse Shape = new Ellipse();
        private Point PixelPt = new Point();
        private bool visible = true;
        public static Brush Colour = GameConstants.ScorePointColours[1];

        public ScorePt(Point PointParam, int[] cellDimensions)
        {
            Shape.Width = cellDimensions[0] / 5;
            Shape.Height = cellDimensions[1] / 5;

            Game.MW.GameCanvas.Children.Add(Shape);

            PixelPt = new Point(PointParam.X + 0.5 * (cellDimensions[0] - Shape.Width), PointParam.Y + 0.5 * (cellDimensions[1] - Shape.Height));
            Draw();
        }

        public void Hide()
        {
            Shape.Opacity = 0;
            visible = false;
        }

        public void Draw()
        {
            Shape.Fill = Colour;

            if (!visible)
            {
                Shape.Opacity = 255;
                visible = true;
            }

            Canvas.SetLeft(Shape, PixelPt.X);
            Canvas.SetTop(Shape, PixelPt.Y);
        }

        public bool GetVisible()
        {
            return visible;
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
        //the number of visible ScorePoints in the maze

        #region Maze Generation/Recursive Backtracker Variables
        private List<int> ValidMoves = new List<int>();
        private int randMoveSelection;
        private int thisMove;
        private Point nextCellPt = new Point();
        private Edge thisEdge;
        private Edge reverseEdge;
        private Random rand = new Random();
        private Stack<Point> VisitedCells = new Stack<Point>();
        private int RandNumOfWallsToRemove;
        private int TotalNumOfCells;
        #endregion

        public Maze(int[] MazeDimensions)
        {
            ///number of grid spaces in the maze
            this.MazeDimensions = MazeDimensions;
            this.CellDimensions = GameConstants.CellDimensions;
            Thickness = (int)(CellDimensions[0] * GameConstants.WallThicknessProportion);

            AllWallsH = new Wall[MazeDimensions[0], MazeDimensions[1] + 1];
            AllWallsV = new Wall[MazeDimensions[0] + 1, MazeDimensions[1]];
            ///adds one on right/bottom edge for each array to "close" box

            Cells = new Cell[MazeDimensions[0], MazeDimensions[1]];

            InitialiseMaze();
            //creates instances of each wall/cell and sets their x/y coordinates
            DrawGrid();

            //Generates recursive maze starting at point 0,0 and continuing until all cells have been visited
            TotalNumOfCells = MazeDimensions[0] * MazeDimensions[1];
            NumOfActivePoints = TotalNumOfCells;

            MazeGeneration(GetLastCellInMaze(), false);
            //the false indicates that it should not start by backtracking
        }

        public int[] GetMazeDimensions()
        {
            return MazeDimensions;
        }

        public Point GetLastCellInMaze()
        {
            return new Point(MazeDimensions[0] - 1, MazeDimensions[1] - 1);
        }

        public Point GetFirstCellInMaze()
        {
            return new Point(0, 0);
        }

        public Point GetBottomLeftCell()
        {
            return new Point(0, MazeDimensions[1] - 1);
        }

        public Point GetTopRightCell()
        {
            return new Point(MazeDimensions[0] - 1, 0);
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

        public Point ConvertPixelPtToMazePt(Point PixelPt)
        {
            Point MazeTopLeft = new Point(GameConstants.MazeIndent[0] + Thickness, GameConstants.MazeIndent[1] + Thickness);

            PixelPt.X -= MazeTopLeft.X;
            PixelPt.Y -= MazeTopLeft.Y;

            PixelPt.X /= (CellDimensions[0] + Thickness);
            PixelPt.Y /= (CellDimensions[1] + Thickness);

            PixelPt.X = (int)Math.Round(PixelPt.X);
            PixelPt.Y = (int)Math.Round(PixelPt.Y);

            return PixelPt;
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
                    Point CanvasPoint = AllWallsH[x, y].GetPixelPt();
                    Point MazePoint = new Point(x, y);

                    CanvasPoint.X += Thickness;
                    CanvasPoint.Y += Thickness;

                    Cells[x, y] = new Cell(CanvasPoint, CellDimensions);
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

        private void MazeGeneration(Point currentCellPt, bool IsBacktracking)
        {
            ///RECURSIVE BACKRTRACKER
            ///Depth First Maze Generation Algorithm which uses recursive calls to step through cells

            //stores maze coordinates of maze cell currently being processed by algorithm
            //thisCell = Cells[(int)currentCellPt.X, (int)currentCellPt.Y];

            GetAdjacentDirections(currentCellPt, IsBacktracking);
            //gets all possible moves from the current cell to adjacent ones

            if (ValidMoves.Count > 0)
            {
                VisitedCells.Push(currentCellPt);

                RandNumOfWallsToRemove = rand.Next(1, ValidMoves.Count());

                for (int i = 0; i < RandNumOfWallsToRemove; i++)
                {
                    randMoveSelection = rand.Next(0, ValidMoves.Count);
                    thisMove = ValidMoves[randMoveSelection];
                    //gets a random valid move from the current cell to one of its neighbours

                    ValidMoves.RemoveAt(randMoveSelection);

                    nextCellPt = MoveFromPoint(currentCellPt, thisMove);
                    thisEdge = new Edge(currentCellPt, nextCellPt);

                    Cells[(int)currentCellPt.X, (int)currentCellPt.Y].AddEdge(thisEdge, thisMove);
                    //adds edge to the current cell

                    reverseEdge = new Edge(thisEdge.GetTargetPoint(), thisEdge.GetStartPoint());
                    //flips the edge so its inverse can be stored in the other cell

                    TurnOffWall(nextCellPt, thisMove);

                    Cells[(int)nextCellPt.X, (int)nextCellPt.Y].AddEdge(reverseEdge, ReverseMove(thisMove));
                    //adds opposite edge to the next cell
                }

                if (VisitedCells.Count < TotalNumOfCells)
                {
                    IsBacktracking = false;
                    MazeGeneration(nextCellPt, IsBacktracking);
                }

            }
            else if (VisitedCells.Count > 0)
            {
                IsBacktracking = true;
                MazeGeneration(VisitedCells.Pop(), IsBacktracking);
                //Gets the last cell to be visited and recursively calls algorithm from that cell
            }
        }

        private void TurnOffWall(Point thisCell, int direction)
        {
            //hides the wall in the specified direction

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

        private void GetAdjacentDirections(Point Current, bool IsBacktracking)
        {
            ///Note: this method is for the backtracker algorithm to find the possible directions it can move in
            ///it runs BEFORE the graph of cells is created

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

            ValidMoves.Clear();

            if (IsBacktracking == false)
            {
                //enters this statement when following new path of unvisited cells

                for (int i = 0; i < AdjacentPoints.Count; i++)
                {
                    if (IsValidCell(AdjacentPoints[i]) && !VisitedCells.Contains(AdjacentPoints[i]) && Cells[(int)Current.X, (int)Current.Y].GetEdgesCount() < 4)
                    {
                        ValidMoves.Add(i);
                        //determines which adjacent cells have not already been explored
                        //creates list of valid directions from current cell
                    }
                }
            }
            else if (IsBacktracking)
            {
                //enters this statement when backtracking along path

                for (int i = 0; i < AdjacentPoints.Count; i++)
                {
                    if (IsValidCell(AdjacentPoints[i]) && Cells[(int)Current.X, (int)Current.Y].GetEdgeFromDirection(i) == null && Cells[(int)AdjacentPoints[i].X, (int)AdjacentPoints[i].Y].GetEdgesCount() == 0 && Cells[(int)Current.X, (int)Current.Y].GetEdgesCount() < 4)
                    {
                        ValidMoves.Add(i);
                        //determines which adjacent cells (if any) have not already been explored
                    }
                }
            }
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
                    PointsList.Add(currentEdge.GetTargetPoint());
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

        public double GetApproximateDistance(Point current, Point target)
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

        public int PointCrossed(Point newPoint, double Value)
        {
            int IncrementVal = 0;

            if (NumOfActivePoints < 1)
            {
                //if there are no visible collectible points, they are all redrawn
                DrawAllScorePoints();
                IncrementVal += (int)(Value * GameConstants.ClearPointsValue);
            }

            bool Unvisited = Cells[(int)newPoint.X, (int)newPoint.Y].HideScorePoint();

            if (Unvisited)
            {
                NumOfActivePoints -= 1;
                IncrementVal += (int)(1 * Value);
            }

            return IncrementVal;
        }

        private void DrawAllScorePoints()
        {
            foreach (var cell in Cells)
            {
                cell.DrawScorePoint();
            }

            NumOfActivePoints = MazeDimensions[0] * MazeDimensions[1];
        }

        public void SetScorePointColour(double effectVal)
        {
            int ColourIndex = (int)Math.Truncate(effectVal);

            if (ColourIndex >= 0 && ColourIndex < GameConstants.ScorePointColours.Count())
            {
                ScorePt.Colour = GameConstants.ScorePointColours[ColourIndex];
            }

            foreach (var cell in Cells)
            {
                if (cell.IsPointVisible())
                {
                    cell.DrawScorePoint();
                }
            }
        }
    }

    class Wall
    {
        private double width;
        private double height;
        private Rectangle Shape = new Rectangle { Fill = GameConstants.WallColours[0], };
        private Point PixelPt;
        private bool hideable = true;                 //used to prevent hiding outer edge walls

        public Wall(char typeParam, int i, int j, int[] cellDimensions, int[] mazeDimensions, int thickness)
        {
            //sets size of wall based on whether horizontal or vertical

            if (typeParam == 'h')
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
            else if (typeParam == 'v')
            {
                //vertical wall conditions

                width = thickness;
                height = cellDimensions[1] + thickness;

                if (i == 0 || i == mazeDimensions[0])
                {
                    //walls can be hidden unless they are on the outer edge of the maze
                    hideable = false;
                }
            }

            Shape.Width = this.width;
            Shape.Height = this.height;

            PixelPt.X = GameConstants.MazeIndent[0] + (i + 1) * cellDimensions[0];
            PixelPt.Y = GameConstants.MazeIndent[1] + (j + 1) * cellDimensions[1];
            //calculates position of wall based on location in array

            Game.MW.GameCanvas.Children.Add(Shape);
        }

        public Point GetPixelPt()
        {
            return PixelPt;
        }

        public void Hide()
        {
            if (hideable)
            {
                Game.MW.GameCanvas.Children.Remove(Shape);
            }
        }

        public void Draw()
        {
            Canvas.SetLeft(Shape, PixelPt.X);
            Canvas.SetTop(Shape, PixelPt.Y);
        }

    }

    public class DataEntry
    {
        //these need to be public for the JSON to write their value correctly
        public int GameID { get; set; }
        public List<string> PlayerNames { get; set; } = new List<string>();
        public List<int> PlayerScores { get; set; } = new List<int>();
        public string Timestamp { get; set; }
        public int[] mazeDimensions { get; set; }
        public int difficulty { get; set; }
        public int SurvivedFor { get; set; }
        //

        public void AddPlayer(string name, int score)
        {
            PlayerNames.Add(name);
            PlayerScores.Add(score);
        }

        public void SetOtherGameVariables(int GameID, int difficulty, int currentTime, int[] mazeDimensions)
        {
            this.GameID = GameID;
            this.difficulty = difficulty;
            this.mazeDimensions = mazeDimensions;
            SurvivedFor = currentTime;
            Timestamp = DateTime.Now.ToString();
        }

        public int GetNumberOfPlayers()
        {
            return PlayerNames.Count;
        }

        public int GetHighestScore()
        {
            int LargestSoFar = 0;

            foreach (var score in PlayerScores)
            {
                if (score > LargestSoFar)
                {
                    LargestSoFar = score;
                }
            }

            return LargestSoFar;
        }

        public string GetNameofHighestScore()
        {
            int lastScore = 0;
            int index = 0;

            for (int i = 0; i < PlayerNames.Count; i++)
            {
                if (PlayerScores[i] > lastScore)
                {
                    lastScore = PlayerScores[i];
                    index = i;
                }
            }

            return PlayerNames[index];
        }

        public int GetTotalScore()
        {
            int Total = 0;

            foreach (var score in PlayerScores)
            {
                Total += score;
            }

            return Total;
        }

        public int GetScoreFromName(string NameToFind, bool IncludeCapitals)
        {
            int Index = SearchPlayers(NameToFind, IncludeCapitals);

            if (Index > -1 && Index < PlayerScores.Count())
            {
                return PlayerScores[Index];
            }

            return -1;
        }

        public int SearchPlayers(string toFind, bool IncludeCapitals)
        {
            //searches through all the game saves in the file, checking the players in each

            for (int i = 0; i < PlayerNames.Count(); i++)
            {
                if (PlayerNames[i] == toFind)
                {
                    return i;
                }
                else if (IncludeCapitals && toFind.ToLower() == PlayerNames[i].ToLower())
                {
                    return i;    
                }
            }

            return -1;
        }
    }

    class Game
    {
        public static GameWindow MW { get; set; }                 ////allows access to canvas outside of main window
        private Maze MazeOne;
        private int[] MazeDimensions;
        private const int DisplayRefreshConstant = 16;          ///number of milliseconds between each refresh of the entities

        private DispatcherTimer GameTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1), };
        private DispatcherTimer MovementTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, DisplayRefreshConstant), };

        private List<Player> ActivePlayers = new List<Player>();
        private List<Enemy> ActiveEnemies = new List<Enemy>();

        private Player[] RemovedPlayers;
        private GameConstants Constants;
        //initialised in constructor

        private int[] CountOfPowerupType = new int[4] { 0, 0, 0, 0 };
        private List<Powerup> VisiblePowerups = new List<Powerup>();
        //stores powerups that can be seen in the maze
        private SimplePriorityQueue<Powerup, int> AppliedPowerUpEffects = new SimplePriorityQueue<Powerup, int>();
        //stores powerups which have been collected and the effect is being applied, 
        //with the priority as the powerup duration

        private int NumOfPlayers = 1;
        private int currentTime = 0;
        private int TotalScore = 0;
        private double Difficulty = GameConstants.difficulties.Last();
        //this is set just in case it is not passed by the Settings Window (default = 2 is normal speed)

        public Game(GameWindow thisWindow, int[] MazeDimensions, int[] CellDimensions, bool TwoPlayers, double EnemyDifficulty)
        {
            MW = thisWindow;
            MW.GameCanvas.Background = GameConstants.BackgroundColour;

            this.MazeDimensions = MazeDimensions;

            if (TwoPlayers)
            {
                NumOfPlayers = 2;
            }

            Constants = new GameConstants();
            Difficulty = EnemyDifficulty;

            RemovedPlayers = new Player[NumOfPlayers];

            CreateMaze(MazeDimensions);

            GameTimer.Tick += GameTimer_Tick;
            MovementTimer.Tick += MovementTimer_Tick;

            thisWindow.TimeDisplayTXT.Foreground = GameConstants.ForegroundColour;
            thisWindow.ScoreDisplayTXT.Foreground = GameConstants.ForegroundColour;
            thisWindow.PowerupInfoBlock.Background = GameConstants.BackgroundColour;
            thisWindow.PowerupInfoBlock.Foreground = GameConstants.ForegroundColour;

            MazeOne.SetScorePointColour(1);
        }

        public void UpdatePlayerMovement(Key thisKey, int type)
        {
            foreach (var Player in ActivePlayers)
            {
                Player.UpdateDirection(thisKey, type);
            }
        }

        public Point GenerateRandomLocation()
        {
            //generates a random location to place the powerup
            Random rand = new Random();
            Point GeneratedPt = new Point();

            GeneratedPt.X = rand.Next(0, MazeDimensions[0]);
            GeneratedPt.Y = rand.Next(0, MazeDimensions[1]);

            return GeneratedPt;
        }

        public void CollectPoint(Player thisPlayer)
        {
            int IncrementScore = MazeOne.PointCrossed(thisPlayer.GetCurrentMazePt(), thisPlayer.GetScorePointValue());

            thisPlayer.IncrementScore(IncrementScore);
            TotalScore += IncrementScore;

            if (IncrementScore > 10)
            {
                //if the board was cleared
            }
        }

        private void MovementTimer_Tick(object sender, EventArgs e)
        {
            List<int> ExpiredPowerupIndexes = new List<int>();

            foreach (Player player in ActivePlayers)
            {
                player.IncrementDisplayNumber();
                CheckTouchingPowerup(player);

                if (!player.IsMoving())
                {
                    CollectPoint(player);
                    UpdatePlayerPosition(player);
                }

                player.Draw();
            }

            foreach (Enemy enemy in ActiveEnemies)
            {
                enemy.IncrementDisplayNumber();
                CheckTouches(enemy);
                CheckTouchingPowerup(enemy);

                if (!enemy.IsMoving())
                {
                    enemy.UpdateDirection();
                    UpdatePlayerPosition(enemy);
                    FindShortestPath(enemy);
                }

                enemy.Draw();
            }

            for (int i = 0; i < VisiblePowerups.Count; i++)
            {
                if (VisiblePowerups[i].IsExpired())
                {
                    VisiblePowerups[i].RemoveFromMap();
                    CountOfPowerupType[VisiblePowerups[i].GetTypeNumber()] -= 1;
                    ExpiredPowerupIndexes.Add(i);
                }
            }

            foreach (int i in ExpiredPowerupIndexes)
            {
                if (i > -1 && i < VisiblePowerups.Count())
                {
                    VisiblePowerups.RemoveAt(i);
                }
            }

            UpdateScoreAndTime();

            if (ActivePlayers.Count() < 1)
            {
                EndGame();
            }
        }

        private void RemovePowerupEffect(Powerup thisPowerup)
        {
            double[] effects = thisPowerup.GetEffects();
            double currentSpeed;

            CountOfPowerupType[thisPowerup.GetTypeNumber()] -= 1;

            for (int i = 0; i < ActivePlayers.Count; i++)
            {
                currentSpeed = ActivePlayers[i].GetSpeed();

                ActivePlayers[i].SetSpeed(currentSpeed / effects[0]);
                ActivePlayers[i].SetScoreValue(1);
            }

            for (int i = 0; i < ActiveEnemies.Count; i++)
            {
                currentSpeed = ActiveEnemies[i].GetSpeed();
                ActiveEnemies[i].SetSpeed(currentSpeed / effects[1]);
            }

            if (effects[2] != 1)
            {
                MazeOne.SetScorePointColour(1);
            }

            RemoveFromPowerupTextBlock(thisPowerup.GetTypeOfPowerup());
            //removes the effect text from the side panel
        }

        private void RemoveFromPowerupTextBlock(string ToRemove)
        {
            string Contents = MW.PowerupInfoBlock.Text;

            int index = Contents.IndexOf(ToRemove);

            Contents = Contents.Remove(index, ToRemove.Length + Environment.NewLine.Length);

            MW.PowerupInfoBlock.Text = Contents;
        }

        private void CheckTouches(Player EnemyToCheck)
        {
            Point EnemyPixel = EnemyToCheck.GetPixelPt();
            double dx = 0;
            double dy = 0;
            int playerNum = -1;

            for (int i = 0; i < ActivePlayers.Count(); i++)
            {
                //cycles through all players in the maze

                //finds the pixel distance between the two entities
                dx = Math.Abs(EnemyPixel.X - ActivePlayers[i].GetPixelPt().X);
                dy = Math.Abs(EnemyPixel.Y - ActivePlayers[i].GetPixelPt().Y);

                //if (*EnemyToCheck.GetCurrentMazePt() == ActivePlayers[i].GetCurrentMazePt() || 
                if ((dx < GameConstants.CellDimensions[0] / 2 && dy < GameConstants.CellDimensions[1] / 2))
                {
                    //if the enemy is within half a cell of the player, they are "touching"

                    ActivePlayers[i].RemoveFromMap();
                    //removes the visual aspect of the player

                    playerNum = ActivePlayers[i].GetPlayerNum();

                    RemovedPlayers[playerNum] = ActivePlayers[i];
                    //stores the player in another structure so that its score can be retrieved later

                }
            }

            //has to be removed from the list after it checks through all players

            if (playerNum > -1 && playerNum < RemovedPlayers.Length)
            {
                ActivePlayers.Remove(RemovedPlayers[playerNum]);
                //removes that player from the game so it cannot be tracked by enemy

            }
        }

        private void CheckTouchingPowerup(Player EntityToCheck)
        {
            Point currentPixelPt = EntityToCheck.GetPixelPt();

            double dx;
            double dy;

            for (int i = 0; i < VisiblePowerups.Count; i++)
            {
                //pixel distances between the entity and the powerup
                dx = Math.Abs(currentPixelPt.X - VisiblePowerups[i].GetPixelPt().X);
                dy = Math.Abs(currentPixelPt.Y - VisiblePowerups[i].GetPixelPt().Y);

                if (dx < GameConstants.CellDimensions[0] / 2 && dy < GameConstants.CellDimensions[1] / 2)
                {
                    ApplyPowerupEffect(VisiblePowerups[i], EntityToCheck);
                }
            }
        }

        private void GenerateRandomPowerUp(int type)
        {
            Random rand = new Random();
            Point MazeLocation = GenerateRandomLocation();     //gets random location inside the maze                ///needs to be amended to ensure it is certain distance from entities
            Point PixelPt = MazeOne.GetPixelPoint(MazeLocation);

            Powerup PowerupToAdd = new Powerup(MazeLocation, PixelPt);

            if (MazeLocation != new Point(0, 0) && CountOfPowerupType[type] < 1)
            {
                switch (type % 4)
                {
                    case 0:
                        PowerupToAdd = new SpeedUpPowerup(MazeLocation, PixelPt);
                        break;
                    case 1:
                        PowerupToAdd = new SpeedDownPowerup(MazeLocation, PixelPt);
                        break;
                    case 2:
                        PowerupToAdd = new FreezePowerup(MazeLocation, PixelPt);
                        break;
                    case 3:
                        PowerupToAdd = new PointPowerup(MazeLocation, PixelPt);
                        break;
                    default:
                        break;
                }

                CountOfPowerupType[type] += 1;
                VisiblePowerups.Add(PowerupToAdd);
            }
        }

        private void ApplyPowerupEffect(Powerup thisPowerup, Player CollectedBy)
        {
            double[] Effects;
            double currentSpeed;
            bool appliedEffect = true;

            thisPowerup.Collect(CollectedBy);
            VisiblePowerups.Remove(thisPowerup);

            Effects = thisPowerup.GetEffects();

            foreach (var player in ActivePlayers)
            {
                if (player.GetSpeed() <= 0)
                {
                    appliedEffect = false;
                    //doesn't apply if player already frozen
                }
            }

            foreach (var enemy in ActiveEnemies)
            {
                if (enemy.GetSpeed() <= 0)
                {
                    appliedEffect = false;
                    //doesn't apply effect if the enemy is already frozen
                }
            }

            if (appliedEffect == true)
            {
                //if neither entity is frozen
                AppliedPowerUpEffects.Enqueue(thisPowerup, thisPowerup.GetMaxDuration());
                //enqueues powerup based on how long they last -> shortest will be checked/removed earlier
            
                foreach (var player in ActivePlayers)
                {
                    if (Effects[0] != 1)
                    {
                        currentSpeed = player.GetSpeed();
                        player.SetSpeed(Effects[0] * currentSpeed);
                    }

                    if (Effects[2] >= 0 && Effects[2] != 1)
                    {
                        player.SetScoreValue((int)Effects[2]);
                        MazeOne.SetScorePointColour(Effects[2]);
                    }

                    //sets the friendly effect for all players on same team as the player who collected it
                    //e.g. if player 1 or 2 collects it, both get the same friendly effect
                    //e.g. if the enemy gets it, neither player gets the effect
                }

                foreach (var enemy in ActiveEnemies)
                {
                    if (Effects[1] != 1)
                    {
                        currentSpeed = enemy.GetSpeed();

                        //only stacks effect on speed up/down, not freeze
                        //-1 * -1 = +1, so freeze stacks would not work
                        enemy.SetSpeed(Effects[1] * currentSpeed);
                    }

                }

                MW.PowerupInfoBlock.Text += thisPowerup.GetTypeOfPowerup() + Environment.NewLine;
            }

        }

        #region Saving Results To File

        private void AddEntryToFile()
        {
            List<DataEntry> FileEntries = new List<DataEntry>();
            JsonSerializer Serializer = new JsonSerializer();
            DataEntry thisEntry = new DataEntry();
            int ID;

            thisEntry = AddPlayersToEntry(thisEntry);

            using (StreamReader reader = new StreamReader(GameConstants.FileName))
            {
                using (JsonTextReader jreader = new JsonTextReader(reader))
                {
                    FileEntries = (List<DataEntry>)Serializer.Deserialize(jreader, FileEntries.GetType());
                    //gets all entries from the file and stores in a list of data entries
                }
            }


            if (FileEntries != null)
            {
                //if the file contains at least one previous game
                ID = FileEntries.Last().GameID + 1;
            }
            else
            {
                //if the file was empty
                ID = 1;
                FileEntries = new List<DataEntry>();
            }

            thisEntry.SetOtherGameVariables(ID, (int)Difficulty, currentTime, MazeOne.GetMazeDimensions());

            FileEntries.Add(thisEntry);

            using (StreamWriter writer = new StreamWriter(GameConstants.FileName))
            {
                using (JsonWriter jwriter = new JsonTextWriter(writer))
                {
                    Serializer.Serialize(jwriter, FileEntries);
                    //stores the list of all entries back in the file
                }
            }

            MessageBox.Show("Game Saved");
        }

        private DataEntry AddPlayersToEntry(DataEntry thisEntry)
        {
            List<string> ChosenNames = new List<string>();
            string thisName;
            bool valid = false;

            for (int i = 0; i < RemovedPlayers.Count(); i++)
            {
                do
                {
                    thisName = NamePlayers.GetName(i+1, ChosenNames);

                    if (thisName.Length < 1 || thisName.ToLower() == "anonymous")
                    {
                        thisName = "anonymous";
                        valid = true;
                    }
                    else if (ChosenNames.Contains(thisName))
                    {
                        valid = false;
                        MessageBox.Show("Name already used by other player for this game. Please try again with a different name.");
                    }
                    else
                    {
                        valid = true;
                    }

                } while (valid == false);

                ChosenNames.Add(thisName);
                MessageBox.Show("Saving Player " + (i+1) + " as '" + thisName + "'");

                thisEntry.AddPlayer(thisName, RemovedPlayers[i].GetScore());
            }

            return thisEntry;
        }

        #endregion

        private void EndGame()
        {
            string message;
            GameTimer.Stop();
            MovementTimer.Stop();

            message = "You collected " + TotalScore + " point";

            if (TotalScore > 1)
            {
                message += "s";
            }
            message += ", and survived for " + currentTime + " seconds";

            MessageBox.Show(message);

            AddEntryToFile();

            new MainWindow().Show();
            MW.Close();
        }

        private void UpdatePlayerPosition(Player Entity)
        {
            Point currentPoint = Entity.GetCurrentMazePt();
            int direction = Entity.GetDirection();
            Point newPoint;
            Point newPixelPoint;
            if (direction >= 0 && direction <= 3)
            {
                if (MazeOne.CheckEdgeInDirection(currentPoint, direction))
                {
                    //checks that there is an edge between those cells
                    newPoint = MazeOne.MoveFromPoint(currentPoint, direction);
                    Entity.SetCurrentCellPt(newPoint);

                    newPixelPoint = MazeOne.GetPixelPoint(newPoint);
                    Entity.ConvertMoveToChange(newPoint, newPixelPoint);
                    //gets the new position in the window
                }
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            Random rand = new Random();
            int randVal = rand.Next(5, 21);
            currentTime += 1;
            //updates the length of time the game has been open, once per second

            List<Powerup> PowerupsToRemove = new List<Powerup>();

            for (int i = 0; i < VisiblePowerups.Count; i++)
            {
                VisiblePowerups[i].IncrementActiveTime();
                //increments each powerups time by 1

                if (VisiblePowerups[i].IsExpired())
                {
                    //if they have been visible for more than their maximum duration, 
                    //then they are removed from the map and from the list of powerups
                    VisiblePowerups[i].RemoveFromMap();
                    CountOfPowerupType[VisiblePowerups[i].GetTypeNumber()] -= 1;
                    PowerupsToRemove.Add(VisiblePowerups[i]);
                }
            }

            foreach (var powerup in AppliedPowerUpEffects)
            {
                powerup.IncrementActiveTime();
            }

            foreach (var powerup in PowerupsToRemove)
            {
                VisiblePowerups.Remove(powerup);
            }

            if (AppliedPowerUpEffects.Count > 0)
            {
                Powerup NextEffectToExpire = AppliedPowerUpEffects.Dequeue();

                if (NextEffectToExpire.IsExpired())
                {
                    RemovePowerupEffect(NextEffectToExpire);
                }
                else
                {
                    AppliedPowerUpEffects.Enqueue(NextEffectToExpire, NextEffectToExpire.GetMaxDuration());
                }
            }

            int type = rand.Next(0, 4);
            GenerateRandomPowerUp(type);

        }

        private void UpdateScoreAndTime()
        {
            MW.TimeDisplayTXT.Content = Convert.ToString(currentTime);
            MW.ScoreDisplayTXT.Content = Convert.ToString(TotalScore);
        }

        public void CreateMaze(int[] MazeDim)
        {
            currentTime = 0;

            MazeOne = new Maze(MazeDim);

            AddEntities();
            GameTimer.Start();
            MovementTimer.Start();
        }

        private void AddEntities()
        {
            Point StartPoint = MazeOne.GetFirstCellInMaze();
            Point StartPointPixelPt = MazeOne.GetPixelPoint(StartPoint);
            Point LastPoint = MazeOne.GetLastCellInMaze();
            Point LastPointPixelPt = MazeOne.GetPixelPoint(LastPoint);

            ActivePlayers.Clear();
            ActiveEnemies.Clear();

            ActivePlayers.Add(new Player(StartPoint, StartPointPixelPt, Constants, 0));
            ActiveEnemies.Add(new Enemy(LastPoint, LastPointPixelPt, Constants, 2, Difficulty));

            if (NumOfPlayers == 2)
            {
                StartPoint = MazeOne.GetBottomLeftCell();
                StartPointPixelPt = MazeOne.GetPixelPoint(StartPoint);
                ActivePlayers.Add(new Player(StartPoint, StartPointPixelPt, Constants, 1));

                //LastPoint = MazeOne.GetTopRightCell(0);
                //LastPointPixelPt = MazeOne.GetTopRightCell(1);
                //ActiveEnemies.Add(new Enemy(LastPoint, LastPointPixelPt, Constants, 2, Difficulty));
            }

            //FindShortestPath(ActiveEnemies[0]);
        }

        private void FindShortestPath(Enemy enemyParam)
        {
            Queue<int> ShortestPath = new Queue<int>();
            Queue<int> AlternatePath = new Queue<int>();
            Point target = new Point(0, 0);

            double directDistance = 0;
            double shortestDistance = 0;

            int closestPowerupIndex = 0;
            int closestPlayerIndex = 0;

            foreach (var player in ActivePlayers)
            {
                directDistance = MazeOne.GetApproximateDistance(enemyParam.GetCurrentMazePt(), player.GetCurrentMazePt());

                if (directDistance < shortestDistance || shortestDistance == 0)
                {
                    shortestDistance = directDistance;
                    closestPlayerIndex = ActivePlayers.IndexOf(player);
                }

                //finds closest player to current location
            }

            shortestDistance = 0;
            directDistance = 0;

            foreach (var powerup in VisiblePowerups)
            {
                directDistance = MazeOne.GetApproximateDistance(enemyParam.GetCurrentMazePt(), powerup.GetCurrentMazePt());

                if (directDistance < shortestDistance || shortestDistance == 0)
                {
                    shortestDistance = directDistance;
                    closestPowerupIndex = VisiblePowerups.IndexOf(powerup);
                }

                //finds the closest powerup to the current location
            }

            if (ActivePlayers.Count > 0)
            {
                ShortestPath = MazeOne.GeneratePathToTarget(ActivePlayers[closestPlayerIndex].GetCurrentMazePt(), enemyParam.GetCurrentMazePt());

                if (VisiblePowerups.Count > 0)
                {
                    AlternatePath = MazeOne.GeneratePathToTarget(VisiblePowerups[closestPowerupIndex].GetCurrentMazePt(), enemyParam.GetCurrentMazePt());
                }

                if (ShortestPath.Count > 0 && AlternatePath.Count == 0 || ShortestPath.Count < AlternatePath.Count * (4 - Difficulty))
                {
                    target = ActivePlayers[closestPlayerIndex].GetCurrentMazePt();
                    enemyParam.UpdatePath(ShortestPath);
                    enemyParam.SetTarget(target);
                }
                else if (AlternatePath.Count > 0)
                {
                    target = VisiblePowerups[closestPowerupIndex].GetCurrentMazePt();
                    enemyParam.UpdatePath(AlternatePath);
                    enemyParam.SetTarget(target);
                }
            }

            #region OldPathFinding - generate paths for each player/powerup and compare

            //if (ActivePlayers.Count > 0)
            //{
            //    ShortestPath = MazeOne.GeneratePathToTarget(ActivePlayers[0].GetCurrentMazePt(), enemyParam.GetCurrentMazePt());

            //    for (int i = 1; i < ActivePlayers.Count; i++)
            //    {
            //        AlternatePath = MazeOne.GeneratePathToTarget(ActivePlayers[i].GetCurrentMazePt(), enemyParam.GetCurrentMazePt());

            //        if (AlternatePath.Count() < ShortestPath.Count())
            //        {
            //            ShortestPath = AlternatePath;
            //            target = ActivePlayers[i].GetCurrentMazePt();
            //        }
            //    }

            //    foreach (var powerup in VisiblePowerups)
            //    {
            //        AlternatePath = MazeOne.GeneratePathToTarget(powerup.GetCurrentMazePt(), enemyParam.GetCurrentMazePt());

            //        if (AlternatePath.Count() * 4 < ShortestPath.Count())
            //        {
            //            ShortestPath = AlternatePath;
            //            target = powerup.GetCurrentMazePt();
            //        }
            //    }

            #endregion
        }
    }

    public partial class GameWindow : Window
    {
        Game newGame;
        bool UseClassicControls = true;

        public GameWindow(int[] MazeDimensions, bool TwoPlayers, double EnemyDifficulty, bool ClassicControls)
        {
            InitializeComponent();

            UseClassicControls = ClassicControls;

            this.Width = GameConstants.WinIndent[0] + (MazeDimensions[0] + 3) * GameConstants.CellDimensions[0];
            this.Height = GameConstants.WinIndent[1] + (MazeDimensions[1] + 3) * GameConstants.CellDimensions[1];

            this.ResizeMode = ResizeMode.NoResize;

            InfoTitleLbl.Foreground = GameConstants.ForegroundColour;
            ScoreLbl.Foreground = GameConstants.ForegroundColour;
            TimeLbl.Foreground = GameConstants.ForegroundColour;
            GameCanvas.Background = GameConstants.BackgroundColour;

            newGame = new Game(this, MazeDimensions, GameConstants.CellDimensions, TwoPlayers, EnemyDifficulty);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            newGame.UpdatePlayerMovement(e.Key, 0);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (UseClassicControls)
            {
                newGame.UpdatePlayerMovement(e.Key, 1);
            }

        }
    }
}
