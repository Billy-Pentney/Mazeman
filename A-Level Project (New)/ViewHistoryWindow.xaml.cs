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
        private static Brush[] Colours = new Brush[]{ Brushes.Blue, Brushes.LightBlue };

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
        private Label HighestScoreLbl = new Label() { Width = 200, Height = 30 };
        private Label NumOfRecordsLbl = new Label() { Width = 200, Height = 30 };

        public VisualGraph(Canvas myCanvas)
        {
            thisCanvas = myCanvas;

            thisCanvas.Children.Add(HighestScoreLbl);
            Canvas.SetRight(HighestScoreLbl, 40);
            Canvas.SetTop(HighestScoreLbl, 100);
            thisCanvas.Children.Add(NumOfRecordsLbl);
            Canvas.SetRight(NumOfRecordsLbl, 40);
            Canvas.SetTop(NumOfRecordsLbl, 130);
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
            thisCanvas.Children.RemoveRange(14, thisCanvas.Children.Count);
            //keeps the user input and labels, but removes the graph
        }

        public void GetAllResultsWithName(string NameToSearch, bool FindCaseVariations, int SortByType)
        {
            JsonSerializer JS = new JsonSerializer();
            List<DataEntry> FileEntries = new List<DataEntry>();
            List<DataEntry> ChosenEntries = new List<DataEntry>();

            int LastHighestScoreForALL = 0;

            using (StreamReader reader = new StreamReader(GameConstants.FileName))
            {
                using (JsonTextReader jreader = new JsonTextReader(reader))
                {
                    FileEntries = (List<DataEntry>)JS.Deserialize(jreader, FileEntries.GetType());
                    //gets all entries from the file and stores in a list of data entries
                }
            }

            LastHighestScoreForALL = FindGreatestScore(FileEntries); 
            //gets the highest score in the file for all players 

            foreach (var entry in FileEntries)
            {
                 if (entry.SearchPlayers(NameToSearch, FindCaseVariations) != -1)
                 {
                     ChosenEntries.Add(entry);
                     //isolates the data entries which contain the name we are searching for
                 }
            }

            HighestScoreLbl.Content = "High Score For All Players: " + Convert.ToString(LastHighestScoreForALL);
            NumOfRecordsLbl.Content = "Number Of Games Displayed: " + Convert.ToString(ChosenEntries.Count());

            if (ChosenEntries.Count > 0)
            {
                //List<DataEntry> OrderedEntries = ChosenEntries.OrderBy(DataEntry => DataEntry.GetScoreFromName(NameToSearch, FindCaseVariations)).ToList<DataEntry>();
                //orders the selected entries by the score which corresponds to the name to be found

                List<DataEntry> OrderedEntries = SortBySpecifiedType(ChosenEntries, SortByType, NameToSearch, FindCaseVariations);

                ClearCanvas();
                DisplayGraph(OrderedEntries);
            }
            else
            {
                string MessageToShow = "'" + NameToSearch + "'";

                if (FindCaseVariations && NameToSearch.ToLower() != NameToSearch)
                {
                    MessageToShow += ", " + "'" + NameToSearch.ToLower() + "'";
                }

                MessageToShow += " not found in file history";
                MessageBox.Show(MessageToShow);
            }

        }

        private List<DataEntry> SortBySpecifiedType(List<DataEntry> ToSort, int SortType, string thisName, bool includeCapitals)
        {
            List<DataEntry> SortedList = new List<DataEntry>();

            switch (SortType)
            {
                case 0:
                    SortedList = ToSort.OrderBy(DataEntry => DataEntry.Timestamp).ToList();
                    break;
                case 1:
                    SortedList = ToSort.OrderBy(DataEntry => DataEntry.SurvivedFor).ToList();
                    break;
                case 2:
                    SortedList = ToSort.OrderBy(DataEntry => DataEntry.GetTotalScore()).ToList();
                    break;
                case 3:
                    SortedList = ToSort.OrderBy(DataEntry => DataEntry.GetScoreFromName(thisName, includeCapitals)).ToList();
                    break;
                default:
                    break;
            }

            return SortedList;
        }

        private int FindGreatestScore(List<DataEntry> EntriesToSearch)
        {
            int currentGreatest = 0;
            int thisScore;

            foreach (var item in EntriesToSearch)
            {
                thisScore = item.GetHighestScore();

                if (thisScore > currentGreatest)
                {
                    currentGreatest = thisScore;
                }
            }

            return currentGreatest;
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

        public void DisplayGraph(List<DataEntry> ChosenEntries)
        {
            double[] AvailableSpace = new double[2];

            GraphLines.Clear();

            double[] indent = new double[2];
            indent[0] = thisCanvas.ActualWidth * 0.05;
            indent[1] = thisCanvas.ActualHeight * 0.2;

            AvailableSpace[0] = thisCanvas.ActualWidth - (2 * indent[0]);
            AvailableSpace[1] = thisCanvas.ActualHeight - (1.5 * indent[1]);

            int LargestScore = FindGreatestScore(ChosenEntries);
            double newHeightScale = AvailableSpace[1] / LargestScore;

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
        private VisualGraph GraphDisplay;
        string SearchName;
        int SortByType = 0;

        public ViewHistoryWindow()
        {
            InitializeComponent();
            LowerUpperCheckBox.IsChecked = true;

            GraphDisplay = new VisualGraph(myCanvas);
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            string NameToSearch = InputNameTxtBox.Text;
            
            GraphDisplay.GetAllResultsWithName(NameToSearch, (bool)LowerUpperCheckBox.IsChecked, SortByType);
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
