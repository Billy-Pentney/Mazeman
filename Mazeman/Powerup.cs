﻿using System;
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
}