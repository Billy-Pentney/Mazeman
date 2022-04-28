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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Mazeman
{
    static class GameConstants
    {
        //this class contains most of the major Game constants/variables that may need to be changed

        public static int[] CellDimensions { get; } = new int[2] { 25, 25 };
        public static double WallThicknessProportion = 0.1;
        //the walls are 25 * 0.1 pixels thick

        public static int[] WinIndent { get; } = new int[2] { 85, 20 };
        public static int[] MazeIndent { get; } = new int[2] { 85, 0 };
        //the pixel values used to indent the maze from the left/top of the window

        public const string FileName = "History.txt";
        //the name/address of the file where scores should be written to/read from

        public static double[] difficulties { get; } = new double[] { 1, 2, 3 };
        //the player movement speed with no powerups

        public static Brush BackgroundColour { get; set; } = Brushes.Black;       ///colour for the main background of the game windows
        public static Brush ForegroundColour { get; set; } = Brushes.White;           ///colour for the text/alternate background colour if switched
        public static Brush[] WallColours { get; set; } = new Brush[] { Brushes.Red, Brushes.Black };

        public static string[] FileNameSuffixes { get; } = new string[] { "-U.png", "-R.png", "-D.png", "-L.png" };
        //added to the end of the Character Sprite Filenames, corresponding to each direction e.g. Suffixes[0] is the filename for the Up image

        public static string SpriteFolderAddress { get; } = Environment.CurrentDirectory + "/Sprites";

        public const int NumOfUniquePowerupTypes = 4;

        public const int NumOfEnemyColours = 5;

        public static void SwapColours()
        {
            Brush temporary = BackgroundColour;

            //swaps the primary colours so that the window appears on a dark background

            BackgroundColour = ForegroundColour;
            ForegroundColour = temporary;

            temporary = WallColours[0];
            WallColours[0] = WallColours[1];
            WallColours[1] = temporary;
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
        protected bool ReverseEffects = false;
        //used to flip effects if the enemy collects the powerup

        protected string DisplayMessage = "Null Effect";
        //message displayed to user, to indicate the powerup in effect
        protected int TypeNumber = -1;
        private static Random rand = new Random();

        protected Image Sprite = new Image();
        protected BitmapImage IMGSource = new BitmapImage();

        public Powerup(Point mazePt, Point pixelPt, int TypeNum)
        {
            if (TypeNum > -1 && TypeNum < 5)
            {
                TypeNumber = TypeNum;
            }
            else
            {
                TypeNumber = 0;
            }

            maxDuration = rand.Next(3, 8);
            //generates a random time for the powerup to appear/be applied for
            //e.g. if maxDuration = 5, then the powerup will be displayed for 5 seconds, and then when collected, its effect will be applied for 5 seconds

            switch (TypeNumber)
            {
                case 0:
                    Effects[0] = 1.5;           ///increases speed by 50%
                    DisplayMessage = "Player Speed is increased for " + maxDuration + " seconds!";
                    break;
                case 1:
                    Effects[1] = 0.5;           ///decreases enemy speed by 50%
                    DisplayMessage = "Enemy Speed is decreased for " + maxDuration + " seconds!";
                    break;
                case 2:
                    Effects[1] = -1;            ///entitites don't move with negative speed so they "freeze"
                    DisplayMessage = "Enemies are frozen for " + maxDuration + " seconds!";
                    break;
                case 3:
                    Effects[2] = 2;             ///each collected point is worth twice as many points in the total score
                    DisplayMessage = "Points are multiplied by " + Effects[2] + " for " + maxDuration + " seconds!";
                    break;
                default:
                    break;
            }

            IMGSource.BeginInit();
            IMGSource.UriSource = new Uri(GameConstants.SpriteFolderAddress + "/Powerup-" + TypeNumber + ".png");
            IMGSource.EndInit();

            Sprite.Source = IMGSource;

            Sprite.Width = GameConstants.CellDimensions[0] / 2;
            Sprite.Height = GameConstants.CellDimensions[1] / 2;

            this.MazePt = mazePt;
            this.PixelPt = pixelPt;
            this.PixelPt.X += Sprite.Width / 2;     //centres sprite in the cell
            this.PixelPt.Y += Sprite.Height / 2;

            Game.CurrentWindow.GameCanvas.Children.Add(Sprite);
            Canvas.SetLeft(Sprite, this.PixelPt.X);
            Canvas.SetTop(Sprite, this.PixelPt.Y);

            Sprite.Opacity = 0.1;               //used to fade powerup in when generated
        }

        public void Collect(Player collector)
        {
            //virtual because each type of powerup overrides this method

            currentActiveTime = 0;
            RemoveFromMap();

            ReverseEffects = (collector is Enemy);
            //if collected by an Enemy, then the effects need to be reversed, so that the enemy receives the benefit

            if (ReverseEffects)
            {
                //flips the effects so that the enemy receives the friendly effect, when it collects the powerup
                double temporaryVal = Effects[0];
                Effects[0] = Effects[1];
                Effects[1] = temporaryVal;

                Enemy enemy = (Enemy)collector;

                maxDuration = (int)Math.Round(enemy.GetSpeed());
                //halves the duration if the enemy collected it

                switch (TypeNumber)
                {
                    case 0:
                        DisplayMessage = "Enemy Speed is increased";
                        break;
                    case 1:
                        DisplayMessage = "Player Speed is decreased";
                        break;
                    case 2:
                        DisplayMessage = "Players are frozen";
                        break;
                    case 3:
                        Effects[2] = -1;
                        DisplayMessage = "Points have no value";
                        break;
                    default:
                        break;
                }

                DisplayMessage += " for " + maxDuration;
                if (maxDuration > 1)
                {
                    DisplayMessage += " seconds!";
                }
                else
                {
                    DisplayMessage += " second";
                }
            }

        }

        public void SetDuration(double distance)
        {
            if (distance < 1)
            {
                distance = 1;
            }
            else if (distance > 25)
            {
                distance = 25;
            }

            maxDuration = (int)Math.Sqrt(distance);
        }

        public double[] GetEffects()
        {
            return Effects;
        }

        public int GetTypeNumber()
        {
            //used to determine which powerup is being applied/collected/removed e.g. 0 = SpeedUp
            return TypeNumber;
        }

        public string GetMessage()
        {
            return DisplayMessage;
        }

        public void IncrementActiveTime()
        {
            //stores how long the powerup has been displayed/its effect has been applied
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
            if (currentActiveTime > maxDuration)
            {
                return true;
            }

            return false;
        }

        public void RemoveFromMap()
        {
            Game.CurrentWindow.GameCanvas.Children.Remove(Sprite);
        }

        public int GetMaxDuration()
        {
            return maxDuration;
        }

        public bool IsReversed()
        {
            return ReverseEffects;
        }

        public void UpdateOpacity()
        {
            //fades powerups in and out when generating/removing from the map

            if (currentActiveTime < 2 && Sprite.Opacity < 0.95)
            {
                //FADE IN
                Sprite.Opacity *= 1.18;
            }
            else if (maxDuration - currentActiveTime <= 1 && Sprite.Opacity > 0.01)
            {
                //FADE OUT
                Sprite.Opacity *= 0.85;
            }

        }

        public double GetOpacity()
        {
            return Sprite.Opacity;
        }

        public int GetRemainingTime()
        {
            return maxDuration - currentActiveTime;
        }

        public Player ApplyEffect(Player thisPlayer)
        {
            double speed = thisPlayer.GetSpeed();
            double PointValue = thisPlayer.GetScorePointValue();

            //limitation to prevent two freezes cancelling each other out since -1 * -1 = +1

            if (thisPlayer is Enemy && (speed > 0 || Effects[1] > 0))
            {
                thisPlayer.SetSpeed(speed * Effects[1]);
            }
            else
            {
                if (speed > 0 || Effects[0] > 0)
                {
                    thisPlayer.SetSpeed(speed * Effects[0]);
                }

                if (PointValue > 0 || Effects[2] > 0)
                {
                    thisPlayer.SetScorePointValue((int)(PointValue * Effects[2]));
                }
            }

            return thisPlayer;

        }

        public Player RemoveEffect(Player thisPlayer)
        {
            double speed = thisPlayer.GetSpeed();
            double PointValue = thisPlayer.GetScorePointValue();

            if (thisPlayer is Enemy && !(speed > 0 && Effects[1] < 0))
            {
                thisPlayer.SetSpeed(speed / Effects[1]);
            }
            else
            {
                if (!(speed > 0 && Effects[0] < 0))
                {
                    thisPlayer.SetSpeed(speed / Effects[0]);
                }

                if (!(PointValue > 0 && Effects[2] < 0))
                {
                    thisPlayer.SetScorePointValue((int)(PointValue / Effects[2]));
                }
            }

            return thisPlayer;
        }
    }

    class Player
    {
        protected double CurrentMovementSpeed;
        protected double DefaultMovementSpeed;
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

        private Point StartingPoint = new Point();      //when the player dies, it is reset to these coordinates
        private Point StartingPixelPoint = new Point();

        protected Image Sprite = new Image();
        protected BitmapImage[] IMGSources = new BitmapImage[4];
        protected BitmapImage FrozenIMGSource = new BitmapImage();

        public Player(Point StartMazePt, Point StartPixelPt, int Number)
        {
            StartingPoint = StartMazePt;
            StartingPixelPoint = StartPixelPt;

            DefaultMovementSpeed = GameConstants.difficulties.Last();

            PlayerNum = Number;
            //player 1 has number 0, player 2 has number 1, Enemy has number 2

            if (PlayerNum > -1 && PlayerNum < 3)
            {
                for (int i = 0; i < IMGSources.Length; i++)
                {
                    IMGSources[i] = new BitmapImage();
                    IMGSources[i].BeginInit();
                    IMGSources[i].UriSource = new Uri(GameConstants.SpriteFolderAddress + "/P" + (PlayerNum + 1) + GameConstants.FileNameSuffixes[i]);
                    IMGSources[i].EndInit();
                }

                //sets colour based on which player is being created 

                FrozenIMGSource.BeginInit();
                FrozenIMGSource.UriSource = new Uri(GameConstants.SpriteFolderAddress + "/P-F.png");
                FrozenIMGSource.EndInit();

                Sprite.Source = IMGSources[1];

            }

            Sprite.Width = GameConstants.CellDimensions[0] * (1 - GameConstants.WallThicknessProportion);
            Sprite.Height = GameConstants.CellDimensions[1] * (1 - GameConstants.WallThicknessProportion);
            //subtracts the wall thickness from the shape dimensions, so that the player fits in a square

            Game.CurrentWindow.GameCanvas.Children.Add(Sprite);
            Reset();

            Draw();
        }

        public virtual void Reset()
        {
            //resets position/speed of player if caught and it still has lives

            CurrentDirection = -1;
            PixelPtChange = new Point(0, 0);
            SetCurrentCellPt(StartingPoint);
            SetPixelPt(StartingPixelPoint);
            SetSpeed(DefaultMovementSpeed);
            SetScorePointValue(1);
            Sprite.Source = IMGSources[1];

            if (PlayerNum >= 0)
            {
                //Players
                Canvas.SetZIndex(Sprite, 0);
            }
            else
            {
                //Enemies
                Canvas.SetZIndex(Sprite, 50);
            }
        }

        public void UpdateDirection(Key thisKey)
        {
            if (PlayerNum == 0 && CurrentMovementSpeed > 0)
            {
                //PLAYER 1 CONTROLS
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
            else if (PlayerNum == 1 && CurrentMovementSpeed > 0)
            {
                //PLAYER 2 CONTROLS
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
        }

        public int GetDirection()
        {
            return CurrentDirection;
        }

        public void IncrementPixelPt()
        {
            int TimestoIncrement = 0;
            //how many times the pixel position is incremented by 1

            if (CurrentMovementSpeed > 0 && PixelPtChange != new Point(0, 0))
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

                    //increments player position in the direction of movement

                    if (PixelPt == NextPixelPt)
                    {
                        //if the entity reaches its target point for that move, its movement is set to 0
                        PixelPtChange.X = 0;
                        PixelPtChange.Y = 0;
                        CurrentMazePt = NextPoint;
                    }
                }
            }
        }

        public void ConvertMoveToChange(Point newPoint, Point newPixelPt)
        {
            NextPoint = newPoint;
            NextPixelPt = newPixelPt;
            PixelPtChange.X += (newPixelPt.X - PixelPt.X) / GameConstants.CellDimensions[0];
            PixelPtChange.Y += (newPixelPt.Y - PixelPt.Y) / GameConstants.CellDimensions[1];
            //determines the movement in terms of x and y for that move based on the pixel positions

            Sprite.Source = IMGSources[Math.Abs(CurrentDirection)];

            if (CurrentMovementSpeed < 0)
            {
                Sprite.Source = FrozenIMGSource;
            }
        }

        public void IncrementDisplayNumber()
        {
            //used to only update position on alternate frames
            DisplayNumber += 1;
        }

        public bool IsMoving()
        {
            if (PixelPtChange.X != 0 || PixelPtChange.Y != 0)
            {
                return true;
            }
            return false;
        }

        public int GetScore()
        {
            //to be used when storing each player's score
            return Score;
        }

        public void IncrementScore(int Increment)
        {
            if (Increment > 0 && ScorePointValue > 0)
            {
                //prevents negative increments that decrease the score
                Score += Increment;
            }
        }

        public void SetScorePointValue(int value)
        {
            //sets the value of each point when collected by the player 
            //this is a multiplier so if it is 2, then all points that are collected increment the score by twice their normal value

            ScorePointValue = value;
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

            if (CurrentMovementSpeed > 0 && CurrentMovementSpeed < 1)
            {
                UpdateFrequency = 1 / CurrentMovementSpeed;
                //UpdateFrequency represents how frequently the player's position is updated
                //e.g. if the current movement speed is 0.5, then UF = 2, so it is updated every second frame
            }
            else if (CurrentMovementSpeed < 0)
            {
                //if the character is frozen 
                Sprite.Source = FrozenIMGSource;
            }
            else
            {
                UpdateFrequency = 1;
                //updates player position every frame
            }

            DisplayNumber = 0;
        }

        public double GetSpeed()
        {
            return CurrentMovementSpeed;
        }

        public int GetPlayerNum()
        {
            //0 = player 1, 1 = player 2...

            return PlayerNum;
        }

        public void SetCurrentCellPt(Point MazePt)
        {
            CurrentMazePt = MazePt;
        }

        public Point GetCurrentMazePt()
        {
            return CurrentMazePt;
        }

        public void SetPixelPt(Point NewPt)
        {
            PixelPt = NewPt;
        }

        public Point GetPixelPt()
        {
            return PixelPt;
        }

        public void RemoveFromMap()
        {
            Canvas.SetLeft(Sprite, -100);
            Canvas.SetTop(Sprite, -100);
        }

        public void Draw()
        {
            IncrementPixelPt();
            //moves player position

            if (IsMoving() && CurrentMovementSpeed > 0)
            {
                Sprite.Source = IMGSources[Math.Abs(CurrentDirection)];
            }

            Canvas.SetLeft(Sprite, PixelPt.X);
            Canvas.SetTop(Sprite, PixelPt.Y);
            //redraws player in new position
        }

    }

    class Enemy : Player
    {
        private Point Target;
        //the position of the object the enemy generated a path to

        private Queue<int> DirectionsToFollow = new Queue<int>();
        //a list of movements for the enemy to follow to reach the target

        private double PlayerAffinity;
        //represents the enemy tendency to follow the player, not go for powerups

        public Enemy(Point StartMazePt, Point StartPixelPt, int PNumber, double Difficulty, int ColourIndex) : base(StartMazePt, StartPixelPt, PNumber)
        {
            //speed of the enemy is directly proportional to the difficulty i.e. 1 on Easy, 2 on Medium, 3 on hard
            //larger values for speed increase how many pixels the enemy moves per frame

            DefaultMovementSpeed = Difficulty;
            SetSpeed(DefaultMovementSpeed);

            if (PNumber == -1)
            {
                PlayerAffinity = 50;
            }
            else
            {
                PlayerAffinity = 1 + (0.1 * (PNumber + 1));
            }

            for (int i = 0; i < IMGSources.Length; i++)
            {
                IMGSources[i] = new BitmapImage();
                IMGSources[i].BeginInit();
                IMGSources[i].UriSource = new Uri(GameConstants.SpriteFolderAddress + "/Ghost" + ColourIndex + GameConstants.FileNameSuffixes[i]);
                IMGSources[i].EndInit();
            }

            FrozenIMGSource.BeginInit();
            FrozenIMGSource.UriSource = new Uri(GameConstants.SpriteFolderAddress + "/Ghost-F.png");
            FrozenIMGSource.EndInit();

            Sprite.Source = IMGSources[3];

            //used to determine whether an enemy prefers to collect powerups or follow the player
            //close to 0 means high preference for players
            //close to 1 means high preference for players
        }

        public double GetPlayerAffinity()
        {
            return PlayerAffinity;
        }

        public override void Reset()
        {
            base.Reset();
            Sprite.Source = IMGSources[3];
        }

        public void UpdatePath(Queue<int> newDirections)
        {
            //adds the list of instructions to the path for the enemy
            if (newDirections.Count > 0)
            {
                DirectionsToFollow = newDirections;
            }
        }

        public void UpdateDirection()
        {
            //gets the next movement by dequeueing the list of moves

            if (DirectionsToFollow.Count > 0)
            {
                CurrentDirection = DirectionsToFollow.Dequeue();
            }
        }

        public Point GetTarget()
        {
            return Target;
        }

        public void SetTarget(Point newTarget)
        {
            Target = newTarget;
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
        private int NumOfMovesSinceSplit = 0;
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

            GetAdjacentDirections(currentCellPt, IsBacktracking);
            //gets all possible moves from the current cell to adjacent ones

            if (ValidMoves.Count > 0)
            {
                VisitedCells.Push(currentCellPt);

                RandNumOfWallsToRemove = rand.Next(1, ValidMoves.Count());

                if (RandNumOfWallsToRemove > 1)
                {
                    NumOfMovesSinceSplit = 0;
                }
                else if (NumOfMovesSinceSplit > 2 && RandNumOfWallsToRemove < ValidMoves.Count())
                {
                    //if it has been 5 moves since the path split, then an additional wall is taken this time
                    RandNumOfWallsToRemove += 1;
                    NumOfMovesSinceSplit = 0;
                }
                else if (RandNumOfWallsToRemove < ValidMoves.Count())
                {
                    NumOfMovesSinceSplit += RandNumOfWallsToRemove;
                }

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

        public int[] GetMazeDimensions()
        {
            return MazeDimensions;
        }

        public Point GetPixelPoint(Point location)
        {
            return Cells[(int)location.X, (int)location.Y].GetPixelPt();
        }

        public Point GetLastCellInMaze()
        {
            return new Point(MazeDimensions[0] - 1, MazeDimensions[1] - 1);
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

        #region Path-finding (A*)

        public Queue<int> GeneratePathToTarget(Point Target, Point Current, List<Point> OccupiedPositions)
        {
            ///A* SEARCH ALGORITHM 
            ///Takes two points (T, C) in the maze and returns a queue of movements to move from C to T in the shortest valid path
            //NOTE: occupiedpositions is a list of points the Path should avoid if possible

            SimplePriorityQueue<Point, double> SearchCells = new SimplePriorityQueue<Point, double>();
            //search cells is the list of cells to be checked by the algorithm

            Dictionary<Point, double> MovesToReach = new Dictionary<Point, double>();
            //for each key, this is the number of moves to reach that point along the current best path

            Dictionary<Point, Point> PreviousCell = new Dictionary<Point, Point>();
            //each point directs back to the previous point in the path (i.e. where it came from on the path)
            //e.g. if the Enemy has to move from 1,1 to 0,1 and then from 0,1 to 0,0
            //Previous[0,1] = [1,1]
            //Previous[0,0] = [0,1]

            SearchCells.Enqueue(Current, 0);
            MovesToReach.Add(Current, 0);
            PreviousCell.Add(Current, new Point(-1, -1));
            //initialises start point of the search

            List<Point> AdjacentCells = new List<Point>();
            List<Point> NewPath = new List<Point>();

            int CostOfBlockage = 5;
            //the additional cost of moving through another occupied position

            if (OccupiedPositions.Contains(Current))
            {
                OccupiedPositions.Remove(Current);
            }

            double newCost = 0;
            double thisPriority = 0;

            bool pathFound = false;

            while (SearchCells.Count() != 0 && pathFound == false)
            {
                Current = SearchCells.Dequeue();
                //takes out next cell to be checked (based on priority)

                if (Current == Target)
                {
                    //exits loop if path reaches the target cell early
                    pathFound = true;
                }
                else
                {
                    AdjacentCells = GetAdjacentPoints(Current);
                    //gets all valid neighbouring cells to the current cell

                    foreach (var NextCell in AdjacentCells)
                    {
                        if (OccupiedPositions.Contains(NextCell))
                        {
                            newCost = MovesToReach[Current] + CostOfBlockage;
                            //if the path would include a blocked cell (i.e. one which is occupied by another enemy), then the cost is much higher
                        }
                        else
                        {
                            newCost = MovesToReach[Current] + 1;
                            //adds one to the total number of moves to reach that cell from the start (each move increases the path length by 1)
                        }

                        if (!MovesToReach.ContainsKey(NextCell) || newCost < MovesToReach[NextCell])
                        {
                            //if this path now includes a cell that hadn't been in explored before 
                            //or it is shorter than the previous best path to that cell,

                            MovesToReach[NextCell] = newCost;
                            thisPriority = newCost + GetApproximateDistance(NextCell, Target);
                            //the priority associated with moving to that cell is based on the approximate distance from it to the target

                            SearchCells.Enqueue(NextCell, thisPriority);
                            PreviousCell[NextCell] = Current;
                            //points from the new cell to the last one so that a path can be followed
                        }
                    }
                }
            }

            //at the end of the loop, PreviousCell points from each cell to the one before it on the path
            //now we need to remove unnecessary points (those that don't lead to the goal)
            //and reverse the list, before converting it to a set of instructions

            Point newkey = PreviousCell[Target];
            NewPath.Add(Target);

            while (newkey != new Point(-1, -1))
            {
                //at the start we said that the value for the start cell was [-1,-1]
                //if the key is [-1,-1], we must be at the start of the path

                NewPath.Add(newkey);
                newkey = PreviousCell[newkey];

                //creates list of points leading from the target point to the start point
            }

            NewPath.Reverse();
            //reverses the list so that the path is from the start to the target

            Queue<int> DirectionsToFollow = new Queue<int>();

            for (int i = 0; i < NewPath.Count - 1; i++)
            {
                DirectionsToFollow.Enqueue(ConvertMovementToDirection(NewPath[i], NewPath[i + 1]));
                //converts the list of points to instructions for the enemy to follow, and enqueues it in the return collection
            }

            return DirectionsToFollow;
        }

        public double GetApproximateDistance(Point current, Point target)
        {
            //finds manhattan/Pythagorean/direct distance from current point to target

            double dx = target.X - current.X;
            double dy = target.Y - current.Y;

            double distSquared = Math.Pow(dx, 2) + Math.Pow(dy, 2);
            return Math.Sqrt(distSquared);
        }

        #endregion

        #region Secondary Maze Generation Functions

        private void TurnOffWall(Point thisCell, int direction)
        {
            //hides the wall in the specified direction
            //used by Recursive Backtracker to hide the walls as it generates maze

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

        #endregion

        #region Gameplay Functions

        public int[] PointCrossed(Point newPoint)
        {
            int[] NumOfVisiblePoints = new int[2];

            //the number of points in the maze before the point is collected
            NumOfVisiblePoints[0] = NumOfActivePoints;

            if (Cells[(int)newPoint.X, (int)newPoint.Y].HideScorePoint())
            {
                //represents whether the cell has been collected in this movement
                //(false if the scorepoint was already hidden from a player crossing it previously)

                NumOfActivePoints -= 1;
            }

            //the number of points in the maze after the point was collected
            NumOfVisiblePoints[1] = NumOfActivePoints;

            return NumOfVisiblePoints;
        }

        public void ClearMaze()
        {
            foreach (var cell in Cells)
            {
                cell.ClearScorePoint();
            }

            foreach (var wall in AllWallsH)
            {
                wall.Clear();
            }

            foreach (var wall in AllWallsV)
            {
                wall.Clear();
            }
        }

        public void SetScorePointColour(double effectVal)
        {
            int ColourIndex = (int)Math.Truncate(effectVal);

            if (ColourIndex >= 0 && ColourIndex < 3)
            {
                //sets colour of the scorepoints based on the effect of the powerup
                //

                ScorePt.CurrentSourceIndex = ColourIndex;
            }

            foreach (var cell in Cells)
            {
                if (cell.IsPointVisible())
                {
                    cell.DrawScorePoint();
                }
            }
        }

        #endregion

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
            //returns the next cell in that direction if possible

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

        public int ConvertMovementToDirection(Point start, Point target)
        {
            //converts the movement between two points into the four directions of motion

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

        public int GetNumofScorePoints()
        {
            return NumOfActivePoints;
        }

    }

    class Cell
    {
        private Point ShapePt;  ///point in top-left of cell
        private Dictionary<int, Edge> Edges = new Dictionary<int, Edge>();
        private ScorePt ScorePoint;

        public Cell(Point CanvasLoc, int[] cellDimensions)
        {
            ShapePt = CanvasLoc;
            ScorePoint = new ScorePt(ShapePt, cellDimensions);
        }

        public void ClearScorePoint()
        {
            ScorePoint.Clear();
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

            Game.CurrentWindow.GameCanvas.Children.Add(Shape);
        }

        public Point GetPixelPt()
        {
            return PixelPt;
        }

        public void Clear()
        {
            if (Game.CurrentWindow.GameCanvas.Children.Contains(Shape))
            {
                Game.CurrentWindow.GameCanvas.Children.Remove(Shape);
            }
        }

        public void Hide()
        {
            if (hideable)
            {
                Game.CurrentWindow.GameCanvas.Children.Remove(Shape);
            }
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
        private Image Sprite = new Image();
        private BitmapImage[] IMGSources = new BitmapImage[3];

        private Point PixelPt = new Point();
        private bool visible = true;
        public static int CurrentSourceIndex = 1;

        public ScorePt(Point PointParam, int[] cellDimensions)
        {
            for (int i = 0; i < IMGSources.Length; i++)
            {
                IMGSources[i] = new BitmapImage();
                IMGSources[i].BeginInit();
                IMGSources[i].UriSource = new Uri(GameConstants.SpriteFolderAddress + "/SP-" + i + ".png");
                IMGSources[i].EndInit();
            }

            Sprite.Source = IMGSources[CurrentSourceIndex];

            Sprite.Width = cellDimensions[0] / 3;
            Sprite.Height = cellDimensions[1] / 3;

            Game.CurrentWindow.GameCanvas.Children.Add(Sprite);
            Canvas.SetZIndex(Sprite, -50);

            PixelPt = new Point(PointParam.X + 0.5 * (cellDimensions[0] - Sprite.Width), PointParam.Y + 0.5 * (cellDimensions[1] - Sprite.Height));
            Draw();
        }

        public bool GetVisible()
        {
            return visible;
        }

        public void Clear()
        {
            Game.CurrentWindow.GameCanvas.Children.Remove(Sprite);
        }

        public void Hide()
        {
            Sprite.Opacity = 0;
            visible = false;
        }

        public void Draw()
        {
            Sprite.Source = IMGSources[CurrentSourceIndex];

            if (!visible)
            {
                Sprite.Opacity = 255;
                visible = true;
            }

            Canvas.SetLeft(Sprite, PixelPt.X);
            Canvas.SetTop(Sprite, PixelPt.Y);
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
        public static GameWindow CurrentWindow { get; set; }                 ////allows access to canvas outside of main window
        private Maze MazeOne;
        private int[] MazeDimensions;
        private const int DisplayRefreshConstant = 17;          ///number of milliseconds between each refresh of the entities

        private DispatcherTimer GameTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1), };
        private DispatcherTimer MovementTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, DisplayRefreshConstant), };

        private List<Player> ActivePlayers = new List<Player>();
        private List<Enemy> ActiveEnemies = new List<Enemy>();

        private List<Player> RemovedPlayers = new List<Player>();

        private int[] CountOfPowerupType;
        private List<Powerup> VisiblePowerups = new List<Powerup>();
        //stores powerups that can be seen in the maze
        private List<Powerup> AppliedPowerups = new List<Powerup>();
        //stores powerups which have been collected and the effect is being applied with the priority as the powerup duration

        private int NumOfPlayers = 1;
        private int currentTime = 0;
        private int TotalScore = 0;
        private double Difficulty = GameConstants.difficulties.Last();
        //this is set just in case it is not passed by the Settings Window

        private int PlayerLives = 3;
        private int TimesBoardCleared = 0;

        private int MaxPowerupsPerType = 1;
        ///how many of each powerup can be active (visible or collected) at one time (e.g. a freeze must expire before another can be generated)
        private int PowerupGenDelay = 3;
        ///number of seconds between generation of each powerup

        private Random rand = new Random();

        private Image[] LifeImages = new Image[3];
        private BitmapImage LifeIMGSource = new BitmapImage();

        private int NextEnemyColourIndex = 0;
        private double RemainingPauseTime = -1;

        public Game(GameWindow thisWindow, int[] MazeDimensions, bool TwoPlayers, double EnemyDifficulty, int EnemyColourIndex, int NumOfEnemies)
        {
            CurrentWindow = thisWindow;
            CurrentWindow.GameCanvas.Background = GameConstants.BackgroundColour;

            CountOfPowerupType = new int[GameConstants.NumOfUniquePowerupTypes];

            for (int i = 0; i < CountOfPowerupType.Length; i++)
            {
                CountOfPowerupType[i] = 0;
            }

            this.MazeDimensions = MazeDimensions;

            int MazeArea = this.MazeDimensions[0] * this.MazeDimensions[1];

            if (MazeArea < 301)
            {
                MaxPowerupsPerType = 1;
            }
            else if (MazeArea < 601)
            {
                MaxPowerupsPerType = 2;
            }
            else
            {
                MaxPowerupsPerType = 3;
            }

            if (TwoPlayers)
            {
                NumOfPlayers = 2;
            }

            NextEnemyColourIndex = EnemyColourIndex;
            Difficulty = EnemyDifficulty;
            CreateMaze(MazeDimensions, NumOfEnemies);

            GameTimer.Tick += GameTimer_Tick;
            MovementTimer.Tick += MovementTimer_Tick;

            //setting colours for window
            thisWindow.TimeDisplayTXT.Foreground = GameConstants.ForegroundColour;
            thisWindow.ScoreDisplayTXT.Foreground = GameConstants.ForegroundColour;
            thisWindow.PowerupInfoBlock.Foreground = GameConstants.ForegroundColour;
            thisWindow.PowerupInfoBlock.Background = GameConstants.BackgroundColour;
            thisWindow.PowerupInfoBlock.Foreground = GameConstants.ForegroundColour;
            thisWindow.MenuBtn.Background = GameConstants.BackgroundColour;
            thisWindow.MenuBtn.Foreground = GameConstants.ForegroundColour;

            MazeOne.SetScorePointColour(1);

            //setting up life images so that users can see the number of lives they have left
            LifeIMGSource.BeginInit();
            LifeIMGSource.UriSource = new Uri(GameConstants.SpriteFolderAddress + "/Life.png");
            LifeIMGSource.EndInit();

            for (int i = 0; i < LifeImages.Length; i++)
            {
                //creates and displays the hearts to represent lives
                LifeImages[i] = new Image();
                LifeImages[i].Source = LifeIMGSource;
                CurrentWindow.GameCanvas.Children.Add(LifeImages[i]);

                LifeImages[i].Width = 22;
                LifeImages[i].Height = 22;

                Canvas.SetLeft(LifeImages[i], 17 + i * 25);
                Canvas.SetTop(LifeImages[i], 95);
            }
        }

        public void CreateMaze(int[] MazeDim, int NumOfEnemies)
        {
            currentTime = 0;

            MazeOne = new Maze(MazeDim);

            AddEntities(NumOfEnemies);
            GameTimer.Start();
            MovementTimer.Start();
        }

        private void AddEntities(int NumOfEnemies)
        {
            Point StartPoint = new Point(0, 0);
            Point StartPointPixelPt = MazeOne.GetPixelPoint(StartPoint);

            ActivePlayers.Add(new Player(StartPoint, StartPointPixelPt, 0));

            if (NumOfPlayers == 2)
            {
                StartPoint = new Point(0, MazeDimensions[1] - 1);
                StartPointPixelPt = MazeOne.GetPixelPoint(StartPoint);
                ActivePlayers.Add(new Player(StartPoint, StartPointPixelPt, 1));
            }

            StartPoint = new Point(MazeDimensions[0] - 1, 0);

            for (int i = 1; i < NumOfEnemies + 1; i++)
            {
                StartPoint.Y += MazeDimensions[1] / (NumOfEnemies + 1);
                StartPoint.Y = StartPoint.Y % MazeDimensions[1];

                StartPointPixelPt = MazeOne.GetPixelPoint(StartPoint);
                ActiveEnemies.Add(new Enemy(StartPoint, StartPointPixelPt, -i, Difficulty, NextEnemyColourIndex));

                NextEnemyColourIndex = (NextEnemyColourIndex) % GameConstants.NumOfEnemyColours + 1;

                FindShortestPath(ActiveEnemies[i - 1]);
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (RemainingPauseTime < 0)
            {
                int randVal = rand.Next(5, 21);
                currentTime += 1;
                //updates the unpaused time the game has been open

                foreach (var powerup in AppliedPowerups)
                {
                    powerup.IncrementActiveTime();
                }

                if (AppliedPowerups.Count > 0)
                {
                    AppliedPowerups = AppliedPowerups.OrderBy(powerup => powerup.GetRemainingTime()).ToList();
                    //orders powerup effects by how long before they should be removed
                    Powerup NextEffectToExpire = AppliedPowerups.First();

                    if (NextEffectToExpire.IsExpired())
                    {
                        RemovePowerupEffect(NextEffectToExpire);
                        AppliedPowerups.Remove(NextEffectToExpire);
                    }
                }

                for (int i = 0; i < VisiblePowerups.Count;)
                {
                    VisiblePowerups[i].IncrementActiveTime();
                    //increments each powerups time by 1

                    if (VisiblePowerups[i].IsExpired())
                    {
                        //if they have been visible for more than their maximum duration, 
                        //then they are removed from the map and from the list of powerups
                        VisiblePowerups[i].RemoveFromMap();
                        CountOfPowerupType[VisiblePowerups[i].GetTypeNumber()] -= 1;
                        VisiblePowerups.RemoveAt(i);
                    }
                    else
                    {
                        i += 1;
                    }
                }

                if (currentTime % PowerupGenDelay == 0)
                {
                    //e.g. generates a powerup every three seconds (time is divisible by 3)
                    GenerateRandomPowerUp();
                }
            }
            else if (RemainingPauseTime == 0)
            {
                Unpause();

            }
            else
            {
                RemainingPauseTime -= 1;
            }

        }

        private void MovementTimer_Tick(object sender, EventArgs e)
        {
            UpdateEntityMovement();

            if (MazeOne.GetNumofScorePoints() < 1)
            {
                //resets maze if the board is cleared of collectible points
                ResetMazeScorePoints();
            }

            RemoveExpiredPowerups();
            UpdateScoreAndTime();

            //if no players left in the map

            if (ActivePlayers.Count() < 1)
            {
                PlayerLives -= 1;
                LifeImages[PlayerLives].Opacity = 0.2;

                if (PlayerLives > 0)
                {
                    ResetGame();
                }
                else
                {
                    EndGame();
                }
            }
        }

        private void ResetMazeScorePoints()
        {
            //called when there are no scorePoints left to collect
            //resets the board and the entities

            int ClearBoardPoints = GameConstants.CellDimensions[0] + GameConstants.CellDimensions[1];
            int ScorePointValue = 0;

            foreach (var player in ActivePlayers)
            {
                ScorePointValue = player.GetScorePointValue();
                if (ScorePointValue > 0)
                {
                    player.IncrementScore(ClearBoardPoints * ScorePointValue);
                    ///adds a bonus based on the size of the board and the number of players.
                    ///e.g. in 15x15, with 1P, 30pts is added; in 30x20 with 2P, 50 pts is added per player
                    TotalScore += ClearBoardPoints * ScorePointValue;
                }
            }

            Pause(1);
            TimesBoardCleared++;

            MazeOne.ClearMaze();
            MazeOne = new Maze(MazeDimensions);

            if (PlayerLives < 3)
            {
                //adds another life for clearing the board
                LifeImages[PlayerLives].Opacity = 1;
                PlayerLives += 1;
            }

            ResetGame();
        }

        private void UpdateEntityMovement()
        {
            //updates the position/direction/points for all players/enemies

            for (int i = 0; i < ActivePlayers.Count; i++)
            {
                ActivePlayers[i].IncrementDisplayNumber();
                CheckPowerupTouches(ActivePlayers[i]);

                if (!ActivePlayers[i].IsMoving() && ActivePlayers[i].GetSpeed() > 0)
                {
                    CollectPoint(ActivePlayers[i]);
                    UpdatePlayerPosition(ActivePlayers[i]);
                }

                ActivePlayers[i].Draw();
            }

            for (int i = 0; i < ActiveEnemies.Count; i++)
            {
                ActiveEnemies[i].IncrementDisplayNumber();

                CheckPlayerTouches(ActiveEnemies[i]);
                CheckPowerupTouches(ActiveEnemies[i]);

                if (!ActiveEnemies[i].IsMoving() && ActiveEnemies[i].GetSpeed() > 0 && currentTime > 0)
                {
                    ActiveEnemies[i].UpdateDirection();
                    UpdatePlayerPosition(ActiveEnemies[i]);
                    FindShortestPath(ActiveEnemies[i]);
                }

                ActiveEnemies[i].Draw();
            }
        }

        private void RemoveExpiredPowerups()
        {
            List<int> ExpiredPowerupIndexes = new List<int>();

            for (int i = 0; i < VisiblePowerups.Count; i++)
            {
                if (VisiblePowerups[i].IsExpired())
                {
                    //removes expired displayed powerups (those which have not been collected in the time limit)

                    VisiblePowerups[i].RemoveFromMap();
                    CountOfPowerupType[VisiblePowerups[i].GetTypeNumber()] -= 1;
                    ExpiredPowerupIndexes.Add(i);
                }
                else
                {
                    VisiblePowerups[i].UpdateOpacity();
                }
            }

            foreach (int i in ExpiredPowerupIndexes)
            {
                if (i > -1 && i < VisiblePowerups.Count())
                {
                    VisiblePowerups.RemoveAt(i);
                }
            }
        }

        private void UpdateScoreAndTime()
        {
            CurrentWindow.TimeDisplayTXT.Content = Convert.ToString(currentTime);
            CurrentWindow.ScoreDisplayTXT.Content = Convert.ToString(TotalScore);
        }

        public void UpdatePlayerDirection(Key thisKey)
        {
            foreach (var Player in ActivePlayers)
            {
                Player.UpdateDirection(thisKey);
            }
        }

        private void CollectPoint(Player thisPlayer)
        {
            //controls incrementing the score when a player moves over a point

            int ScorePointValue = thisPlayer.GetScorePointValue();
            int[] NumOfVisiblePoints = MazeOne.PointCrossed(thisPlayer.GetCurrentMazePt());
            //0 is the points before this move, 1 is the points after

            int IncrementScore = NumOfVisiblePoints[0] - NumOfVisiblePoints[1];

            if (ScorePointValue > 0)
            {
                thisPlayer.IncrementScore(IncrementScore * ScorePointValue);
                TotalScore += IncrementScore * ScorePointValue;
            }

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

                    if (Entity is Enemy && !CheckEnemyOccupying(newPoint) || !(Entity is Enemy))
                    {
                        //only moves if the entity is a player OR the next point is not obstructed by an enemy
                        Entity.SetCurrentCellPt(newPoint);

                        newPixelPoint = MazeOne.GetPixelPoint(newPoint);
                        Entity.ConvertMoveToChange(newPoint, newPixelPoint);
                    }
                    //gets the new position in the window
                }
            }
        }

        private List<Point> GetAllEnemyPositions()
        {
            List<Point> EnemyPositions = new List<Point>();

            foreach (var enemy in ActiveEnemies)
            {
                EnemyPositions.Add(enemy.GetCurrentMazePt());
            }

            return EnemyPositions;
        }

        private bool CheckEnemyOccupying(Point toCheck)
        {
            //determines if the specific location is shared by an enemy or not
            List<Point> occupied = GetAllEnemyPositions();

            return occupied.Contains(toCheck);
        }

        private void FindShortestPath(Enemy enemyParam)
        {
            Queue<int> ShortestPath = new Queue<int>();
            Queue<int> AlternatePath = new Queue<int>();

            double directDistance = 0;
            double shortestDistance = 0;

            Point closestPowerupPoint = new Point(-1, -1);
            Point closestPlayerPoint = new Point(-1, -1);
            Point currentPoint = enemyParam.GetCurrentMazePt();

            foreach (var player in ActivePlayers)
            {
                directDistance = MazeOne.GetApproximateDistance(currentPoint, player.GetCurrentMazePt());

                if (directDistance < shortestDistance || shortestDistance == 0)
                {
                    shortestDistance = directDistance;
                    closestPlayerPoint = player.GetCurrentMazePt();
                }

                //finds closest player to current location
            }

            shortestDistance = 0;

            foreach (var powerup in VisiblePowerups)
            {
                directDistance = MazeOne.GetApproximateDistance(currentPoint, powerup.GetCurrentMazePt());

                if (directDistance < shortestDistance || shortestDistance == 0)
                {
                    shortestDistance = directDistance;
                    closestPowerupPoint = powerup.GetCurrentMazePt();
                }

                //finds the closest powerup to the current location
            }

            if (MazeOne.IsValidCell(closestPlayerPoint) && (closestPlayerPoint != enemyParam.GetTarget() || currentTime < 1))
            {
                //only generates a path if the player is not in the same cell as the previous target

                ShortestPath = MazeOne.GeneratePathToTarget(closestPlayerPoint, currentPoint, GetAllEnemyPositions());
                //targets the closest player by default

                enemyParam.UpdatePath(ShortestPath);
                enemyParam.SetTarget(closestPlayerPoint);

            }

            if (MazeOne.IsValidCell(closestPowerupPoint) && closestPowerupPoint != enemyParam.GetTarget())
            {
                //only generates a path if the powerup is not in the same cell as the previous target

                AlternatePath = MazeOne.GeneratePathToTarget(closestPowerupPoint, currentPoint, GetAllEnemyPositions());

                //if the path to the nearest powerup is sufficiently small, then it is preferred to the player

                if ((AlternatePath.Count * 4 * enemyParam.GetPlayerAffinity()) < ShortestPath.Count)
                {
                    enemyParam.UpdatePath(AlternatePath);
                    enemyParam.SetTarget(closestPowerupPoint);
                }
            }
        }

        private void GenerateRandomPowerUp()
        {
            Point MazeLocation = GenerateRandomUnoccupiedLocation();        //gets a random cell that is a certain distance from all entites/powerups
            Point PixelPt = new Point();

            int type = rand.Next(0, GameConstants.NumOfUniquePowerupTypes);

            if (CountOfPowerupType[type] < MaxPowerupsPerType && MazeOne.IsValidCell(MazeLocation))
            {
                PixelPt = MazeOne.GetPixelPoint(MazeLocation);
                VisiblePowerups.Add(new Powerup(MazeLocation, PixelPt, type));
                CountOfPowerupType[type] += 1;
            }
        }

        private Point GenerateRandomUnoccupiedLocation()
        {
            //generates a random location to place the powerup

            Random rand = new Random();
            Point GeneratedPt = new Point(-1, -1);
            List<Point> OccupiedPoints = new List<Point>();
            int TimesTried = 0;
            bool validGen = true;

            do
            {
                validGen = true;
                GeneratedPt.X = rand.Next(0, MazeDimensions[0]);
                GeneratedPt.Y = rand.Next(0, MazeDimensions[1]);

                foreach (var player in ActivePlayers)
                {
                    OccupiedPoints.Add(player.GetCurrentMazePt());
                }

                foreach (var enemy in ActiveEnemies)
                {
                    OccupiedPoints.Add(enemy.GetCurrentMazePt());
                }

                foreach (var powerup in VisiblePowerups)
                {
                    OccupiedPoints.Add(powerup.GetCurrentMazePt());
                }

                foreach (var point in OccupiedPoints)
                {
                    if (MazeOne.GetApproximateDistance(GeneratedPt, point) < 0.1 * (MazeDimensions[0] + MazeDimensions[1]))
                    {
                        validGen = false;
                        //ensures each powerup is significant distances from the entities/other powerups
                    }
                }

                ///breaks if tried more than 2 times
            } while (TimesTried < 2 && validGen == false);

            return GeneratedPt;
        }

        private void ResetGame()
        {
            //resets game after all players are caught, and at least one still has lives

            Pause(1);

            if (RemovedPlayers.Count > 0)
            {
                for (int i = 0; i < RemovedPlayers.Count; i++)
                {
                    ActivePlayers.Add(RemovedPlayers[i]);
                    //retrieves players from their temporary location in RemovedPlayers
                }

                RemovedPlayers.Clear();
                ActivePlayers = ActivePlayers.OrderBy(player => player.GetPlayerNum()).ToList();
                //orders by player number to ensure that Player 1 is [0]...
            }

            for (int i = 0; i < AppliedPowerups.Count; i++)
            {
                Powerup thisPowerup = AppliedPowerups[i];
                RemovePowerupEffect(thisPowerup);
            }

            AppliedPowerups.Clear();
            //removes all active powerup effects on reset
            //this must be done before the players are restored, so that the effects are successfully removed

            foreach (var powerup in VisiblePowerups)
            {
                powerup.RemoveFromMap();
            }
            //removes all visible powerups so that the game is clear again

            VisiblePowerups.Clear();

            MazeOne.SetScorePointColour(1);

            for (int i = 0; i < CountOfPowerupType.Length; i++)
            {
                CountOfPowerupType[i] = 0;
            }
            //resets the counters so that new powerups can be generated 

            foreach (var player in ActivePlayers)
            {
                player.Reset();
                player.Draw();
            }

            foreach (var enemy in ActiveEnemies)
            {
                enemy.Reset();
                enemy.Draw();
            }
        }

        private void ApplyPowerupEffect(Powerup thisPowerup, Player CollectedBy)
        {   
            thisPowerup.Collect(CollectedBy);
            VisiblePowerups.Remove(thisPowerup);

            double[] Effects = thisPowerup.GetEffects();

            if (CollectedBy is Enemy)
            {
                //if the enemy collects a powerup, its duration is determined by the distance to the player(s)
                double dist = 999;

                foreach (var enemy in ActiveEnemies)
                {
                    foreach (var player in ActivePlayers)
                    {
                        Point enemypos = enemy.GetCurrentMazePt();
                        Point playerpos = player.GetCurrentMazePt();

                        double newDist = MazeOne.GetApproximateDistance(playerpos, enemypos);

                        if (newDist < dist)
                        {
                            //finds closest player to any enemy
                            dist = newDist;
                        }
                    }
                }

                if (dist < 999)
                {
                    //sets the duration of the powerup based on the distance to the nearest player to any enemy
                    thisPowerup.SetDuration(dist);
                }
            }

            for (int i = 0; i < ActivePlayers.Count; i++)
            {
                ActivePlayers[i] = thisPowerup.ApplyEffect(ActivePlayers[i]);
            }

            for (int i = 0; i < ActiveEnemies.Count; i++)
            {
                ActiveEnemies[i] = (Enemy)thisPowerup.ApplyEffect(ActiveEnemies[i]);
            }

            AppliedPowerups.Add(thisPowerup);

            int ScorePointValue = ActivePlayers[0].GetScorePointValue();

            if (ScorePointValue < 0)
            {
                MazeOne.SetScorePointColour(0);
            }
            else
            {
                MazeOne.SetScorePointColour(ScorePointValue);
            }

            CurrentWindow.PowerupInfoBlock.Text += thisPowerup.GetMessage() + Environment.NewLine;
        }

        private void RemovePowerupEffect(Powerup thisPowerup)
        {
            double[] Effects = thisPowerup.GetEffects();

            CountOfPowerupType[thisPowerup.GetTypeNumber()] -= 1;

            for (int i = 0; i < ActivePlayers.Count; i++)
            {
                ActivePlayers[i] = thisPowerup.RemoveEffect(ActivePlayers[i]);
            }

            for (int i = 0; i < ActiveEnemies.Count; i++)
            {
                ActiveEnemies[i] = (Enemy)thisPowerup.RemoveEffect(ActiveEnemies[i]);
            }

            int ScorePointValue = ActivePlayers[0].GetScorePointValue();

            if (ScorePointValue < 0)
            {
                MazeOne.SetScorePointColour(0);
            }
            else
            {
                MazeOne.SetScorePointColour(ScorePointValue);
            }

            RemoveFromPowerupTextBlock(thisPowerup.GetMessage());
            //removes the effect text from the side panel
        }

        private void RemoveFromPowerupTextBlock(string ToRemove)
        {
            //removes the powerup text from the information bar at the left
            //this stops the effect being displayed after it has been stopped

            string Contents = CurrentWindow.PowerupInfoBlock.Text;

            int index = Contents.IndexOf(ToRemove);

            if (index >= 0 && index < Contents.Length - 1)
            {
                Contents = Contents.Remove(index, ToRemove.Length + Environment.NewLine.Length);

                CurrentWindow.PowerupInfoBlock.Text = Contents;
            }
        }

        private void CheckPlayerTouches(Player EnemyToCheck)
        {
            Point EnemyPixel = EnemyToCheck.GetPixelPt();
            double DirectDistance = 0;
            double CellSize = (GameConstants.CellDimensions[0] + GameConstants.CellDimensions[1]) / 2;
            //averages cell width and height to find how close the entites have to be to touch

            for (int i = 0; i < ActivePlayers.Count();)
            {
                //cycles through all players in the maze
                DirectDistance = MazeOne.GetApproximateDistance(EnemyPixel, ActivePlayers[i].GetPixelPt());

                if (DirectDistance < CellSize / 2)
                {
                    //if the enemy is within half a cell of the player, they are "touching"

                    ActivePlayers[i].RemoveFromMap();
                    //removes the visual aspect of the player and decreases the number of lives they have

                    RemovedPlayers.Add(ActivePlayers[i]);
                    ActivePlayers.RemoveAt(i);
                    //stores the player in another data structure so that its score can be retrieved later
                    //the players are stored by player number, so they can be retrieved in the correct order
                }
                else
                {
                    i++;
                }
            }
        }

        private void CheckPowerupTouches(Player EntityToCheck)
        {
            Point currentPixelPt = EntityToCheck.GetPixelPt();

            double CellSize = (GameConstants.CellDimensions[0] + GameConstants.CellDimensions[1]) / 2;  
            //averages the cell width and height
            double DirectDistance;

            for (int i = 0; i < VisiblePowerups.Count; i++)
            {
                //pixel distances between the entity and the powerup
                DirectDistance = MazeOne.GetApproximateDistance(currentPixelPt, VisiblePowerups[i].GetPixelPt());

                if (DirectDistance < CellSize / 2 && VisiblePowerups[i].GetOpacity() > 0.2)
                {
                    ApplyPowerupEffect(VisiblePowerups[i], EntityToCheck);
                }
            }
        }

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
            //new MessageWindow(message).ShowDialog();

            AddEntryToFile();

            new MainWindow().Show();
            CurrentWindow.Close();
        }

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

            RemovedPlayers = RemovedPlayers.OrderBy(player => player.GetPlayerNum()).ToList();

            for (int i = 0; i < RemovedPlayers.Count; i++)
            {
                do
                {
                    thisName = NamePlayers.GetName(i + 1, ChosenNames);

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
                MessageBox.Show("Saving Player " + (i + 1) + " as '" + thisName + "'");

                thisEntry.AddPlayer(thisName, RemovedPlayers[i].GetScore());
            }

            return thisEntry;
        }

        private void Pause(double timeRemaining)
        {
            MovementTimer.Stop();
            RemainingPauseTime = timeRemaining;
        }

        private void Unpause()
        {
            MovementTimer.Start();
            RemainingPauseTime = -1;
        }

        public void PauseByUser()
        {
            GameTimer.Stop();
            MovementTimer.Stop();
            MessageBox.Show("Game Paused. Press Space/Enter to continue");
            GameTimer.Start();
            MovementTimer.Start();
        }

        public void Destroy()
        {
            //Removes all dependencies of the game object so that it can be replaced with a new game

            GameTimer.Stop();
            MovementTimer.Stop();
            VisiblePowerups.Clear();
            AppliedPowerups.Clear();
            ActivePlayers.Clear();
            ActiveEnemies.Clear();
            CurrentWindow.GameCanvas.Children.Clear();
            MazeOne = null;
        }
    }

    public partial class GameWindow : Window
    {
        private Game newGame;

        public GameWindow(int[] MazeDimensions, bool TwoPlayers, double EnemyDifficulty, int EnemyColourIndex, bool DisableEnemies)
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            int NumOfEnemies = 0;
            int MazeArea = MazeDimensions[0] * MazeDimensions[1];

            if (MazeArea < 200)
            {
                NumOfEnemies = 1;
            }
            else if (MazeArea < 400)
            {
                NumOfEnemies = 2;
            }
            else if (MazeArea < 600)
            {
                NumOfEnemies = 3;
            }
            else if (MazeArea < 800)
            {
                NumOfEnemies = 4;
            }
            else
            {
                NumOfEnemies = 5;
            }

            if (DisableEnemies)
            {
                NumOfEnemies = 0;
            }

            BitmapImage WindowIconSource = new BitmapImage();
            WindowIconSource.BeginInit();
            WindowIconSource.UriSource = new Uri(GameConstants.SpriteFolderAddress + "/P1" + GameConstants.FileNameSuffixes[2]);
            WindowIconSource.EndInit();

            this.Icon = WindowIconSource;

            this.Width = GameConstants.WinIndent[0] + (MazeDimensions[0] + 3) * GameConstants.CellDimensions[0];
            this.Height = GameConstants.WinIndent[1] + (MazeDimensions[1] + 3) * GameConstants.CellDimensions[1];
            //sets dimensions of the window based on the size of the maze

            if (this.Height < 400)
            {
                this.Height = 400;
            }

            this.ResizeMode = ResizeMode.NoResize;

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

            PowerupInfoBlock.Height = this.Height - (3 * GameConstants.CellDimensions[1] + Canvas.GetTop(PowerupInfoBlock));

            AddPowerupShapesToKey();

            newGame = new Game(this, MazeDimensions, TwoPlayers, EnemyDifficulty, EnemyColourIndex, NumOfEnemies);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            newGame.UpdatePlayerDirection(e.Key);

            if (e.Key == Key.Space)
            {
                newGame.PauseByUser();
            }
        }

        private void AddPowerupShapesToKey()
        {
            Image[] PowerupSprites = new Image[4];
            BitmapImage[] PowerupSpriteSources = new BitmapImage[4];

            //used for displaying powerups on left of game

            for (int i = 0; i < PowerupSpriteSources.Length; i++)
            {
                PowerupSpriteSources[i] = new BitmapImage();
                PowerupSpriteSources[i].BeginInit();
                PowerupSpriteSources[i].UriSource = new Uri(GameConstants.SpriteFolderAddress + "/Powerup-" + i + ".png");
                PowerupSpriteSources[i].EndInit();

                double SpriteSize = GameConstants.CellDimensions[0] / 2;

                PowerupSprites[i] = new Image() { Width = SpriteSize, Height = SpriteSize };
                PowerupSprites[i].Source = PowerupSpriteSources[i];

                GameCanvas.Children.Add(PowerupSprites[i]);
                Canvas.SetLeft(PowerupSprites[i], 15);
                Canvas.SetTop(PowerupSprites[i], 147 + i * 20);
            }
        }

        private void MenuBtn_Click(object sender, RoutedEventArgs e)
        {
            //newGame.PauseByUser();
            newGame.Destroy();
            SettingsWindow SW = new SettingsWindow();
            Application.Current.Windows[0].Close();
            SW.Show();
        }
    }
}
