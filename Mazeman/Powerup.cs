using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Mazeman
{
    class Powerup
    {
        protected int maxDuration = 10;
        protected int currentActiveTime = 0;
        protected double[] Effects = new double[3] { 1, 1, 1 };
        //  [0] = friendly speed multiplier
        //  [1] = enemy speed multiplier
        //  [2] = the multiplier for the points in the game, as the player collects them

        protected Point MazePt;
        protected Point PixelPt;

        // Indicates the effects should be flipped (only if the enemy collects it)
        protected bool ReverseEffects = false;

        // Message displayed to user when the powerup is activated
        protected string DisplayMessage = "Null Effect";

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
            // Generates a random time for the powerup to appear/be applied for
            //  e.g. if maxDuration = 5, then the powerup will be displayed for 5 seconds, and then when collected, its effect will be applied for 5 seconds

            switch (TypeNumber)
            {
                case 0:
                    // Increases speed by 50%
                    Effects[0] = 1.5;
                    DisplayMessage = "Player Speed is increased for " + maxDuration + " seconds!";
                    break;
                case 1:
                    // Decreases enemy speed by 50%
                    Effects[1] = 0.5;
                    DisplayMessage = "Enemy Speed is decreased for " + maxDuration + " seconds!";
                    break;
                case 2:
                    // Freezes enemies
                    Effects[1] = -1;
                    DisplayMessage = "Enemies are frozen for " + maxDuration + " seconds!";
                    break;
                case 3:
                    // Doubles the value of each point collected by the player
                    Effects[2] = 2;    
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
            currentActiveTime = 0;
            RemoveFromMap();

            ReverseEffects = (collector is Enemy);
            // If collected by an Enemy, then the effects need to be reversed, so that the enemy receives the benefit

            if (ReverseEffects)
            {
                // Flips the effects so that the enemy receives the friendly effect, when it collects the powerup
                double temporaryVal = Effects[0];
                Effects[0] = Effects[1];
                Effects[1] = temporaryVal;

                Enemy enemy = (Enemy)collector;

                // Duration of the powerup is proportional to the enemy's speed/difficulty
                maxDuration = (int)Math.Round(enemy.GetSpeed());

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
            // Constrains value between 1 and 25
            distance = Math.Min(25, distance);
            distance = Math.Max(1, distance);

            // Duration between 1 and 5
            maxDuration = (int)Math.Sqrt(distance);
        }

        public double[] GetEffects()
        {
            return Effects;
        }

        public int GetTypeNumber()
        {
            // Used to determine which powerup is being applied/collected/removed e.g. 0 = SpeedUp
            return TypeNumber;
        }

        public string GetMessage()
        {
            return DisplayMessage;
        }

        public void IncrementActiveTime()
        {
            // Stores how long the powerup has been displayed/its effect has been applied
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
            return currentActiveTime > maxDuration;
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
            // Fades powerups in and out when generating/removing from the map

            if (currentActiveTime < 2 && Sprite.Opacity < 0.95)
            {
                // FADE IN
                Sprite.Opacity /= 0.85;
            }
            else if (maxDuration - currentActiveTime <= 1 && Sprite.Opacity > 0.01)
            {
                // FADE OUT
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

            // Only apply effect to the enemy if they are not frozen or the effect is not freezing
            // This prevents two freezes cancelling each other out since -1 * -1 = +1
            if (thisPlayer is Enemy && (speed > 0 || Effects[1] > 0))
            {
                thisPlayer.SetSpeed(speed * Effects[1]);
            }
            else
            {
                // Apply effect to Player's speed
                if (speed > 0 || Effects[0] > 0)
                {
                    thisPlayer.SetSpeed(speed * Effects[0]);
                }

                // Apply effect to points
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

            // Only apply effect to the enemy if they are not frozen or the effect is not freezing
            // This prevents two freezes cancelling each other out since -1 * -1 = +1
            if (thisPlayer is Enemy && !(speed > 0 && Effects[1] < 0))
            {
                thisPlayer.SetSpeed(speed / Effects[1]);
            }
            else
            {
                // Apply effect to Player's speed
                if (!(speed > 0 && Effects[0] < 0))
                {
                    thisPlayer.SetSpeed(speed / Effects[0]);
                }

                // Apply effect to points
                if (!(PointValue > 0 && Effects[2] < 0))
                {
                    thisPlayer.SetScorePointValue((int)(PointValue / Effects[2]));
                }
            }

            return thisPlayer;
        }
    }
}
