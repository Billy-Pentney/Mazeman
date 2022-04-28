using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Mazeman
{
    class Wall
    {
        private double width;
        private double height;
        private Rectangle Shape = new Rectangle { Fill = GameConstants.WallColours[0], };
        private Point PixelPt;
        private bool hideable = true;                 //used to prevent hiding outer edge walls

        public Wall(char typeParam, int i, int j, int[] cellDimensions, int[] mazeDimensions, int thickness)
        {
            //sets size of wall based on whether horizontal or vertical

            if (typeParam == 'h')
            {
                //horizontal wall conditions

                width = cellDimensions[0] + thickness;
                height = thickness;

                if (j == 0 || j == mazeDimensions[1])
                {
                    //walls can be hidden unless they are on the outer edge of the maze
                    hideable = false;
                }
            }
            else if (typeParam == 'v')
            {
                //vertical wall conditions

                width = thickness;
                height = cellDimensions[1] + thickness;

                if (i == 0 || i == mazeDimensions[0])
                {
                    //walls can be hidden unless they are on the outer edge of the maze
                    hideable = false;
                }
            }

            Shape.Width = this.width;
            Shape.Height = this.height;

            PixelPt.X = GameConstants.MazeIndent[0] + (i + 1) * cellDimensions[0];
            PixelPt.Y = GameConstants.MazeIndent[1] + (j + 1) * cellDimensions[1];
            //calculates position of wall based on location in array

            Game.CurrentWindow.GameCanvas.Children.Add(Shape);
        }

        public Point GetPixelPt()
        {
            return PixelPt;
        }

        public void Clear()
        {
            if (Game.CurrentWindow.GameCanvas.Children.Contains(Shape))
            {
                Game.CurrentWindow.GameCanvas.Children.Remove(Shape);
            }
        }

        public void Hide()
        {
            if (hideable)
            {
                Game.CurrentWindow.GameCanvas.Children.Remove(Shape);
            }
        }

        public void Draw()
        {
            Canvas.SetLeft(Shape, PixelPt.X);
            Canvas.SetTop(Shape, PixelPt.Y);
        }

    }
}
