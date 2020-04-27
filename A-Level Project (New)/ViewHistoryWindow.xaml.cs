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
        public static double Width = 20;
        private Label IDLabel = new Label();
        private string GameStatsText;
        private static Brush[] Colours = new Brush[] { Brushes.Blue, Brushes.LightBlue };

        public BarLine(DataEntry thisEntry, double heightScale)
        {
            GameStatsText = "GameID: " + thisEntry.GameID + Environment.NewLine + "Timestamp: " + thisEntry.Timestamp + Environment.NewLine;
            GameStatsText += Environment.NewLine + "Total Score For All Players: " + Convert.ToString(thisEntry.GetTotalScore()) + " pts" + Environment.NewLine;

            for (int i = 0; i < thisEntry.PlayerNames.Count; i++)
            {
                Rectangle thisLine = new Rectangle();
                thisLine.Fill = Colours[0];
                thisLine.Width = Width;
                thisLine.Height = thisEntry.PlayerScores[i] * heightScale;
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

            DisplayXLabel(Convert.ToString(thisEntry.GameID));

            VisualGraph.thisCanvas.MouseLeftButtonDown += ThisWindow_MouseLeftButtonDown;
        }

        private void ThisWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            foreach (var item in shapes)
            {
                if (e.Source == item)
                {
                    MessageBox.Show(GameStatsText);
                }
            }
        }

        public void DisplayXLabel(string IDToDisplay)
        {
            IDLabel.Width = Width * shapes.Count();
            IDLabel.Height = 40;
            IDLabel.FontSize = Width / 3;

            IDLabel.HorizontalContentAlignment = HorizontalAlignment.Center;

            if (IDLabel.FontSize > 20)
            {
                IDLabel.FontSize = 20;
            }
            else if (IDLabel.FontSize <= 8)
            {
                IDLabel.FontSize = 9;
                IDLabel.Width = Width;
                IDLabel.HorizontalContentAlignment = HorizontalAlignment.Left;
            }

            if (IDLabel.Width > 2 * IDLabel.FontSize)
            {
                IDLabel.Content = IDToDisplay;
            }

        }

        public Label GetLabel()
        {
            return IDLabel;
        }

        public List<Rectangle> GetShapes()
        {
            return shapes;
        }

    }

    class VisualGraph
    {
        public static Canvas thisCanvas;
        private List<BarLine> GraphLines = new List<BarLine>();
        private Line[] GraphSides = new Line[2];

        private List<DataEntry> DisplayedEntries = new List<DataEntry>();
        private int HighestScoreForGraph = 0;

        public VisualGraph(Canvas myCanvas)
        {
            thisCanvas = myCanvas;
        }

        public void DrawAxes(double[] availableArea, double[] indent, double currentLeft)
        {
            double fractionFromGraph = 0.05;

            GraphSides[0] = new Line() { Stroke = Brushes.DarkGray, StrokeThickness = 2 };

            thisCanvas.Children.Add(GraphSides[0]);

            GraphSides[0].X1 = indent[0] * (1 - fractionFromGraph);     //bottom left point
            GraphSides[0].Y1 = indent[1] * (1 + fractionFromGraph) + availableArea[1];
            GraphSides[0].X2 = currentLeft;                             //bottom right point
            GraphSides[0].Y2 = indent[1] * (1 + fractionFromGraph) + availableArea[1];

            GraphSides[1] = new Line() { Stroke = Brushes.DarkGray, StrokeThickness = 2 };

            thisCanvas.Children.Add(GraphSides[1]);

            GraphSides[1].X1 = indent[0] * (1 - fractionFromGraph);     //top left point
            GraphSides[1].Y1 = indent[1] * (1 - fractionFromGraph);
            GraphSides[1].X2 = indent[0] * (1 - fractionFromGraph);     //bottom left point
            GraphSides[1].Y2 = indent[1] * (1 + fractionFromGraph) + availableArea[1];
        }

        public void ClearCanvas()
        {
            thisCanvas.Children.RemoveRange(15, thisCanvas.Children.Count);
            //keeps the user input and labels, but removes the graph
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

        public void DisplayGraph(List<DataEntry> ChosenEntries, int GreatestScore)
        {
            double[] AvailableSpace = new double[2];

            GraphLines.Clear();
            ClearCanvas();

            HighestScoreForGraph = GreatestScore;

            double[] indent = new double[2];
            indent[0] = thisCanvas.ActualWidth * 0.05;
            indent[1] = thisCanvas.ActualHeight * 0.2;

            AvailableSpace[0] = thisCanvas.ActualWidth - (2 * indent[0]);
            AvailableSpace[1] = thisCanvas.ActualHeight - (1.5 * indent[1]);

            double newHeightScale = AvailableSpace[1] / HighestScoreForGraph;

            double shapeWidth = 10;
            double newWidthScale = AvailableSpace[0] / (2 * shapeWidth * GetNumOfBars(ChosenEntries));
            shapeWidth *= newWidthScale;

            if (shapeWidth > 150)
            {
                shapeWidth = 200;
            }
            else if (shapeWidth < 3)
            {
                shapeWidth = 3;
            }

            //if (shapeWidth < 20)
            //{
            //    MessageBox.Show("Too many records to show GameID on X-axis");
            //}

            BarLine.Width = shapeWidth;

            double CurrentLeft = indent[0] + 0.5 * shapeWidth;

            for (int i = 0; i < ChosenEntries.Count; i++)
            {
                BarLine thisBar = new BarLine(ChosenEntries[i], newHeightScale);

                GraphLines.Add(thisBar);

                if (shapeWidth >= 6)
                {
                    thisCanvas.Children.Add(thisBar.GetLabel());
                    Canvas.SetLeft(thisBar.GetLabel(), CurrentLeft);
                    Canvas.SetTop(thisBar.GetLabel(), indent[1] * 1.1 + AvailableSpace[1]);
                }

                foreach (var item in thisBar.GetShapes())
                {
                    thisCanvas.Children.Add(item);
                    Canvas.SetLeft(item, CurrentLeft);
                    Canvas.SetTop(item, indent[1] + AvailableSpace[1] - item.Height);
                    CurrentLeft += shapeWidth;
                }

                CurrentLeft += shapeWidth / 2;
            }

            DrawAxes(AvailableSpace, indent, CurrentLeft);

        }
    }

    public partial class ViewHistoryWindow : Window
    {
        private VisualGraph BarChart;
        private bool IncludeCapitals;
        private string SearchName;
        private int SortByType = 0;

        private List<DataEntry> AllFileEntries = new List<DataEntry>();
        private int HighestScoreForAllPlayers = 0;

        private Label HighestScoreLbl = new Label() { Width = 200, Height = 30 };
        private Label NumOfRecordsLbl = new Label() { Width = 200, Height = 30 };

        public ViewHistoryWindow()
        {
            InitializeComponent();
            IncCapsCheckBox.IsChecked = true;
            DateRadioBtn.IsChecked = true;

            BarChart = new VisualGraph(myCanvas);

            myCanvas.Children.Add(HighestScoreLbl);
            Canvas.SetRight(HighestScoreLbl, 20);
            Canvas.SetBottom(HighestScoreLbl, 50);
            myCanvas.Children.Add(NumOfRecordsLbl);
            Canvas.SetRight(NumOfRecordsLbl, 20);
            Canvas.SetBottom(NumOfRecordsLbl, 80);
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            SearchName = InputNameTxtBox.Text;

            if (SearchName.Length > 0)
            {
                IncludeCapitals = (bool)IncCapsCheckBox.IsChecked;

                List<DataEntry> EntriesToDisplay = GetAllResultsWithName(SearchName, IncludeCapitals);

                EntriesToDisplay = SortEntries(EntriesToDisplay, SortByType, SearchName, IncludeCapitals);

                if (EntriesToDisplay.Count > 0)
                {
                    NumOfRecordsLbl.Content = "Number Of Games Displayed: " + Convert.ToString(EntriesToDisplay.Count());
                    BarChart.DisplayGraph(EntriesToDisplay, FindGreatestScore(EntriesToDisplay));
                }
                else
                {
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

                    NumOfRecordsLbl.Content = "Number Of Games Displayed: " + 0;
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
                    SortedList = ToSort.OrderBy(DataEntry => DataEntry.Timestamp).ToList();
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

            HighestScoreForAllPlayers = FindGreatestScore(AllFileEntries);
            //gets the highest score in the file for all players 

            HighestScoreLbl.Content = "High Score For All Players: " + Convert.ToString(HighestScoreForAllPlayers) + "pts";
        }

        private List<DataEntry> GetAllResultsWithName(string NameToSearch, bool IncludeCapitals)
        {
            List<DataEntry> ChosenEntries = new List<DataEntry>();

            if (AllFileEntries.Count == 0)
            {
                DeserialiseFile();
            }

            foreach (var entry in AllFileEntries)
            {
                if (entry.SearchPlayers(NameToSearch, IncludeCapitals) != -1)
                {
                    ChosenEntries.Add(entry);
                    //isolates the data entries which contain the name we are searching for
                }
            }

            return ChosenEntries;

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
    }    
}
