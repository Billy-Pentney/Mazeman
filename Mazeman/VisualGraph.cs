using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Mazeman
{
    class VisualGraph
    {
        public static Canvas thisCanvas;
        private List<BarLine> GraphLines = new List<BarLine>();
        private Line[] GraphSides = new Line[2];

        // Components used for positioning the graph and bars correctly
        private double[] BottomLeftIndent = new double[2];    //[0] = from left, [1] = from bottom
        private double[] TopRightIndent = new double[2];      //[0] = from right, [1] = from top
        private double[] AvailableArea = new double[2];       //[0] = width of graph, [1] = height of graph

        // The interval of the bars which are currently visible
        //  First index describes which bar is on the extreme left of the window
        //  Second index describes which bar is on the extreme right of the window
        private int[] DisplayRange = new int[2];

        private List<DataEntry> EntriesToDisplay = new List<DataEntry>();
        private int HighestScoreForGraph = 0;

        public VisualGraph(Canvas myCanvas)
        {
            thisCanvas = myCanvas;
        }

        public void CreateGraph(List<DataEntry> ChosenEntries, int GreatestScore)
        {
            #region Preparing Area/Indents
            ClearCanvas();

            BottomLeftIndent[0] = 60;
            BottomLeftIndent[1] = 60;

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
            // 1.5 is the multiplier because the graph is half of the width, plus a bar

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

            // Convert each entry into a single bar object, and add it to the Lines array
            foreach (var entry in ChosenEntries)
            {
                BarLine thisBar = new BarLine(entry);
                GraphLines.Add(thisBar);
            }

            // Determines the range of bars which can be displayed based on their width e.g. 0-10 of 20 bars
            DisplayRange[0] = 0;
            DisplayRange[1] = DetermineDisplayRange(shapeWidth, 0);

            DrawGraph();
        }

        public void DrawGraph()
        {
            double shapeWidth = BarLine.GetWidth();
            double currentLeft = BottomLeftIndent[0] + shapeWidth / 2;

            for (int i = DisplayRange[0]; i <= DisplayRange[1]; i++)
            {
                foreach (var bar in GraphLines[i].GetShapes())
                {
                    if (!thisCanvas.Children.Contains(bar))
                    {
                        thisCanvas.Children.Add(bar);
                    }

                    Canvas.SetLeft(bar, currentLeft);
                    // Align the bottom of the bars with the bottom of the graph
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

            for (int i = 0; i < GraphSides.Length; i++)
            {
                // Initialises the axes lines once only
                if (!thisCanvas.Children.Contains(GraphSides[i]))
                {
                    GraphSides[i] = new Line() { Stroke = Brushes.DarkGray, StrokeThickness = 2 };
                    thisCanvas.Children.Add(GraphSides[i]);
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
            // Gets the pixel position for the far end of that graph axes

            if (type == 'x')
            {
                // Returns the end of line for the x-axis
                return GraphSides[0].X2;
            }
            else if (type == 'y')
            {
                // Returns the end of line for the y-axis
                return GraphSides[1].Y1;
            }

            return 0;
        }

        public void ClearCanvas()
        {
            // Keeps the user input and labels, but removes the graph

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
            // Determines the number of bars not displayed on the left
            int UndisplayedLeft = DisplayRange[0];

            // Determines the number of bars not displayed on the right
            int UndisplayedRight = GraphLines.Count - 1 - DisplayRange[1];

            double[] OldDisplayRange = new double[] { DisplayRange[0], DisplayRange[1] };

            // If the user tries to move left and there are bars to be shown
            if (UndisplayedLeft > 0 && difference < 0)
            {
                DisplayRange[0] -= 1;
                // Determines how many bars to show
                // Note: this must be recalculated, since we cannot assume that the bars are all the 
                // same width (due to Two-Player games requiring two bars)
                DisplayRange[1] = DetermineDisplayRange(BarLine.GetWidth(), DisplayRange[0]);

                // Removes any bars which have fallen out of the display range
                for (int i = DisplayRange[1]; i <= OldDisplayRange[1]; i++)
                {
                    GraphLines[i].RemoveAllFromCanvas();
                }

                // Redraw with the changes
                DrawGraph();
            }
            // Otherwise, if the user tries to move right and there are bars to be shown in that direction
            else if (UndisplayedRight > 0 && difference > 0)
            {
                // Always removes the left-most bar
                GraphLines[DisplayRange[0]].RemoveAllFromCanvas();

                DisplayRange[0] += 1;
                // Determines how many bars to show
                // Note: this must be recalculated, since we cannot assume that the bars are all the 
                // same width (due to Two-Player games requiring two bars)
                DisplayRange[1] = DetermineDisplayRange(BarLine.GetWidth(), DisplayRange[0]);

                // Also, redraw with the changes
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
            return DisplayRange;
        }

        public int GetNumOfEntries()
        {
            return EntriesToDisplay.Count;
        }
    }
}
