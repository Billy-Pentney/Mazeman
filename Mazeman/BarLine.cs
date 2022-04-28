using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Mazeman
{
    /// <summary>
    /// 
    /// Window responsible for showing the previous scores of players.
    /// 
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
                ShapeColour = Colours[thisEntry.Difficulty - 1];
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
            GameStatsText += Environment.NewLine + "Maze Dimensions: " + Convert.ToString(thisEntry.MazeDimensions[0]) + "x" + Convert.ToString(thisEntry.MazeDimensions[1]);
            GameStatsText += Environment.NewLine + "Enemy Difficulty: ";

            switch (thisEntry.Difficulty)
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
}
