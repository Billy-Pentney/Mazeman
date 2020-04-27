﻿using System;
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

        private List<DataEntry> DisplayedEntries = new List<DataEntry>();
        private int HighestScoreForGraph = 0;

        public VisualGraph(Canvas myCanvas)
        {
            thisCanvas = myCanvas;
            //prevents the first objects from being removed from the canvas
        }

        public void DrawAxes(double currentLeft)
        {
            double fractionFromGraph = 0.01;

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
            int UndisplayedRight = GraphLines.Count - 1 - DisplayRange[1];            // number of bars after the last one on the graph
            int UndisplayedLeft = DisplayRange[0];  // number of bars before the first one on the graph

            if (UndisplayedLeft > 0 && difference < 0)
            {
                GraphLines[DisplayRange[1]].RemoveAllFromCanvas();

                DisplayRange[0] += -1;
                DisplayRange[1] += -1;

                DrawGraph();
            }
            else if (UndisplayedRight > 0 && difference > 0)
            {
                GraphLines[DisplayRange[0]].RemoveAllFromCanvas();

                DisplayRange[0] += 1;
                DisplayRange[1] += 1;

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

        public void CreateGraph(List<DataEntry> ChosenEntries, int GreatestScore)
        {
            ClearCanvas();

            HighestScoreForGraph = GreatestScore;

            BottomLeftIndent[0] = thisCanvas.ActualWidth * 0.05;
            BottomLeftIndent[1] = thisCanvas.ActualHeight * 0.1;

            TopRightIndent[0] = 250;
            TopRightIndent[1] = 100;

            TopRightIndent[0] += BottomLeftIndent[0];
            TopRightIndent[1] += BottomLeftIndent[1];

            AvailableArea[0] = thisCanvas.ActualWidth - (BottomLeftIndent[0] + TopRightIndent[0]);
            AvailableArea[1] = thisCanvas.ActualHeight - (BottomLeftIndent[1] + TopRightIndent[1]);

            double HeightScale = AvailableArea[1] / HighestScoreForGraph;
            int NumOfBars = GetNumOfBars(ChosenEntries);

            double shapeWidth = Math.Round(AvailableArea[0] / (2 * NumOfBars), 1);       
            //1.5 is the multiplier because the graph is half of the width, then a bar

            if (shapeWidth > 150)
            {
                shapeWidth = 150;
            }
            else if (shapeWidth < 5)
            {
                shapeWidth = 5;
            }

            BarLine.SetBarScales(shapeWidth, HeightScale);

            DisplayedEntries = ChosenEntries;

            foreach (var entry in ChosenEntries)
            {
                BarLine thisBar = new BarLine(entry);
                GraphLines.Add(thisBar);
                //converts all entries into bars
            }

            DisplayRange[0] = 0;
            DisplayRange[1] = DetermineDisplayRange(shapeWidth, AvailableArea[0]);
            //determines the range of bars which need to be displayed based on their width e.g. 0-10 of 20 bars

            DrawGraph();

        }

        public int DetermineDisplayRange(double width, double availableWidth)
        {
            double currentLeft = width / 2;
            int DisplayUpTo = 0;

            for (int i = 0; i < GraphLines.Count; i++)
            {
                foreach (var shape in GraphLines[i].GetShapes())
                {
                    currentLeft += width;
                }
                currentLeft += width / 2;

                DisplayUpTo = i;

                if (currentLeft >= availableWidth)
                {
                    i = GraphLines.Count;
                    //early exit of for loop
                }
            }

            return DisplayUpTo;
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
            IncCapsCheckBox.IsChecked = true;
            IncTwoPlayersCheckBox.IsChecked = true;
            DateRadioBtn.IsChecked = true;

            SetColours();
            BarChart = new VisualGraph(myCanvas);

            DeserialiseFile();
            Determine1PLeaderboard();
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
            SearchBtn.Background = bkg;
            SearchBtn.Foreground = frg;
            IncCapsCheckBox.Foreground = frg;
            IncTwoPlayersCheckBox.Foreground = frg;
            GraphInfoBlock.Background = bkg;
            GraphInfoBlock.Foreground = frg;
            SearchInfoBlock.Background = bkg;
            SearchInfoBlock.Foreground = frg;
            DateRadioBtn.Foreground = frg;
            TimeSurvivedRadioBtn.Foreground = frg;
            PScoreRadioBtn.Foreground = frg;
            DiffRadioBtn.Foreground = frg;
            TotalScoreRadioBtn.Foreground = frg;
            LeaderboardBtn.Background = bkg;
            LeaderboardBtn.Foreground = frg;
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            SearchName = InputNameTxtBox.Text;

            if (SearchName.Length > 0)
            {
                IncludeCapitals = (bool)IncCapsCheckBox.IsChecked;
                IncludeTwoPlayers = (bool)IncTwoPlayersCheckBox.IsChecked;

                List<DataEntry> EntriesToDisplay = GetAllResultsWithName(SearchName, IncludeCapitals);

                EntriesToDisplay = SortEntries(EntriesToDisplay, SortByType, SearchName, IncludeCapitals);

                BarChart.ClearCanvas();

                GraphInfoBlock.Inlines.Clear();

                if (EntriesToDisplay.Count > 0)
                {
                    SearchInfoBlock.Inlines.Add(Environment.NewLine + "Number Of Games Displayed: " + Convert.ToString(EntriesToDisplay.Count()));
                    BarChart.CreateGraph(EntriesToDisplay, FindGreatestScore(EntriesToDisplay));
                    GraphInfoBlock.Inlines.Add("Click on a bar to see more information about that game!" + Environment.NewLine + Environment.NewLine);
                    GraphInfoBlock.Inlines.Add("The height of each bar represents the score of the player in that game. ");
                    GraphInfoBlock.Inlines.Add("The colour of each bar represents the difficulty of the game (red = hard, yellow = medium, green = easy)" + Environment.NewLine + Environment.NewLine);

                    if (IncludeTwoPlayers)
                    {
                        GraphInfoBlock.Inlines.Add("Two-player games are shown as two adjacent bars with no gap between them.");
                    }
                }
                else
                {
                    SearchInfoBlock.Inlines.Add("Number Of Games Displayed: " + 0);

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
                MessageBox.Show("Please input a name");
            }
        }

        private List<DataEntry> SortEntries(List<DataEntry> ToSort, int SortType, string SearchName, bool IncludeCapitals)
        {
            List<DataEntry> SortedList = new List<DataEntry>();

            switch (SortType)
            {
                case 0:
                    SortedList = ToSort.OrderBy(DataEntry => DataEntry.GameID).ToList();
                    //Orders by the date when the game was played
                    break;
                case 1:
                    SortedList = ToSort.OrderBy(DataEntry => DataEntry.SurvivedFor).ToList();
                    //Orders by the length of time the last player survived for 
                    //e.g. if Bob was captured at 10 seconds and Bill was captured at 20 seconds, it returns 20
                    break;
                case 2:
                    SortedList = ToSort.OrderBy(DataEntry => DataEntry.GetTotalScore()).ToList();
                    //orders entries by the total score of all players in that game
                    //e.g. if Bob got 12, and Bill got 15, it returns 27
                    break;
                case 3:
                    SortedList = ToSort.OrderBy(DataEntry => DataEntry.GetScoreFromName(SearchName, IncludeCapitals)).ToList();
                    // Orders entries by the score of the player with the searched name
                    //e.g if Bob got 12, and Bill got 15, and you search for Bill, it returns 15; if you search for Bob, it returns 12;
                    break;
                case 4:
                    SortedList = ToSort.OrderBy(DataEntry => DataEntry.difficulty).ToList();
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

            SearchInfoBlock.Inlines.Clear();

            if (AllFileEntries.Count == 0)
            {
                DeserialiseFile();
            }

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

            string Rank = GetRank(NameToSearch, IncludeCapitals);

            if (Rank != "0th")
            {
                SearchInfoBlock.Inlines.Add(Environment.NewLine + "'" + NameToSearch + "' is " + Rank + " on the Leaderboard" + Environment.NewLine);
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

        private void DateRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            SortByType = 0;
        }

        private void TimeSurvivedRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            SortByType = 1;
        }

        private void TotalScoreRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            SortByType = 2;
        }

        private void PScoreRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            SortByType = 3;
        }

        private void DiffRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            SortByType = 4;
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
        }
    }    
}
