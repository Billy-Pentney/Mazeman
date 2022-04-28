using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Mazeman
{
    class Game
    {
        // Allows canvas to be accessed out of the main window
        public static GameWindow CurrentWindow { get; set; }
        private Maze MazeOne;
        private int[] MazeDimensions;
        // Number of milliseconds between each refresh of the entities
        private const int DisplayRefreshConstant = 17;

        // Counts the length of time a game has been running
        private DispatcherTimer GameTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1), };
        // Redraws the window at regular intervals
        private DispatcherTimer MovementTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, DisplayRefreshConstant), };

        private List<Player> ActivePlayers = new List<Player>();
        private List<Enemy> ActiveEnemies = new List<Enemy>();

        // Players are moved to here when they die and run out of lives
        private List<Player> RemovedPlayers = new List<Player>();

        private int[] CountOfPowerupType;

        // Stores the powerups currently in the maze
        private List<Powerup> VisiblePowerups = new List<Powerup>();
        // Stores powerups which have been collected and the effect is being applied with the priority as the powerup duration
        private List<Powerup> AppliedPowerups = new List<Powerup>();

        private int NumOfPlayers = 1;
        private int CurrentTime = 0;
        private int TotalScore = 0;
        private double Difficulty = GameConstants.Difficulties.Last();

        private int PlayerLives = 3;
        private int TimesBoardCleared = 0;

        // Describes how many of each powerup can be active (visible or collected) at one time (e.g. a freeze must expire before another can be generated)
        private int MaxPowerupsPerType = 1;
        // Number of seconds between generation of each powerup
        private int PowerupGenDelay = 3;

        private Random rand = new Random();

        // Images for the player lives
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
            CurrentTime = 0;

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
                CurrentTime += 1;
                // Updates the unpaused time the game has been open

                foreach (Powerup powerup in AppliedPowerups)
                {
                    powerup.IncrementActiveTime();
                }

                if (AppliedPowerups.Count > 0)
                {
                    // Orders powerup effects by how long before they should be removed
                    AppliedPowerups = AppliedPowerups.OrderBy(powerup => powerup.GetRemainingTime()).ToList();
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
                    // Increments each powerups time by 1

                    if (VisiblePowerups[i].IsExpired())
                    {
                        // If they have been visible for more than their maximum duration, 
                        // Then they are removed from the map and from the list of powerups
                        VisiblePowerups[i].RemoveFromMap();
                        CountOfPowerupType[VisiblePowerups[i].GetTypeNumber()] -= 1;
                        VisiblePowerups.RemoveAt(i);
                    }
                    else
                    {
                        i += 1;
                    }
                }

                if (CurrentTime % PowerupGenDelay == 0)
                {
                    // e.g. generates a powerup every three seconds (time is divisible by 3)
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

            // If all the points have been collected, add them all back again
            if (MazeOne.GetNumofScorePoints() < 1)
            {
                ResetMazeScorePoints();
            }

            RemoveExpiredPowerups();
            UpdateScoreAndTime();

            // If all players have been caught
            if (ActivePlayers.Count() < 1)
            {
                // Subtract one life
                PlayerLives -= 1;
                LifeImages[PlayerLives].Opacity = 0.2;

                // If the player(s) have at least one more life, reset their position and continue
                if (PlayerLives >= 1)
                {
                    ResetGame();
                }
                // Otherwise, this is the end of the game
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

                if (!ActiveEnemies[i].IsMoving() && ActiveEnemies[i].GetSpeed() > 0 && CurrentTime > 0)
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
            CurrentWindow.TimeDisplayTXT.Content = Convert.ToString(CurrentTime);
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

            if (MazeOne.IsValidCell(closestPlayerPoint) && (closestPlayerPoint != enemyParam.GetTarget() || CurrentTime < 1))
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
            bool validGen;

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
            double DirectDistance;

            // Averages cell width and height to find how close the entites have to be to touch
            double CellSize = (GameConstants.CellDimensions[0] + GameConstants.CellDimensions[1]) / 2;

            int playerIndex = 0;

            while (playerIndex < ActivePlayers.Count)
            {
                // Calculates the distance between the centre points of this enemy with each player
                DirectDistance = MazeOne.GetApproximateDistance(EnemyPixel, ActivePlayers[playerIndex].GetPixelPt());

                if (DirectDistance < CellSize / 2)
                {
                    // If the enemy is within half a cell of the player, they are "touching"

                    // So, remove the visual aspect of the player and reduce their lives
                    ActivePlayers[playerIndex].RemoveFromMap();

                    // Stores the player in another data structure so that its score can be retrieved later
                    RemovedPlayers.Add(ActivePlayers[playerIndex]);
                    // The players are stored by player number, so they can be retrieved in the correct order
                    ActivePlayers.RemoveAt(playerIndex);
                }
                else
                {
                    playerIndex++;
                }
            }
        }

        private void CheckPowerupTouches(Player EntityToCheck)
        {
            Point currentPixelPt = EntityToCheck.GetPixelPt();

            // Averages the cell width and height (no difference with a square maze)
            double CellSize = (GameConstants.CellDimensions[0] + GameConstants.CellDimensions[1]) / 2;  

            double DirectDistance;

            for (int i = 0; i < VisiblePowerups.Count; i++)
            {
                // Approximates the distance between the centres of the powerup and the player
                DirectDistance = MazeOne.GetApproximateDistance(currentPixelPt, VisiblePowerups[i].GetPixelPt());

                // Only collect the powerup if the player is with half a cell AND the powerup has not completely faded away yet
                if (DirectDistance < CellSize / 2 && VisiblePowerups[i].GetOpacity() > 0.2)
                {
                    ApplyPowerupEffect(VisiblePowerups[i], EntityToCheck);
                }
            }
        }

        private void EndGame()
        {
            string gameEndMessage;
            GameTimer.Stop();
            MovementTimer.Stop();

            gameEndMessage = "You collected " + TotalScore + " point";

            // Only prints "points" if the player(s) scored more than 1 point
            if (TotalScore > 1)
            {
                gameEndMessage += "s";
            }

            gameEndMessage += ", and survived for " + CurrentTime + " seconds";

            MessageBox.Show(gameEndMessage);

            SaveGameToFile();

            new MainWindow().Show();
            CurrentWindow.Close();
        }

        private void SaveGameToFile()
        {
            List<DataEntry> FileEntries = new List<DataEntry>();
            JsonSerializer Serializer = new JsonSerializer();
            DataEntry thisEntry = new DataEntry();
            int ID;

            // Add details about all players in this game (score and names)
            thisEntry = AddPlayersToEntry(thisEntry);

            // Read the existing entries from the file
            using (StreamReader reader = new StreamReader(GameConstants.FileName))
            {
                using (JsonTextReader jreader = new JsonTextReader(reader))
                {
                    FileEntries = (List<DataEntry>)Serializer.Deserialize(jreader, FileEntries.GetType());
                }
            }


            // If the file contains at least one previous game
            if (FileEntries != null)
            {
                // This game's ID is one more than the last game
                ID = FileEntries.Last().GameID + 1;
            }
            else
            {
                // Otherwise, begin the IDs at 1
                ID = 1;
                FileEntries = new List<DataEntry>();
            }

            // Store attributes of this game in the entry
            thisEntry.SetOtherGameVariables(ID, (int)Difficulty, CurrentTime, MazeOne.GetMazeDimensions());

            // Add this game after the previous games
            FileEntries.Add(thisEntry);

            // Write out all games to the file again, overwriting its contents
            using (StreamWriter writer = new StreamWriter(GameConstants.FileName))
            {
                using (JsonWriter jwriter = new JsonTextWriter(writer))
                {
                    Serializer.Serialize(jwriter, FileEntries);
                }
            }

            MessageBox.Show("Game Saved");
        }

        // For each player, get a name from the user and store it, along with the player's score
        // in the given <c>DataEntry</c>.
        // Returns an entry containing the names and scores of all players from this game.
        private DataEntry AddPlayersToEntry(DataEntry thisEntry)
        {
            List<string> ChosenNames = new List<string>();
            string thisName;
            const string ANONYMOUS = "anonymous";
            bool validName = false;
            
            // Sort the removed players by their number (i.e. P1, then P2).
            // This is so that the P1 inputs their name first, no matter which order the players died in.
            RemovedPlayers = RemovedPlayers.OrderBy(player => player.GetPlayerNum()).ToList();

            for (int i = 0; i < RemovedPlayers.Count; i++)
            {
                do
                {
                    // Prompt the user for a name for this player
                    thisName = NamePlayers.GetName(i + 1, ChosenNames);
                    // Assume the name is valid
                    validName = true;

                    // If no name, or some capitalisation of "anonymous"
                    if (thisName.Length < 1 || thisName.ToLower() == ANONYMOUS)
                    {
                        thisName = ANONYMOUS;
                    }
                    else if (ChosenNames.Contains(thisName))
                    {
                        // Do not allow the same name for both players
                        validName = false;
                        MessageBox.Show("Name already used by other player for this game. Please try again with a different name.");
                    }
                    
                    // Repeat until the user inputs a valid name
                } while (!validName);

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

            // Stop active timers
            GameTimer.Stop();
            MovementTimer.Stop();

            // Remove images 
            VisiblePowerups.Clear();
            AppliedPowerups.Clear();
            ActivePlayers.Clear();
            ActiveEnemies.Clear();

            // Remove any visual elements
            CurrentWindow.GameCanvas.Children.Clear();

            // Clear the maze
            MazeOne = null;
        }
    }
}
