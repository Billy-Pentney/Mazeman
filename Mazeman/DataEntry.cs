using System;
using System.Collections.Generic;
using System.Linq;

namespace Mazeman
{
    public class DataEntry
    {
        /* Public attributes which are read from/written to the JSON file */
        public int GameID { get; set; }
        public List<string> PlayerNames { get; set; } = new List<string>();
        public List<int> PlayerScores { get; set; } = new List<int>();
        public string Timestamp { get; set; }
        public int[] MazeDimensions { get; set; }
        public int Difficulty { get; set; }

        // Length of time the game lasted, in seconds
        public int SurvivedFor { get; set; }

        /* */

        public void AddPlayer(string name, float score)
        {
            PlayerNames.Add(name);
            // Truncate the decimal part of the score
            PlayerScores.Add((int)score);
        }

        public void SetOtherGameVariables(int GameID, int difficulty, int currentTime, int[] mazeDimensions)
        {
            this.GameID = GameID;
            this.Difficulty = difficulty;
            this.MazeDimensions = mazeDimensions;
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

            foreach (int score in PlayerScores)
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

        /* Sums the scores for each player, returning the total */
        public int GetTotalScore()
        {
            int Total = 0;

            foreach (int score in PlayerScores)
            {
                Total += score;
            }

            return Total;
        }

        /* Searches for a given name, returning the index of that name in the scores */
        public int GetScoreFromName(string NameToFind, bool IncludeCapitals)
        {
            int Index = SearchPlayers(NameToFind, IncludeCapitals);

            if (Index > -1 && Index < PlayerScores.Count())
            {
                return PlayerScores[Index];
            }

            return -1;
        }

        /* Searches through the game saves, for the given name.
         * Returns the index of that name, if found; or -1, if not. */
        public int SearchPlayers(string nameToFind, bool IncludeCapitals)
        {
            for (int i = 0; i < PlayerNames.Count(); i++)
            {
                // Check for an exact match, or case-insensitive match
                if (PlayerNames[i] == nameToFind || (IncludeCapitals && nameToFind.ToLower() == PlayerNames[i].ToLower()))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
