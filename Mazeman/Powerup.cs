using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Mazeman
{
    abstract class Powerup
    {
        protected int maxDuration = 10;
        protected int currentActiveTime = 0;

        protected Point MazePt;
        protected Point PixelPt;

        // Indexes of the powerup images, by colour
        protected const int ORANGE_SPRITE_NUM = 0;
        protected const int GREEN_SPRITE_NUM = 1;
        protected const int BLUE_SPRITE_NUM = 2;
        protected const int PURPLE_SPRITE_NUM = 3;

        // Indicates the effects should be flipped (only if the enemy collects it)
        protected bool EffectsAreReversed = false;

        private static Random rand = new Random();

        protected Image Sprite = new Image();
        protected BitmapImage IMGSource = new BitmapImage();

        public Powerup(Point mazePt, Point pixelPt)
        {
            maxDuration = rand.Next(6, 15);
            // Generates a random time for the powerup to appear/be applied for
            //  e.g. if maxDuration = 5, then the powerup will be displayed for 5 seconds, and then when collected, its effect will be applied for 5 seconds

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

        public void InitImage(int imageNum)
        {
            if (imageNum >= ORANGE_SPRITE_NUM && imageNum <= PURPLE_SPRITE_NUM)
            {
                IMGSource.BeginInit();
                IMGSource.UriSource = new Uri(GameConstants.SpriteFolderAddress + "/Powerup-" + imageNum + ".png");
                IMGSource.EndInit();
                Sprite.Source = IMGSource;
            }
        }

        public void Collect(Player collector)
        {
            currentActiveTime = 0;
            RemoveFromMap();

            // If a player collects a powerup, then the effects benefit the player and detriment the enemy.
            // If an enemy collects the powerup, then the effects must benefit the enemy and detriment the player.
            if (collector is Enemy)
            {
                this.ReverseEffect();

                Enemy enemy = (Enemy)collector;

                // Duration of the powerup is proportional to the enemy's speed/difficulty
                maxDuration = (int)Math.Round(enemy.GetSpeed());
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
            return EffectsAreReversed;
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

        public abstract Player ApplyEffectToPlayer(Player player);

        public abstract Enemy ApplyEffectToEnemy(Enemy enemy);

        public abstract Player RemoveEffectFromPlayer(Player thisPlayer);

        public abstract Enemy RemoveEffectFromEnemy(Enemy enemy);


        public virtual void ReverseEffect() {
            EffectsAreReversed = !EffectsAreReversed;
        }

        public abstract string GetMessage();
    }

    abstract class SpeedPowerup : Powerup
    {
        protected float playerSpeedMultiplier = 1.0f;
        protected float enemySpeedMultiplier = 1.0f;

        public SpeedPowerup(Point mazePt, Point pixelPt) : base(mazePt, pixelPt) { }

        public override Player ApplyEffectToPlayer(Player player)
        {
            player.MultiplySpeed(playerSpeedMultiplier);
            return player;
        }

        public override Enemy ApplyEffectToEnemy(Enemy enemy)
        {
            enemy.MultiplySpeed(enemySpeedMultiplier);
            return enemy;
        }

        public override Player RemoveEffectFromPlayer(Player player)
        {
            player.MultiplySpeed(1.0f / playerSpeedMultiplier);
            return player;
        }

        public override Enemy RemoveEffectFromEnemy(Enemy enemy)
        {
            enemy.MultiplySpeed(1.0f / enemySpeedMultiplier);
            return enemy;
        }

        public override void ReverseEffect()
        {
            base.ReverseEffect();
            float temp = playerSpeedMultiplier;
            playerSpeedMultiplier = enemySpeedMultiplier;
            enemySpeedMultiplier = temp;
        }
    }

    class SpeedUpPlayerPowerup : SpeedPowerup
    {

        public SpeedUpPlayerPowerup(Point mazePt, Point pixelPt): base(mazePt, pixelPt)
        {
            playerSpeedMultiplier = 1.5f;
            InitImage(ORANGE_SPRITE_NUM);
        }

        public override string GetMessage()
        {
            return (EffectsAreReversed ? "Enemy" : "Player") + " Speed is increased for " + maxDuration + " seconds!";
        }
    }

    class SpeedDownEnemyPowerup : SpeedPowerup
    {
        public SpeedDownEnemyPowerup(Point mazePt, Point pixelPt) : base(mazePt, pixelPt)
        {
            enemySpeedMultiplier = 0.5f;
            InitImage(GREEN_SPRITE_NUM);
        }

        public override string GetMessage()
        {
            return (EffectsAreReversed ? "Player" : "Enemy") + " Speed is decreased for " + maxDuration + " seconds!";
        }
    }

    class FreezePowerup : Powerup
    {
        public FreezePowerup(Point mazePt, Point pixelPt) : base(mazePt, pixelPt) 
        {
            InitImage(BLUE_SPRITE_NUM);
        }

        public override Player ApplyEffectToPlayer(Player player)
        {
            // If the player is already frozen, don't do anything
            if (EffectsAreReversed)
                player.TryFreeze();

            return player;
        }

        public override Enemy ApplyEffectToEnemy(Enemy enemy)
        {
            // If the enemy is already frozen, don't do anything
            if (!EffectsAreReversed)
                enemy.TryFreeze();

            return enemy;
        }

        public override Player RemoveEffectFromPlayer(Player player)
        {
            // If the player isn't frozen, don't do anything
            if (EffectsAreReversed)
                player.TryUnfreeze();

            return player;
        }

        public override Enemy RemoveEffectFromEnemy(Enemy enemy)
        {
            if (!EffectsAreReversed)
                enemy.TryUnfreeze();

            return enemy;
        }

        public override string GetMessage()
        {
            return (EffectsAreReversed ? "Players" : "Enemies") + " are frozen for " + maxDuration + " seconds!";
        }
    }

    class PointsPowerup : Powerup
    {
        // This should be a positive value; otherwise point addition will be weird
        float PointValueMultiplier = 1.0f;

        public PointsPowerup(Point mazePt, Point pixelPt) : base(mazePt, pixelPt)
        {
            InitImage(PURPLE_SPRITE_NUM);
            PointValueMultiplier = 2.0f;
        }

        public override Player ApplyEffectToPlayer(Player player)
        {
            player.MultiplyScorePointValue(PointValueMultiplier);
            return player;
        }

        public override Enemy ApplyEffectToEnemy(Enemy enemy)
        {
            return enemy;
        }

        public override Player RemoveEffectFromPlayer(Player player)
        {
            player.MultiplyScorePointValue(1.0f / PointValueMultiplier);
            return player;

        }

        public override Enemy RemoveEffectFromEnemy(Enemy enemy)
        {
            return enemy;
        }

        public override string GetMessage()
        {
            return "Points are multiplied by " + PointValueMultiplier + " for " + maxDuration + " seconds!";
        }

        public override void ReverseEffect()
        {
            base.ReverseEffect();
            // Take Reciprocal -> if the normal value is 2, then the reversed multiplier is 0.5
            PointValueMultiplier = 1.0f / PointValueMultiplier;
        }
    }
}
