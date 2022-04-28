using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Mazeman
{
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
}
