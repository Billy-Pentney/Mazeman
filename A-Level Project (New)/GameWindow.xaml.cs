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
        //24 used because it is divisible by 1, 2, 3 (the movement speeds)
        public int WallThickness { get; }

        public static int[] indent { get; } = new int[2] { 40, 0 };
        //the pixel values used to indent the maze from the left/top of the window

        public const string FileName = "History.txt";
        //the name/address of the file where scores should be written to/read from

        public GameConstants()
        {
            WallThickness = CellDimensions[0] / 10;
        }

        public const double DefaultMovementSpeed = 4;
        //the player movement speed with no powerups

        public static Brush[] PlayerColours { get; } = new Brush[] { Brushes.Yellow, Brushes.Blue, Brushes.Red };
        public static Brush[] PowerUpColours { get; } = new Brush[] { Brushes.Orange, Brushes.LimeGreen, Brushes.LightBlue, Brushes.Purple };
        public static Brush[] ScorePointColours { get; } = new Brush[] { Brushes.Gray, Brushes.Orange, Brushes.Purple };

        public static Brush BackgroundColour { get; } = Brushes.White;
        public static Brush ForegroundColour { get; } = Brushes.Black;
    }

    #region Powerup CLASSES

    //class OldPowerup
    //{
    //    private int type = 0;
    //    private double FriendlyEffect;
    //    private double EnemyEffect;
    //    private double ScorePointEffect;
    //    private Rectangle shape = new Rectangle();
    //    private Point MazePt;
    //    private Point PixelPt;
    //    private DispatcherTimer thisTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) };
    //    private int MaxDuration = 10;
    //    private int CurrentTime = 0;
    //    private bool ReverseEffects = false;
    //    private bool collected = false;
    //    private double[] PlayerSpeeds;
    //    private double[] EnemySpeeds;

    //    public OldPowerup(GameConstants constants, Point MazePt, Point PixelPt, int type)
    //    {
    //        shape.Width = GameConstants.CellDimensions[0] / 3;
    //        shape.Height = GameConstants.CellDimensions[1] / 3;
    //        this.type = type;

    //        if (type > -1 && type < GameConstants.NumOfPowerupTypes)
    //        {
    //            SetEffects();
    //            PixelPt.X += GameConstants.CellDimensions[0] / 3;       //effectively centres the powerup in the cell
    //            PixelPt.Y += GameConstants.CellDimensions[1] / 3;

    //            shape.Fill = GameConstants.PowerUpColours[type];
    //            Game.MW.GameCanvas.Children.Add(shape);
    //            this.PixelPt = PixelPt;
    //            this.MazePt = MazePt;
    //            Canvas.SetLeft(shape, PixelPt.X);
    //            Canvas.SetTop(shape, PixelPt.Y);

    //            thisTimer.Start();
    //            thisTimer.Tick += ThisTimer_Tick;
    //        }
    //    }

    //    public Point GetMazePt()
    //    {
    //        return MazePt;
    //    }

    //    private void ThisTimer_Tick(object sender, EventArgs e)
    //    {
    //        CurrentTime += 1;
    //    }

    //    public void Collected()
    //    {
    //        thisTimer.Stop();
    //        CurrentTime = 0;
    //        collected = true;
    //        thisTimer.Start();
    //    }

    //    public void StoreCurrentEntitySpeeds(List<Player> Players, List<Enemy> Enemies)
    //    {
    //        PlayerSpeeds = new double[Players.Count()];
    //        EnemySpeeds = new double[Enemies.Count()];

    //        if (Players.Count() > 0 && Enemies.Count() > 0)
    //        {
    //            for (int i = 0; i < Players.Count; i++)
    //            {
    //                PlayerSpeeds[i] = Players[i].GetSpeed();
    //            }
    //            for (int i = 0; i < Enemies.Count; i++)
    //            {
    //                EnemySpeeds[i] = Enemies[i].GetSpeed();
    //            }
    //        }
    //    }

    //    private void SetEffects()
    //    {
    //        FriendlyEffect = GameConstants.PowerUpFriendlyEffects[type];
    //        EnemyEffect = GameConstants.PowerUpEnemyEffects[type];
    //        ScorePointEffect = GameConstants.PowerUpScoreEffects[type];

    //        if (EnemyEffect == 0)
    //        {
    //            MaxDuration = 3;
    //        }
    //    }

    //    public double[] GetEffects()
    //    {
    //        double[] effects = new double[3];
    //        //effects[0] is applied to players, 1 is applied to enemy, 2 is applied to the value of the score

    //        effects[2] = ScorePointEffect;

    //        if (ReverseEffects)
    //        {
    //            effects[0] = EnemyEffect;
    //            effects[1] = FriendlyEffect;
    //            effects[2] = 1 / effects[2];
    //        }
    //        else
    //        {
    //            effects[0] = FriendlyEffect;
    //            effects[1] = EnemyEffect;
    //        }

    //        return effects;
    //    }

    //    public void SetReversedEffects()
    //    {
    //        ReverseEffects = true;
    //    }

    //    public void RemoveFromMap()
    //    {
    //        Game.MW.GameCanvas.Children.Remove(shape);
    //        PixelPt.X = -1;
    //        PixelPt.Y = -1;
    //    }

    //    public bool IsReversed()
    //    {
    //        return ReverseEffects;
    //    }

    //    public static int DetermineType(List<Powerup> VisiblePowerups)
    //    {
    //        Random rand = new Random();
    //        int randType;
    //        bool ValidType = false;
    //        int[] EachTypeCount = new int[GameConstants.NumOfPowerupTypes];
    //        //counts how many of each type of powerup there are

    //        foreach (var item in VisiblePowerups)
    //        {
    //            EachTypeCount[item.GetTypeOfPowerup()] += 1;
    //            // e.g. if the powerup is type 1, then count[1] is incremented by 1;
    //        }

    //        randType = rand.Next(0, EachTypeCount.Count());

    //        do
    //        {
    //            if (randType > -1)
    //            {
    //                if (EachTypeCount[randType] > 0)
    //                {
    //                    randType -= 1;
    //                    ValidType = false;
    //                }
    //                else
    //                {
    //                    ValidType = true;
    //                }
    //            }

    //        } while (ValidType == false && randType > -1);


    //        return randType;
    //    }

    //    public int GetTypeOfPowerup()
    //    {
    //        return type;
    //    }

    //    public Point GetPixelPt()
    //    {
    //        return PixelPt;
    //    }

    //    public bool IsExpired()
    //    {
    //        //returns a value to indicate if the powerup has been active/in the maze past its maximum time

    //        if (CurrentTime >= MaxDuration)
    //        {
    //            thisTimer.Stop();
    //            return true;
    //        }

    //        return false;
    //    }

    //    public bool IsCollected()
    //    {
    //        return collected;
    //    }

    //    public double[] GetOldPlayerSpeeds()
    //    {
    //        return PlayerSpeeds;
    //    }

    //    public double[] GetOldEnemySpeeds()
    //    {
    //        return EnemySpeeds;
    //    }

    //    public virtual void ApplyEffect() { }
    //}

    class SpeedUpPowerup : Powerup
    {
        private double multiplier = 1.5;

        public SpeedUpPowerup(GameConstants constants, Point MazePt, Point PixelPt) : base(constants, MazePt, PixelPt)
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
        private double multiplier = 0.5;

        public SpeedDownPowerup(GameConstants constants, Point MazePt, Point PixelPt) : base(constants, MazePt, PixelPt)
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
        public FreezePowerup(GameConstants constants, Point MazePt, Point PixelPt) : base(constants, MazePt, PixelPt)
        {
            Random rand = new Random();
            TypeName = "Enemy is frozen for " + Convert.ToString(maxDuration) + " seconds!";
            TypeNumber = 2;
            shape.Fill = GameConstants.PowerUpColours[TypeNumber];
            Effects[1] = -1;
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

        public PointPowerup(GameConstants constants, Point MazePt, Point PixelPt) : base(constants, MazePt, PixelPt)
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
                Effects[2] = 0;
                TypeName = "Points have no value for " + maxDuration + "seconds!";
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
        //effects[2] = the value of the points in the game, as the player collects them

        protected Point MazePt;
        protected Point PixelPt;
        protected Rectangle shape = new Rectangle();
        private bool Visible = true;
        protected bool ReverseEffects = false;
        //used to flip effects if the enemy collects the powerup

        protected string TypeName = "Null Effect";
        protected int TypeNumber = -1;
        protected Random rand = new Random();

        public Powerup(GameConstants constants, Point mazePt, Point pixelPt)
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
                double temporary = Effects[0];
                Effects[0] = Effects[1];
                Effects[1] = temporary;

                maxDuration = (int)Math.Truncate((double)(maxDuration / 2));
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
            //PixelPt.X = -1;
            //PixelPt.Y = -1;
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
        protected int CurrentDirection = 3;     //moving left at start
        private int Score = 0;
        private int PlayerNum;
        private int ScorePointValue = 1;

        private Point PixelPtChange = new Point(0, 0);        /// used during movement
        private Point NextPixelPt;
        private Point NextPoint;
        private Point CentrePixelPt = new Point();

        public Player(Point StartMazePt, Point StartPixelPt, GameConstants Constants, int Number)
        {
            //takes position of enemy in maze, and the corresponding pixelpoint

            SetCurrentCellPt(StartMazePt);
            SetPixelPt(StartPixelPt);

            DefaultMovementSpeed = GameConstants.DefaultMovementSpeed;
            CurrentMovementSpeed = GameConstants.DefaultMovementSpeed;
            PlayerNum = Number;
            //player 1 has number 0, player 2 has number 1, Enemy has number 2

            if (PlayerNum > -1 && PlayerNum < GameConstants.PlayerColours.Count())
            {
                Shape.Fill = GameConstants.PlayerColours[PlayerNum];
            }

            Shape.Width = GameConstants.CellDimensions[0] - Constants.WallThickness;
            Shape.Height = GameConstants.CellDimensions[1] - Constants.WallThickness;

            CentrePixelPt.X = StartPixelPt.X + Shape.Width / 2;
            CentrePixelPt.Y = StartPixelPt.Y + Shape.Width / 2;

            Game.MW.GameCanvas.Children.Add(Shape);
            Draw();
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
            for (int i = 0; i < CurrentMovementSpeed; i++)
            {
                PixelPt.X += PixelPtChange.X;
                PixelPt.Y += PixelPtChange.Y;

                if (PixelPt == NextPixelPt)
                {
                    PixelPtChange.X = 0;
                    PixelPtChange.Y = 0;
                    CurrentMazePt = NextPoint;
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
        }

        public void IncrementScore()
        {
            Score += ScorePointValue;
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
            //CurrentDirection = -1;

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
            CurrentMovementSpeed = Difficulty;
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
            Thickness = CellDimensions[0] / 10;

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
            if (NumOfActivePoints < 1)
            {
                //if there are no visible collectible points, they are all redrawn
                DrawAllScorePoints();
                NumOfActivePoints = MazeDimensions[0] * MazeDimensions[1];
            }

            bool Unvisited = Cells[(int)newPoint.X, (int)newPoint.Y].HideScorePoint();

            if (Unvisited)
            {
                NumOfActivePoints -= 1;
            }

            return Unvisited;
        }

        private void DrawAllScorePoints()
        {
            foreach (var cell in Cells)
            {
                cell.DrawScorePoint();
            }
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
        //protected allows use in derived Horizontal and Vertical Wall classes
        protected int width;
        protected int height;
        protected Rectangle Shape = new Rectangle { Fill = GameConstants.ForegroundColour, };
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

            XCoord = 65 + (i + 1) * cellDimensions[0];
            YCoord = (j + 1) * cellDimensions[1];
            //calculates position of wall based on location in array

            Game.MW.GameCanvas.Children.Add(Shape);
        }

        public Point GetCoordinates()
        {
            return new Point(XCoord, YCoord);
        }

        public void Hide()
        {
            if (hideable)
            {
                // Shape.Opacity = 0;      
                Game.MW.GameCanvas.Children.Remove(Shape);
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

    class DataEntry
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

        public int GetTotalScore()
        {
            int Total = 0;

            foreach (var score in PlayerScores)
            {
                Total += score;
            }

            return Total;
        }

        public int GetScoreFromName(string NameToFind, bool IncludeLowercase, bool IncludeUppercase)
        {
            int Index = SearchPlayers(NameToFind, IncludeLowercase, IncludeUppercase);

            if (Index > -1 && Index < PlayerScores.Count())
            {
                return PlayerScores[Index];
            }

            return 0;
        }

        public int SearchPlayers(string toFind, bool IncludeLowercase, bool IncludeUppercase)
        {
            //searches through all the game saves in the file, checking the players in each

            for (int i = 0; i < PlayerNames.Count(); i++)
            {
                if (IncludeLowercase)
                {
                    if (toFind.ToLower() == PlayerNames[i].ToLower())
                    {
                        return i;
                    }
                }
                else if (IncludeUppercase)
                {
                    if (toFind.ToUpper() == PlayerNames[i].ToUpper())
                    {
                        return i;
                    }
                }
                else if (PlayerNames[i] == toFind)
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
        private const int DisplayRefreshConstant = 16;          /// refresh rate for the entities
        private DispatcherTimer GameTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1), };
        private DispatcherTimer MovementTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, DisplayRefreshConstant), };
        private List<Player> ActivePlayers = new List<Player>();
        private List<Enemy> ActiveEnemies = new List<Enemy>();
        private TextBlock TimeDisplayTXT = new TextBlock();
        private TextBlock ScoreDisplayTXT = new TextBlock();

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
        private double Difficulty = GameConstants.DefaultMovementSpeed;
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

            TimeDisplayTXT.Width = 30;
            MW.GameCanvas.Children.Add(TimeDisplayTXT);
            Canvas.SetLeft(TimeDisplayTXT, 55);
            Canvas.SetTop(TimeDisplayTXT, 15);

            ScoreDisplayTXT.Width = 30;
            MW.GameCanvas.Children.Add(ScoreDisplayTXT);
            ScoreDisplayTXT.Text = Convert.ToString(0);
            Canvas.SetLeft(ScoreDisplayTXT, 55);
            Canvas.SetTop(ScoreDisplayTXT, 45);

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
            //List<Point> AdjacentPoints = new List<Point>();
            //bool invalidPoint = false;
            //int attempts = 0;
            //int maxAttempts = 10;

            //do
            //{
            GeneratedPt.X = rand.Next(0, MazeDimensions[0]);
            GeneratedPt.Y = rand.Next(0, MazeDimensions[1]);
            //    AdjacentPoints = MazeOne.GetAdjacentPoints(GeneratedPt);

            //    attempts += 1;

            //    foreach (var Point in AdjacentPoints)
            //    {
            //        foreach (var Player in ActivePlayers)
            //        {
            //            if (Player.GetCurrentMazePt() == Point)
            //            {
            //                invalidPoint = true;
            //            }
            //        }

            //        foreach (var Enemy in ActiveEnemies)
            //        {
            //            if (Enemy.GetCurrentMazePt() == Point)
            //            {
            //                invalidPoint = true;
            //            }
            //        }

            //        foreach (var Powerup in VisiblePowerups)
            //        {
            //            if (Powerup.GetCurrentMazePt() == Point)
            //            {
            //                invalidPoint = true;
            //            }
            //        }
            //    }

            //    if (attempts > maxAttempts)
            //    {
            //        GeneratedPt.X = 0;
            //        GeneratedPt.Y = 0;
            //        invalidPoint = false;
            //    }

            //} while (invalidPoint);

            return GeneratedPt;
        }

        private void MovementTimer_Tick(object sender, EventArgs e)
        {
            List<int> ExpiredPowerupIndexes = new List<int>();

            foreach (Player player in ActivePlayers)
            {
                CheckTouchingPowerup(player);

                if (!player.IsMoving())
                {
                    if (MazeOne.PointCrossed(player.GetCurrentMazePt()))
                    {
                        //if the player is on a new point, it increments the score by 1
                        player.IncrementScore();
                        TotalScore += player.GetScorePointValue();
                    }
                    UpdatePlayerPosition(player);
                }

                player.Draw();
            }

            foreach (Enemy enemy in ActiveEnemies)
            {
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

                //if (currentSpeed < 0)
                //{
                //    MessageBox.Show("Players unfrozen");
                //}

                ActivePlayers[i].SetSpeed(currentSpeed / effects[0]);
                ActivePlayers[i].SetScoreValue(1);
            }

            for (int i = 0; i < ActiveEnemies.Count; i++)
            {
                currentSpeed = ActiveEnemies[i].GetSpeed();
                ActiveEnemies[i].SetSpeed(currentSpeed / effects[1]);

                //if (currentSpeed < 0)
                //{
                //    MessageBox.Show("Enemy unfrozen");
                //}
            }

            MazeOne.SetScorePointColour(1);

        }

        private void CheckTouches(Player EnemyToCheck)
        {
            Point EnemyPixel = EnemyToCheck.GetPixelPt();
            double dx = 0;
            double dy = 0;
            int toRemove = -1;
            int playerNum;

            for (int i = 0; i < ActivePlayers.Count(); i++)
            {
                //cycles through all players in the maze

                //finds the pixel distance between the two entities
                dx = Math.Abs(EnemyPixel.X - ActivePlayers[i].GetPixelPt().X);
                dy = Math.Abs(EnemyPixel.Y - ActivePlayers[i].GetPixelPt().Y);

                //if (*EnemyToCheck.GetCurrentMazePt() == ActivePlayers[i].GetCurrentMazePt() || 
                if ((dx < GameConstants.CellDimensions[0] / 2 && dy < GameConstants.CellDimensions[1] / 2))
                {
                    toRemove = i;
                    //if the enemy is within half a cell of the player, it marks that player to be removed

                    ActivePlayers[i].RemoveFromMap();
                    //removes the visual aspect of the player

                    playerNum = ActivePlayers[i].GetPlayerNum();

                    RemovedPlayers[playerNum] = ActivePlayers[i];
                    //stores the player in another structure so that its score can be retrieved later

                }
            }

            //has to be removed after it checks through all players
            if (toRemove > -1 && toRemove < ActivePlayers.Count())
            {
                ActivePlayers.RemoveAt(toRemove);
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

            Powerup PowerupToAdd = new Powerup(Constants, MazeLocation, PixelPt);

            if (MazeLocation != new Point(0, 0) && CountOfPowerupType[type] < 1)
            {
                switch (type % 4)
                {
                    case 0:
                        PowerupToAdd = new SpeedUpPowerup(Constants, MazeLocation, PixelPt);
                        break;
                    case 1:
                        PowerupToAdd = new SpeedDownPowerup(Constants, MazeLocation, PixelPt);
                        break;
                    case 2:
                        PowerupToAdd = new FreezePowerup(Constants, MazeLocation, PixelPt);
                        break;
                    case 3:
                        PowerupToAdd = new PointPowerup(Constants, MazeLocation, PixelPt);
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
                }
            }

            foreach (var enemy in ActiveEnemies)
            {
                if (enemy.GetSpeed() <= 0)
                {
                    appliedEffect = false;
                }
            }

            if (appliedEffect == true)
            {
                //if neither entity is frozen
                AppliedPowerUpEffects.Enqueue(thisPowerup, thisPowerup.GetMaxDuration());

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

                GameTimer.Stop();

                MessageBox.Show("Applied effect: " + Convert.ToString(thisPowerup.GetTypeOfPowerup()));

                GameTimer.Start();
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
                FileEntries.Clear();
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
        }

        private DataEntry AddPlayersToEntry(DataEntry thisEntry)
        {
            List<string> ChosenNames = new List<string>();
            string thisName;

            for (int i = 1; i < RemovedPlayers.Count() + 1; i++)
            {
                thisName = NamePlayers.GetName(i, ChosenNames);
                ChosenNames.Add(thisName);
                MessageBox.Show("Saving Player " + i + " as '" + thisName + "'");

                thisEntry.AddPlayer(thisName, RemovedPlayers[i - 1].GetScore());
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
            TimeDisplayTXT.Text = Convert.ToString(currentTime);
            ScoreDisplayTXT.Text = Convert.ToString(TotalScore);
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
            Queue<int> ShortestPath;
            Queue<int> AlternatePath;
            Point target = new Point(0, 0);

            if (ActivePlayers.Count > 0)
            {
                ShortestPath = MazeOne.GeneratePathToTarget(ActivePlayers[0].GetCurrentMazePt(), enemyParam.GetCurrentMazePt());

                for (int i = 1; i < ActivePlayers.Count; i++)
                {
                    AlternatePath = MazeOne.GeneratePathToTarget(ActivePlayers[i].GetCurrentMazePt(), enemyParam.GetCurrentMazePt());

                    if (AlternatePath.Count() < ShortestPath.Count())
                    {
                        ShortestPath = AlternatePath;
                        target = ActivePlayers[i].GetCurrentMazePt();
                    }
                }

                foreach (var powerup in VisiblePowerups)
                {
                    AlternatePath = MazeOne.GeneratePathToTarget(powerup.GetCurrentMazePt(), enemyParam.GetCurrentMazePt());

                    if (AlternatePath.Count() * 4 < ShortestPath.Count())
                    {
                        ShortestPath = AlternatePath;
                        target = powerup.GetCurrentMazePt();
                    }
                }

                enemyParam.UpdatePath(ShortestPath);
                enemyParam.SetTarget(target);
            }
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

            this.Width = GameConstants.indent[0] + (MazeDimensions[0] + 4) * GameConstants.CellDimensions[0];
            this.Height = GameConstants.indent[1] + (MazeDimensions[1] + 4) * GameConstants.CellDimensions[1];

            this.ResizeMode = ResizeMode.NoResize;

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
