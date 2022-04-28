using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Mazeman
{
    class Cell
    {
        private Point ShapePt;  ///point in top-left of cell
        private Dictionary<int, Edge> Edges = new Dictionary<int, Edge>();
        private ScorePt ScorePoint;

        public Cell(Point CanvasLoc, int[] cellDimensions)
        {
            ShapePt = CanvasLoc;
            ScorePoint = new ScorePt(ShapePt, cellDimensions);
        }

        public void ClearScorePoint()
        {
            ScorePoint.Clear();
        }

        public Edge GetEdgeFromDirection(int key)
        {
            if (Edges.ContainsKey(key))
            {
                return Edges[key];
            }
            else
            {
                return null;
            }
        }

        public int GetEdgesCount()
        {
            return Edges.Count();
        }

        public Point GetPixelPt()
        {
            return ShapePt;
        }

        public bool AddEdge(Edge newEdge, int Direction)
        {
            int numOfEdges = Edges.Count();

            if (!Edges.ContainsKey(Direction))
            {
                Edges.Add(Direction, newEdge);
                return true;
            }

            return false;
        }

        public bool HideScorePoint()
        {
            if (ScorePoint.GetVisible())
            {
                ScorePoint.Hide();
                return true;
                //returns true if the point had not been collected before
            }

            //returns false if it was already hidden
            return false;
        }

        public void DrawScorePoint()
        {
            ScorePoint.Draw();
        }

        public bool IsPointVisible()
        {
            return ScorePoint.GetVisible();
        }
    }
}
