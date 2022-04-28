using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Mazeman
{
    class ScorePt
    {
        private Image Sprite = new Image();
        private BitmapImage[] IMGSources = new BitmapImage[3];

        private Point PixelPt = new Point();
        private bool visible = true;
        public static int CurrentSourceIndex = 1;

        public ScorePt(Point PointParam, int[] cellDimensions)
        {
            for (int i = 0; i < IMGSources.Length; i++)
            {
                IMGSources[i] = new BitmapImage();
                IMGSources[i].BeginInit();
                IMGSources[i].UriSource = new Uri(GameConstants.SpriteFolderAddress + "/SP-" + i + ".png");
                IMGSources[i].EndInit();
            }

            Sprite.Source = IMGSources[CurrentSourceIndex];

            Sprite.Width = cellDimensions[0] / 3;
            Sprite.Height = cellDimensions[1] / 3;

            Game.CurrentWindow.GameCanvas.Children.Add(Sprite);
            Canvas.SetZIndex(Sprite, -50);

            PixelPt = new Point(PointParam.X + 0.5 * (cellDimensions[0] - Sprite.Width), PointParam.Y + 0.5 * (cellDimensions[1] - Sprite.Height));
            Draw();
        }

        public bool GetVisible()
        {
            return visible;
        }

        public void Clear()
        {
            Game.CurrentWindow.GameCanvas.Children.Remove(Sprite);
        }

        public void Hide()
        {
            Sprite.Opacity = 0;
            visible = false;
        }

        public void Draw()
        {
            Sprite.Source = IMGSources[CurrentSourceIndex];

            if (!visible)
            {
                Sprite.Opacity = 255;
                visible = true;
            }

            Canvas.SetLeft(Sprite, PixelPt.X);
            Canvas.SetTop(Sprite, PixelPt.Y);
        }
    }
}
