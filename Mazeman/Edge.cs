using System.Windows;

namespace Mazeman
{
    class Edge
    {
        private Point StartPoint;
        private Point TargetPoint;

        public Edge(Point StartPt, Point EndPt)
        {
            StartPoint = StartPt;
            TargetPoint = EndPt;
        }

        public Point GetStartPoint()
        {
            //returns a specified vertex from the edge.
            return StartPoint;
        }

        public Point GetTargetPoint()
        {
            return TargetPoint;
        }
    }
}
