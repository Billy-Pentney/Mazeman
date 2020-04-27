using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.IO;

namespace A_Level_Project__New_
{
    /// <summary>
    /// Interaction logic for ViewHistoryWindow.xaml
    /// </summary>

    class BarLine
    {
        private List<Rectangle> shapes = new List<Rectangle>();
        private static double WidthScale = 20;
        private static double HeightScale = 1;
        private string GameStatsText;
        private Brush[] Colours = new Brush[] { Brushes.Green, Brushes.Orange, Brushes.Red };
        private Brush ClickColour = Brushes.LightBlue;           //colour when the user clicks on/selects a bar
        private Brush ShapeColour;  // ordinary colour for the shapes

        public BarLine(DataEntry thisEntry)
        {
            GameStatsText = "GameID: " + thisEntry.GameID + Environment.NewLine + "Timestamp: " + thisEntry.Timestamp + Environment.NewLine;
            GameStatsText += Environment.NewLine + "Total Score For All Players: " + Convert.ToString(thisEntry.GetTotalScore()) + " pts" + Environment.NewLine;

            for (int i = 0; i < thisEntry.PlayerNames.Count; i++)
            {
                Rectangle thisLine = new Rectangle();
                ShapeColour = Colours[thisEntry.difficulty - 1];
                thisLine.Fill = ShapeColour;
                thisLine.Width = WidthScale;
                thisLine.Height = thisEntry.PlayerScores[i] * HeightScale;
                shapes.Add(thisLine);
                GameStatsText += Environment.NewLine + "Player " + Convert.ToString(i + 1) + " (" + thisEntry.PlayerNames[i] + ") scored " + thisEntry.PlayerScores[i] + " pt";

                if (thisEntry.PlayerScores[i] > 1)
                {
                    GameStatsText += "s";
                };
            }

            GameStatsText += Environment.NewLine + Environment.NewLine + "Last Player Survived For: " + thisEntry.SurvivedFor + " seconds" + Environment.NewLine;
            GameStatsText += Environment.NewLine + "Maze Dimensions: " + Convert.ToString(thisEntry.mazeDimensions[0]) + "x" + Convert.ToString(thisEntry.mazeDimensions[1]);
            GameStatsText += Environment.NewLine + "Enemy Difficulty: ";

            switch (thisEntry.difficulty)
            {
                case 1:
                    GameStatsText += "Easy";
                    break;
                case 2:
                    GameStatsText += "Medium";
                    break;
                case 3:
                    GameStatsText += "Hard";
                    break;
                default:
                    GameStatsText += "Modified File";
                    break;
            }

            VisualGraph.thisCanvas.MouseLeftButtonDown += ThisWindow_MouseLeftButtonDown;
        }

        public static void SetBarScales(double w, double h)
        {
            WidthScale = w;
            HeightScale = h;
        }

        public static double GetWidth()
        {
            return WidthScale;
        }

        private void ThisWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (shapes.Contains(e.Source))
            {
                SetColour(ClickColour);
                MessageBox.Show(GameStatsText);
                SetColour(ShapeColour);
            }
            
        }

        private void SetColour(Brush ColourToSet)
        {
            foreach (var item in shapes)
            {
                item.Fill = ColourToSet;
            }
        }

        public List<Rectangle> GetShapes()
        {
            return shapes;
        }

        public void RemoveAllFromCanvas()
        {
            foreach (var shape in shapes)
            {
                VisualGraph.thisCanvas.Children.Remove(shape);
            }
        }

    }

    class VisualGraph
    {
        public static Canvas thisCanvas;
        private List<BarLine> GraphLines = new List<BarLine>();
        private Line[] GraphSides = new Line[2];

        private double[] BottomLeftIndent = new double[2];    //[0] = from left, [1] = from bottom
        private double[] TopRightIndent = new double[2];      //[0] = from right, [1] = from top
        private double[] AvailableArea = new double[2];       //[0] = width of graph, [1] = height of graph

        private int[] DisplayRange = new int[2];

        private List<DataEntry> EntriesToDisplay = new List<DataEntry>();
        private int HighestScoreForGraph = 0;

        private double EndOfGraph;

        public VisualGraph(Canvas myCanvas)
        {
            thisCanvas = myCanvas;
            //prevents the first objects from being removed from the canvas
        }

        public void CreateGraph(List<DataEntry> ChosenEntries, int GreatestScore)
        {
            #region Preparing Area/Indents
            ClearCanvas();

            BottomLeftIndent[0] = thisCanvas.ActualWidth * 0.05;
            BottomLeftIndent[1] = thisCanvas.ActualHeight * 0.1;

            TopRightIndent[0] = 220;
            TopRightIndent[1] = 30;

            TopRightIndent[0] += BottomLeftIndent[0];
            TopRightIndent[1] += BottomLeftIndent[1];

            AvailableArea[0] = thisCanvas.ActualWidth - (BottomLeftIndent[0] + TopRightIndent[0]);
            AvailableArea[1] = thisCanvas.ActualHeight - (BottomLeftIndent[1] + TopRightIndent[1]);

            #endregion

            HighestScoreForGraph = GreatestScore;

            double HeightScale = AvailableArea[1] / HighestScoreForGraph;
            int NumOfBars = GetNumOfBars(ChosenEntries);

            double shapeWidth = Math.Round(AvailableArea[0] / (2 * NumOfBars), 1);
            //1.5 is the multiplier because the graph is half of the width, then a bar

            int minWidth = 7;
            int maxWidth = 200;

            if (shapeWidth > maxWidth)
            {
                shapeWidth = maxWidth;
            }
            else if (shapeWidth < minWidth)
            {
                shapeWidth = minWidth;
            }

            BarLine.SetBarScales(shapeWidth, HeightScale);

            EntriesToDisplay = ChosenEntries;

            foreach (var entry in ChosenEntries)
            {
                BarLine thisBar = new BarLine(entry);
                GraphLines.Add(thisBar);
                //converts all entries into bars
            }

            DisplayRange[0] = 0;
            DisplayRange[1] = DetermineDisplayRange(shapeWidth, 0);
            //determines the range of bars which need to be displayed based on their width e.g. 0-10 of 20 bars

            DrawGraph();

        }

        public void DrawGraph()
        {
            double shapeWidth = BarLine.GetWidth();
            double currentLeft = BottomLeftIndent[0] + shapeWidth / 2;

            for (int i = DisplayRange[0]; i < DisplayRange[1] + 1; i++)
            {
                foreach (var bar in GraphLines[i].GetShapes())
                {
                    if (!thisCanvas.Children.Contains(bar))
                    {
                        thisCanvas.Children.Add(bar);
                    }
                    Canvas.SetLeft(bar, currentLeft);
                    Canvas.SetBottom(bar, BottomLeftIndent[1]);
                    currentLeft += shapeWidth;
                }
                currentLeft += shapeWidth / 2;
            }

            DrawAxes(currentLeft);
        }

        public void DrawAxes(double currentLeft)
        {
            double fractionFromGraph = 0.01;

            EndOfGraph = currentLeft;

            for (int i = 0; i < GraphSides.Length; i++)
            {
                if (!thisCanvas.Children.Contains(GraphSides[i]))
                {
                    GraphSides[i] = new Line() { Stroke = Brushes.DarkGray, StrokeThickness = 2 };
                    thisCanvas.Children.Add(GraphSides[i]);
                    //initialises lines for the first time only
                }
            }
            
            GraphSides[0].X1 = BottomLeftIndent[0] * (1 - fractionFromGraph);     //bottom left point
            GraphSides[0].Y1 = TopRightIndent[1] * (1 + fractionFromGraph) + AvailableArea[1];
            GraphSides[0].X2 = currentLeft;                           //bottom right point
            GraphSides[0].Y2 = TopRightIndent[1] * (1 + fractionFromGraph) + AvailableArea[1];

            GraphSides[1].X1 = BottomLeftIndent[0] * (1 - fractionFromGraph);     //top left point
            GraphSides[1].Y1 = TopRightIndent[1] * (1 - fractionFromGraph);
            GraphSides[1].X2 = BottomLeftIndent[0] * (1 - fractionFromGraph);     //bottom left point
            GraphSides[1].Y2 = TopRightIndent[1] * (1 + fractionFromGraph) + AvailableArea[1];
        }

        public double GetEndOfAxis(char type)
        {
            //gets the pixel position for the far end of that graph axes

            if (type == 'x')
            {
                return GraphSides[0].X2;
            }
            else if (type == 'y')
            {
                return GraphSides[1].Y1;
            }

            return 0;
        }

        public void ClearCanvas()
        {
            //keeps the user input and labels, but removes the graph

            foreach (var item in GraphLines)
            {
                item.RemoveAllFromCanvas();
            }

            GraphLines.Clear();
            thisCanvas.Children.Remove(GraphSides[0]);
            thisCanvas.Children.Remove(GraphSides[1]);
        }

        public void ShiftBars(double difference)
        { 
            int UndisplayedRight = (GraphLines.Count - 1) - DisplayRange[1];            // number of bars after the last one on the graph
            int UndisplayedLeft = DisplayRange[0];  // number of bars before the first one on the graph

            double[] OldDisplayRange = new double[] { DisplayRange[0], DisplayRange[1] };

            if (UndisplayedLeft > 0 && difference < 0)
            {
                //SCROLL LEFT

                //if there are more bars to show on the left and the left button is clicked

                DisplayRange[0] += -1;
                DisplayRange[1] = DetermineDisplayRange(BarLine.GetWidth(), DisplayRange[0]);
                //determines how many bars to show

                for (int i = DisplayRange[1]; i <= OldDisplayRange[1]; i++)
                {
                    GraphLines[i].RemoveAllFromCanvas();
                }
                //removes any bars which should no longer be displayed

                DrawGraph();
            }
            else if (UndisplayedRight > 0 && difference > 0)
            {
                //SCROLL RIGHT

                //if there are more bars to show on the right and the right button is clicked

                GraphLines[DisplayRange[0]].RemoveAllFromCanvas();
                //removes the left-most bar

                DisplayRange[0] += 1;
                DisplayRange[1] = DetermineDisplayRange(BarLine.GetWidth(), DisplayRange[0]);
                //determines how many bars to show

                DrawGraph();
            }
        }

        private int GetNumOfBars(List<DataEntry> EntriesToSearch)
        {
            int count = 0;

            foreach (var item in EntriesToSearch)
            {
                foreach (var score in item.PlayerScores)
                {
                    count += 1;
                }
            }

            return count;
        }

        public int DetermineDisplayRange(double width, int startAt)
        {
            double currentLeft = width / 2;
            int DisplayUpTo = startAt;

            //works out how many bars will fit in the window space
            //returns the upper limit e.g. if it can display bars 1-20, then this returns 20

            for (int i = startAt; i < GraphLines.Count; i++)
            {
                if (currentLeft >= AvailableArea[0])
                {
                    i = GraphLines.Count;
                    //early exit of for loop
                }
                else
                {
                    foreach (var shape in GraphLines[i].GetShapes())
                    {
                        currentLeft += width;
                    }
                    currentLeft += width / 2;

                    DisplayUpTo = i;
                }                
            }

            return DisplayUpTo;
        }

        public int[] GetDisplayRange()
        {
            //the range of entries which are currently being displayed
            return DisplayRange;
        }

        public int GetNumOfEntries()
        {
            return EntriesToDisplay.Count;
        }
    }

    public partial class ViewHistoryWindow : Window
    {
        private VisualGraph BarChart;
        private bool IncludeCapitals;
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

            if (AllFileEntries != null)
            {
                Determine1PLeaderboard();
            }
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
            //DateRadioBtn.Foreground = frg;
            //TimeSurvivedRadioBtn.Foreground = frg;
            //PScoreRadioBtn.Foreground = frg;
            //DiffRadioBtn.Foreground = frg;
            //< RadioButton Name = "DateRadioBtn" Canvas.Right = "50" Width = "180" Canvas.Top = "80" Content = "Sort by date/time played" Checked = "DateRadioBtn_Checked" ></ RadioButton >        
            //< RadioButton Name = "TimeSurvivedRadioBtn" Canvas.Right = "50" Width = "180" Canvas.Top = "110" Content = "Sort by longest time survived" Checked = "TimeSurvivedRadioBtn_Checked" ></ RadioButton >
            //< RadioButton Name = "PScoreRadioBtn" Canvas.Right = "50" Width = "180" Canvas.Top = "140" Content = "Sort by this player's score" Checked = "PScoreRadioBtn_Checked" ></ RadioButton >
            //< RadioButton Name = "DiffRadioBtn" Canvas.Right = "50" Width = "180" Canvas.Top = "170" Content = "Sort by Game Difficulty" Checked = "DiffRadioBtn_Checked" />
            
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
                IncludeCapitals = (bool)IncCapsCheckBox.IsChecked;
                IncludeTwoPlayers = (bool)IncTwoPlayersCheckBox.IsChecked;

                List<DataEntry> EntriesToDisplay = GetAllResultsWithName(SearchName, IncludeCapitals);

                EntriesToDisplay = SortEntries(EntriesToDisplay, SortByType, SearchName, IncludeCapitals);

                BarChart.ClearCanvas();
                GraphInfoBlock.Inlines.Clear();

                if (EntriesToDisplay.Count > 0)
                {
                    //at least one entry is to be displayed

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
                    SortedList = ToSort.OrderBy(DataEntry => DataEntry.difficulty).ToList();
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
            LW.Show();
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
            string range = AdjustedDisplayRange[0] + "-" + AdjustedDisplayRange[1];

            GamesDisplayedTxt.Content = Environment.NewLine + "Displaying " + range + " of " + BarChart.GetNumOfEntries() + " games";
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
