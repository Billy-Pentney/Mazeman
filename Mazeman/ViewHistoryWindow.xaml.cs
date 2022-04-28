using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json;

namespace Mazeman
{

    /// <summary>
    /// 
    /// This window displays the history of the game scores for a given username.
    /// It reads from the History.txt file and then generates a VisualGraph object
    /// to contain the details about each game.
    /// 
    /// </summary>
   
    public partial class ViewHistoryWindow : Window
    {
        private VisualGraph BarChart;
        // indicates if the search is case-sensitive
        private bool IncludeCapitals;
        // indicates if the search should display two-player games
        private bool IncludeTwoPlayers;

        private string SearchName;
        private int SortByType = 0;

        private List<DataEntry> AllFileEntries = new List<DataEntry>();
        //stores all the entries taken from the file

        private List<DataEntry> SortedSinglePlayerEntries = new List<DataEntry>();
        //stores the single-player games sorted by score

        public ViewHistoryWindow()
        {
            InitializeComponent();

            this.Width = SystemParameters.MaximizedPrimaryScreenWidth - 10;
            this.Height = SystemParameters.MaximizedPrimaryScreenHeight - 20;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //fills screen and centres window

            List<string> SortTypes = new List<string>();
            SortTypes.Add("Date/Time Played");
            SortTypes.Add("Time Survived");
            SortTypes.Add("This Player's Score");
            SortTypes.Add("Game Difficulty");
            SortByBox.ItemsSource = SortTypes;

            IncCapsCheckBox.IsChecked = true;
            IncTwoPlayersCheckBox.IsChecked = true;
            SortByBox.SelectedIndex = SortByType;

            SetColours();

            BarChart = new VisualGraph(myCanvas);
            DeserialiseFile();
            //reads file in and stores contents in AllFileEntries

            if (AllFileEntries != null)
            {
                Determine1PLeaderboard();
            }

            ScrollRBtn.Visibility = Visibility.Hidden;
            ScrollLBtn.Visibility = Visibility.Hidden;
            //buttons hidden by default because there is no graph to scroll
        }

        private void SetColours()
        {
            Brush bkg = GameConstants.BackgroundColour;
            Brush frg = GameConstants.ForegroundColour;

            myCanvas.Background = bkg;
            InputLbl.Background = bkg;
            InputLbl.Foreground = frg;
            InputNameTxtBox.Background = bkg;
            InputNameTxtBox.Foreground = frg;

            SortByLbl.Foreground = frg;

            SearchBtn.Background = bkg;
            SearchBtn.Foreground = frg;
            IncCapsCheckBox.Foreground = frg;
            IncTwoPlayersCheckBox.Foreground = frg;
            GraphInfoBlock.Background = bkg;
            GraphInfoBlock.Foreground = frg;

            GamesDisplayedTxt.Foreground = frg;

            LeaderboardBtn.Background = bkg;
            LeaderboardBtn.Foreground = frg;

            SortByBox.Background = bkg;

            ScrollLBtn.Foreground = frg;
            ScrollLBtn.Background = bkg;
            ScrollRBtn.Foreground = frg;
            ScrollRBtn.Background = bkg;

            xAxisLabel.Foreground = frg;
            yAxisLabel.Foreground = frg;
        }

        private void InitiateSearchForName(string SearchName)
        {
            if (AllFileEntries.Count > 0)
            {
                // Determine search filters by the state of the checkboxes
                IncludeCapitals = (bool)IncCapsCheckBox.IsChecked;
                IncludeTwoPlayers = (bool)IncTwoPlayersCheckBox.IsChecked;

                // Filter the entries by the given name
                List<DataEntry> EntriesToDisplay = GetAllResultsWithName(SearchName, IncludeCapitals);
                EntriesToDisplay = SortEntries(EntriesToDisplay, SortByType, SearchName, IncludeCapitals);

                // Clear the canvas so that any previous graph is deleted
                BarChart.ClearCanvas();
                GraphInfoBlock.Inlines.Clear();

                // If at least one entry is to be displayed
                if (EntriesToDisplay.Count > 0)
                {
                    BarChart.CreateGraph(EntriesToDisplay, FindGreatestScore(EntriesToDisplay));

                    int[] DisplayRange = BarChart.GetDisplayRange();
                    int[] AdjustedDisplayRange = new int[2] { DisplayRange[0] + 1, DisplayRange[1] + 1 };
                    string range = AdjustedDisplayRange[0] + "-" + AdjustedDisplayRange[1];

                    GamesDisplayedTxt.Content = Environment.NewLine + "Displaying " + range + " of " + Convert.ToString(EntriesToDisplay.Count()) + " games";
                    GraphInfoBlock.Inlines.Add("Click on a bar to see more information about that game!" + Environment.NewLine + Environment.NewLine);
                    GraphInfoBlock.Inlines.Add("The height of each bar represents the score of the player in that game. ");
                    GraphInfoBlock.Inlines.Add("The colour of each bar represents the difficulty of the game (red = hard, yellow = medium, green = easy)" + Environment.NewLine);

                    if (IncludeTwoPlayers)
                    {
                        GraphInfoBlock.Inlines.Add(Environment.NewLine + "Two-player games are shown as two adjacent bars with no gap between them." + Environment.NewLine);
                    }

                    Canvas.SetLeft(xAxisLabel, BarChart.GetEndOfAxis('x') - xAxisLabel.Width);
                    Canvas.SetTop(yAxisLabel, BarChart.GetEndOfAxis('y') + yAxisLabel.Width);
                    yAxisLabel.Content = "Total Score";

                    ScrollRBtn.Visibility = Visibility.Visible;
                    ScrollLBtn.Visibility = Visibility.Visible;

                    string rank = GetRank(SearchName, IncludeCapitals);

                    if (rank != "0th")
                    {
                        GraphInfoBlock.Inlines.Add(Environment.NewLine + "'" + SearchName + "' is " + rank + " on the Single-Player Leaderboard");
                    }
                }
                else
                {
                    //name not found but other names are in the file

                    GamesDisplayedTxt.Content = "Displaying 0 games";
                    xAxisLabel.Content = "";
                    yAxisLabel.Content = "";

                    string MessageToShow = "'" + SearchName + "'";

                    if (IncludeCapitals)
                    {
                        MessageToShow += ", and no capitalised variations of '" + SearchName + "' found in file history";
                    }
                    else
                    {
                        MessageToShow += " not found in file history";
                    }

                    ScrollRBtn.Visibility = Visibility.Hidden;
                    ScrollLBtn.Visibility = Visibility.Hidden;

                    MessageBox.Show(MessageToShow);
                }
            }
            else
            {
                //no names found in file - corrupt or empty file
                MessageBox.Show("Error: No Games found in file. Please try playing a game first!");
            }
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            SearchName = InputNameTxtBox.Text;
            SortByType = SortByBox.SelectedIndex;

            if (CheckValidName(SearchName))
            {
                InitiateSearchForName(SearchName);
            }
            else
            {
                MessageBox.Show("Please enter a name before searching");
            }
        }

        private bool CheckValidName(string Name)
        {
            if (Name.Length > 0)
            {
                return true;
            }
            return false;
        }

        private List<DataEntry> SortEntries(List<DataEntry> ToSort, int SortType, string SearchName, bool IncludeCapitals)
        {
            List<DataEntry> SortedList = new List<DataEntry>();

            switch (SortType)
            {
                case 0:
                    SortedList = ToSort.OrderBy(DataEntry => DataEntry.GameID).ToList();
                    //Orders by the date when the game was played
                    xAxisLabel.Content = "Date Played";
                    break;
                case 1:
                    SortedList = ToSort.OrderBy(DataEntry => DataEntry.SurvivedFor).ToList();
                    //Orders by the length of time the last player survived for 
                    //e.g. if Bob was captured at 10 seconds and Bill was captured at 20 seconds, it returns 20
                    xAxisLabel.Content = "Time Survived";
                    break;
                case 2:
                    SortedList = ToSort.OrderBy(DataEntry => DataEntry.GetScoreFromName(SearchName, IncludeCapitals)).ToList();
                    // Orders entries by the score of the player with the searched name
                    //e.g if Bob got 12, and Bill got 15, and you search for Bill, it returns 15; if you search for Bob, it returns 12;
                    xAxisLabel.Content = SearchName + "'s score";
                    break;
                case 3:
                    SortedList = ToSort.OrderBy(DataEntry => DataEntry.Difficulty).ToList();
                    //Orders entries by the game difficulty 
                    //i.e. all easy games first, then all medium games, then all hard games
                    xAxisLabel.Content = "Difficulty";
                    break;
                default:
                    SortedList = ToSort;
                    throw new Exception("Invalid Sort Type");
            }

            return SortedList;
        }

        private void DeserialiseFile()
        {
            JsonSerializer JS = new JsonSerializer();

            using (StreamReader reader = new StreamReader(GameConstants.FileName))
            {
                using (JsonTextReader jreader = new JsonTextReader(reader))
                {
                    AllFileEntries = (List<DataEntry>)JS.Deserialize(jreader, AllFileEntries.GetType());
                    //gets all entries from the file and stores in a list of data entries
                }
            }

            if (AllFileEntries == null)
            {
                AllFileEntries = new List<DataEntry>();
            }
        }

        private void Determine1PLeaderboard()
        {
            List<DataEntry> SortedByScore = AllFileEntries.OrderBy(DataEntry => DataEntry.GetHighestScore()).ToList();

            SortedByScore.Reverse();
            //highest to lowest

            foreach (var item in SortedByScore)
            {
                if (item.GetNumberOfPlayers() == 1)
                {
                    SortedSinglePlayerEntries.Add(item);
                }
            }

            //isolates single-player games in order of decreasing score (i.e. highest first)
        }

        private List<DataEntry> GetAllResultsWithName(string NameToSearch, bool IncludeCapitals)
        {
            List<DataEntry> ChosenEntries = new List<DataEntry>();

            foreach (var entry in AllFileEntries)
            {
                if (entry.SearchPlayers(NameToSearch, IncludeCapitals) != -1)
                {
                    if (entry.GetNumberOfPlayers() < 2 || IncludeTwoPlayers)
                    {
                        ChosenEntries.Add(entry);
                        //isolates the data entries which contain the name we are searching for
                    }
                }
            }

            return ChosenEntries;

        }

        private string GetRank(string name, bool IncludeCapitals)
        {
            int position = 0;
            bool nameFound = false;

            do
            {
                if (SortedSinglePlayerEntries[position].GetNameofHighestScore() == name || IncludeCapitals && SortedSinglePlayerEntries[position].GetNameofHighestScore().ToLower() == name.ToLower())
                {
                    nameFound = true;
                }

                position += 1;
                //if found, this makes it an ordinal number (e.g. 0 goes to 1st, 1 goes to 2nd...)
                //if not found, this increments position to try the next entry

            } while (nameFound != true && position < SortedSinglePlayerEntries.Count - 1);

            //index is 0...count, but a position is given from 1...count + 1

            string Rank = Convert.ToString(position);

            if (nameFound)
            {
                if (!Rank.EndsWith("11") && Rank.Last() == '1')
                {
                    Rank += "st";
                }
                else if (!Rank.EndsWith("12") && Rank.Last() == '2')
                {
                    Rank += "nd";
                }
                else if (!Rank.EndsWith("13") && Rank.Last() == '3')
                {
                    Rank += "rd";
                }
                else
                {
                    Rank += "th";
                }
            }
            else
            {
                Rank = "unranked";
            }

            return Rank;
            
        }

        private int FindGreatestScore(List<DataEntry> EntriesToSearch)
        {
            int currentGreatest = 0;
            int nextScore;

            foreach (var item in EntriesToSearch)
            {
                nextScore = item.GetHighestScore();

                if (nextScore > currentGreatest)
                {
                    currentGreatest = nextScore;
                }
            }

            return currentGreatest;
        }

        private void LeaderboardBtn_Click(object sender, RoutedEventArgs e)
        {
            LeaderboardWindow LW = new LeaderboardWindow(SortedSinglePlayerEntries);
            LW.ShowDialog();
        }

        private void ScrollBtn_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source == ScrollLBtn)
            {
                BarChart.ShiftBars(-1);
            }
            else if (e.Source == ScrollRBtn)
            {
                BarChart.ShiftBars(1);
            }

            int[] DisplayRange = BarChart.GetDisplayRange();
            int[] AdjustedDisplayRange = new int[2] { DisplayRange[0] + 1, DisplayRange[1] + 1 };
            string Message = Environment.NewLine + "Displaying " + AdjustedDisplayRange[0] + "-" + AdjustedDisplayRange[1] + " of " + BarChart.GetNumOfEntries() + " games";

            GamesDisplayedTxt.Content = Message;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Enter && CheckValidName(InputNameTxtBox.Text))
            {
                InitiateSearchForName(InputNameTxtBox.Text);
            }
        }
    }    
}
