using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Mazeman
{
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

        // Tracks the frame number, so the sprite can be updated at different intervals
        private int DisplayNumber = 0;          
        private double UpdateFrequency = 1;

        // Used during movement, to update position of the image
        private Point PixelPtChange = new Point(0, 0);
        private Point NextPixelPt;
        private Point NextPoint;

        // Position to which a player is reset after death
        private Point StartingPoint = new Point();
        private Point StartingPixelPoint = new Point();

        protected Image Sprite = new Image();
        protected BitmapImage[] IMGSources = new BitmapImage[4];
        protected BitmapImage FrozenIMGSource = new BitmapImage();

        public Player(Point StartMazePt, Point StartPixelPt, int Number)
        {
            StartingPoint = StartMazePt;
            StartingPixelPoint = StartPixelPt;

            DefaultMovementSpeed = GameConstants.Difficulties.Last();

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

            // Sprite image size is proportional to the maze cells
            Sprite.Width = GameConstants.CellDimensions[0] * (1 - GameConstants.WallThicknessProportion);
            Sprite.Height = GameConstants.CellDimensions[1] * (1 - GameConstants.WallThicknessProportion);

            Game.CurrentWindow.GameCanvas.Children.Add(Sprite);
            Reset();

            Draw();
        }

        public int GetDirection()
        {
            return CurrentDirection;
        }

        public int GetScore()
        {
            return Score;
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
            return PixelPtChange.X != 0 || PixelPtChange.Y != 0;
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
                // UpdateFrequency represents how frequently the player's position is updated
                // e.g. if the current movement speed is 0.5, then UF = 2, so it is updated every second frame
            }
            else if (CurrentMovementSpeed < 0)
            {
                // If the character is frozen 
                Sprite.Source = FrozenIMGSource;
            }
            else
            {
                UpdateFrequency = 1;
                // Updates player position every frame
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

        // Sets the player's position outside of the maze, so it cannot be seen once caught
        public void RemoveFromMap()
        {
            Canvas.SetLeft(Sprite, -100);
            Canvas.SetTop(Sprite, -100);
        }

        public void Draw()
        {
            // Moves player position
            IncrementPixelPt();

            if (IsMoving() && CurrentMovementSpeed > 0)
            {
                Sprite.Source = IMGSources[Math.Abs(CurrentDirection)];
            }

            // Redraw player in its new position
            Canvas.SetLeft(Sprite, PixelPt.X);
            Canvas.SetTop(Sprite, PixelPt.Y);
        }

    }
}
