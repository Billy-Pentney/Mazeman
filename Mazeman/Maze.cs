using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Mazeman
{
    class Maze
    {
        private int[] MazeDimensions = new int[2];
        private int[] CellDimensions = new int[2];
        private int Thickness;

        private Wall[,] AllWallsH;
        private Wall[,] AllWallsV;

        private Cell[,] Cells;
        private int NumOfActivePoints;
        //the number of visible ScorePoints in the maze

        #region Maze Generation/Recursive Backtracker Variables
        private List<int> ValidMoves = new List<int>();
        private int randMoveSelection;
        private int thisMove;
        private Point nextCellPt = new Point();
        private Edge thisEdge;
        private Edge reverseEdge;
        private Random rand = new Random();
        private Stack<Point> VisitedCells = new Stack<Point>();
        private int RandNumOfWallsToRemove;
        private int TotalNumOfCells;
        private int NumOfMovesSinceSplit = 0;
        #endregion

        public Maze(int[] MazeDimensions)
        {
            ///number of grid spaces in the maze
            this.MazeDimensions = MazeDimensions;
            this.CellDimensions = GameConstants.CellDimensions;
            Thickness = (int)(CellDimensions[0] * GameConstants.WallThicknessProportion);

            AllWallsH = new Wall[MazeDimensions[0], MazeDimensions[1] + 1];
            AllWallsV = new Wall[MazeDimensions[0] + 1, MazeDimensions[1]];
            ///adds one on right/bottom edge for each array to "close" box

            Cells = new Cell[MazeDimensions[0], MazeDimensions[1]];

            InitialiseMaze();
            //creates instances of each wall/cell and sets their x/y coordinates
            DrawGrid();

            //Generates recursive maze starting at point 0,0 and continuing until all cells have been visited
            TotalNumOfCells = MazeDimensions[0] * MazeDimensions[1];
            NumOfActivePoints = TotalNumOfCells;

            MazeGeneration(GetLastCellInMaze(), false);
            //the false indicates that it should not start by backtracking
        }

        private void InitialiseMaze()
        {
            //creates individuals for each wall in the map

            for (int y = 0; y < AllWallsH.GetLength(1); y++)
            {
                for (int x = 0; x < AllWallsH.GetLength(0); x++)
                {
                    AllWallsH[x, y] = new Wall('h', x, y, CellDimensions, MazeDimensions, Thickness);
                }
            }

            for (int y = 0; y < AllWallsV.GetLength(1); y++)
            {
                for (int x = 0; x < AllWallsV.GetLength(0); x++)
                {
                    AllWallsV[x, y] = new Wall('v', x, y, CellDimensions, MazeDimensions, Thickness);
                }
            }

            for (int y = 0; y < Cells.GetLength(1); y++)
            {
                for (int x = 0; x < Cells.GetLength(0); x++)
                {
                    Point CanvasPoint = AllWallsH[x, y].GetPixelPt();
                    Point MazePoint = new Point(x, y);

                    CanvasPoint.X += Thickness;
                    CanvasPoint.Y += Thickness;

                    Cells[x, y] = new Cell(CanvasPoint, CellDimensions);
                }
            }
        }

        private void DrawGrid()
        {
            //displays all maze walls based on their unique x- and y-coordinates

            foreach (var wall in AllWallsV)
            {
                wall.Draw();
            }

            foreach (var wall in AllWallsH)
            {
                wall.Draw();
            }

        }

        private void MazeGeneration(Point currentCellPt, bool IsBacktracking)
        {
            ///RECURSIVE BACKRTRACKER
            ///Depth First Maze Generation Algorithm which uses recursive calls to step through cells

            //stores maze coordinates of maze cell currently being processed by algorithm

            GetAdjacentDirections(currentCellPt, IsBacktracking);
            //gets all possible moves from the current cell to adjacent ones

            if (ValidMoves.Count > 0)
            {
                VisitedCells.Push(currentCellPt);

                RandNumOfWallsToRemove = rand.Next(1, ValidMoves.Count());

                if (RandNumOfWallsToRemove > 1)
                {
                    NumOfMovesSinceSplit = 0;
                }
                else if (NumOfMovesSinceSplit > 2 && RandNumOfWallsToRemove < ValidMoves.Count())
                {
                    //if it has been 5 moves since the path split, then an additional wall is taken this time
                    RandNumOfWallsToRemove += 1;
                    NumOfMovesSinceSplit = 0;
                }
                else if (RandNumOfWallsToRemove < ValidMoves.Count())
                {
                    NumOfMovesSinceSplit += RandNumOfWallsToRemove;
                }

                for (int i = 0; i < RandNumOfWallsToRemove; i++)
                {
                    randMoveSelection = rand.Next(0, ValidMoves.Count);
                    thisMove = ValidMoves[randMoveSelection];
                    //gets a random valid move from the current cell to one of its neighbours

                    ValidMoves.RemoveAt(randMoveSelection);

                    nextCellPt = MoveFromPoint(currentCellPt, thisMove);
                    thisEdge = new Edge(currentCellPt, nextCellPt);

                    Cells[(int)currentCellPt.X, (int)currentCellPt.Y].AddEdge(thisEdge, thisMove);
                    //adds edge to the current cell

                    reverseEdge = new Edge(thisEdge.GetTargetPoint(), thisEdge.GetStartPoint());
                    //flips the edge so its inverse can be stored in the other cell

                    TurnOffWall(nextCellPt, thisMove);

                    Cells[(int)nextCellPt.X, (int)nextCellPt.Y].AddEdge(reverseEdge, ReverseMove(thisMove));
                    //adds opposite edge to the next cell
                }

                if (VisitedCells.Count < TotalNumOfCells)
                {
                    IsBacktracking = false;
                    MazeGeneration(nextCellPt, IsBacktracking);
                }

            }
            else if (VisitedCells.Count > 0)
            {
                IsBacktracking = true;
                MazeGeneration(VisitedCells.Pop(), IsBacktracking);
                //Gets the last cell to be visited and recursively calls algorithm from that cell
            }
        }

        public int[] GetMazeDimensions()
        {
            return MazeDimensions;
        }

        public Point GetPixelPoint(Point location)
        {
            return Cells[(int)location.X, (int)location.Y].GetPixelPt();
        }

        public Point GetLastCellInMaze()
        {
            return new Point(MazeDimensions[0] - 1, MazeDimensions[1] - 1);
        }

        public bool IsValidCell(Point Location)
        {
            //determines if the passed point corresponds to a valid point in the maze
            //takes Point, returns bool

            if (Location.X > -1 && Location.Y > -1 && Location.X < MazeDimensions[0] && Location.Y < MazeDimensions[1])
            {
                return true;
            }

            return false;
        }

        #region Path-finding (A*)

        public Queue<int> GeneratePathToTarget(Point Target, Point Current, List<Point> OccupiedPositions)
        {
            ///A* SEARCH ALGORITHM 
            ///Takes two points (T, C) in the maze and returns a queue of movements to move from C to T in the shortest valid path
            //NOTE: occupiedpositions is a list of points the Path should avoid if possible

            SimplePriorityQueue<Point, double> SearchCells = new SimplePriorityQueue<Point, double>();
            //search cells is the list of cells to be checked by the algorithm

            Dictionary<Point, double> MovesToReach = new Dictionary<Point, double>();
            //for each key, this is the number of moves to reach that point along the current best path

            Dictionary<Point, Point> PreviousCell = new Dictionary<Point, Point>();
            //each point directs back to the previous point in the path (i.e. where it came from on the path)
            //e.g. if the Enemy has to move from 1,1 to 0,1 and then from 0,1 to 0,0
            //Previous[0,1] = [1,1]
            //Previous[0,0] = [0,1]

            SearchCells.Enqueue(Current, 0);
            MovesToReach.Add(Current, 0);
            PreviousCell.Add(Current, new Point(-1, -1));
            //initialises start point of the search

            List<Point> AdjacentCells = new List<Point>();
            List<Point> NewPath = new List<Point>();

            int CostOfBlockage = 5;
            //the additional cost of moving through another occupied position

            if (OccupiedPositions.Contains(Current))
            {
                OccupiedPositions.Remove(Current);
            }

            double newCost = 0;
            double thisPriority = 0;

            bool pathFound = false;

            while (SearchCells.Count() != 0 && pathFound == false)
            {
                Current = SearchCells.Dequeue();
                //takes out next cell to be checked (based on priority)

                if (Current == Target)
                {
                    //exits loop if path reaches the target cell early
                    pathFound = true;
                }
                else
                {
                    AdjacentCells = GetAdjacentPoints(Current);
                    //gets all valid neighbouring cells to the current cell

                    foreach (var NextCell in AdjacentCells)
                    {
                        if (OccupiedPositions.Contains(NextCell))
                        {
                            newCost = MovesToReach[Current] + CostOfBlockage;
                            //if the path would include a blocked cell (i.e. one which is occupied by another enemy), then the cost is much higher
                        }
                        else
                        {
                            newCost = MovesToReach[Current] + 1;
                            //adds one to the total number of moves to reach that cell from the start (each move increases the path length by 1)
                        }

                        if (!MovesToReach.ContainsKey(NextCell) || newCost < MovesToReach[NextCell])
                        {
                            //if this path now includes a cell that hadn't been in explored before 
                            //or it is shorter than the previous best path to that cell,

                            MovesToReach[NextCell] = newCost;
                            thisPriority = newCost + GetApproximateDistance(NextCell, Target);
                            //the priority associated with moving to that cell is based on the approximate distance from it to the target

                            SearchCells.Enqueue(NextCell, thisPriority);
                            PreviousCell[NextCell] = Current;
                            //points from the new cell to the last one so that a path can be followed
                        }
                    }
                }
            }

            //at the end of the loop, PreviousCell points from each cell to the one before it on the path
            //now we need to remove unnecessary points (those that don't lead to the goal)
            //and reverse the list, before converting it to a set of instructions

            Point newkey = PreviousCell[Target];
            NewPath.Add(Target);

            while (newkey != new Point(-1, -1))
            {
                //at the start we said that the value for the start cell was [-1,-1]
                //if the key is [-1,-1], we must be at the start of the path

                NewPath.Add(newkey);
                newkey = PreviousCell[newkey];

                //creates list of points leading from the target point to the start point
            }

            NewPath.Reverse();
            //reverses the list so that the path is from the start to the target

            Queue<int> DirectionsToFollow = new Queue<int>();

            for (int i = 0; i < NewPath.Count - 1; i++)
            {
                DirectionsToFollow.Enqueue(ConvertMovementToDirection(NewPath[i], NewPath[i + 1]));
                //converts the list of points to instructions for the enemy to follow, and enqueues it in the return collection
            }

            return DirectionsToFollow;
        }

        public double GetApproximateDistance(Point current, Point target)
        {
            //finds manhattan/Pythagorean/direct distance from current point to target

            double dx = target.X - current.X;
            double dy = target.Y - current.Y;

            double distSquared = Math.Pow(dx, 2) + Math.Pow(dy, 2);
            return Math.Sqrt(distSquared);
        }

        #endregion

        #region Secondary Maze Generation Functions

        private void TurnOffWall(Point thisCell, int direction)
        {
            //hides the wall in the specified direction
            //used by Recursive Backtracker to hide the walls as it generates maze

            switch (direction)
            {
                case 0:
                    AllWallsH[(int)thisCell.X, (int)thisCell.Y + 1].Hide();
                    break;
                case 1:
                    AllWallsV[(int)thisCell.X, (int)thisCell.Y].Hide();
                    break;
                case 2:
                    AllWallsH[(int)thisCell.X, (int)thisCell.Y].Hide();
                    break;
                case 3:
                    AllWallsV[(int)thisCell.X + 1, (int)thisCell.Y].Hide();
                    break;
                default:
                    throw new Exception("Invalid direction when hiding wall");
            }
        }

        public int ReverseMove(int OriginalMove)
        {
            //takes int representing move (0, 1, 2, 3), 
            //0 = up, 1 = right, 2 = down, 3 = left

            //reverses move as follows : 0 -> 2, 1 -> 3, 2 -> 0, 3 -> 1

            return (OriginalMove + 2) % 4;

            //numOfDirections = 4, so    (0 + 2) % 4 = 2,    (1 + 2) % 4 = 3,     (2 + 2) % 4 = 0,    (3 + 2) % 4 = 1,

        }

        private void GetAdjacentDirections(Point Current, bool IsBacktracking)
        {
            ///Note: this method is for the backtracker algorithm to find the possible directions it can move in
            ///it runs BEFORE the graph of cells is created

            //ValidDirections stores the directions 0, 1, 2, 3 representing possible movements from the current cell
            //ex. Cell[0,0] is top-left, so it has 1, 2 (right, down)
            //ex-2. Cell[0,1] is one right of [0,0], so it has 1, 2, 3 (right, down, left)
            //ex-3. Cell[1,1] is one below [0,1], so it has 0, 1, 2, 3 (all directions) 

            List<Point> AdjacentPoints = new List<Point>
            {
                new Point(Current.X, Current.Y - 1),
                new Point(Current.X + 1, Current.Y),
                new Point(Current.X, Current.Y + 1),
                new Point(Current.X - 1, Current.Y),
            };

            ValidMoves.Clear();

            if (IsBacktracking == false)
            {
                //enters this statement when following new path of unvisited cells

                for (int i = 0; i < AdjacentPoints.Count; i++)
                {
                    if (IsValidCell(AdjacentPoints[i]) && !VisitedCells.Contains(AdjacentPoints[i]) && Cells[(int)Current.X, (int)Current.Y].GetEdgesCount() < 4)
                    {
                        ValidMoves.Add(i);
                        //determines which adjacent cells have not already been explored
                        //creates list of valid directions from current cell
                    }
                }
            }
            else if (IsBacktracking)
            {
                //enters this statement when backtracking along path

                for (int i = 0; i < AdjacentPoints.Count; i++)
                {
                    if (IsValidCell(AdjacentPoints[i]) && Cells[(int)Current.X, (int)Current.Y].GetEdgeFromDirection(i) == null && Cells[(int)AdjacentPoints[i].X, (int)AdjacentPoints[i].Y].GetEdgesCount() == 0 && Cells[(int)Current.X, (int)Current.Y].GetEdgesCount() < 4)
                    {
                        ValidMoves.Add(i);
                        //determines which adjacent cells (if any) have not already been explored
                    }
                }
            }
        }

        #endregion

        #region Gameplay Functions

        public int[] PointCrossed(Point newPoint)
        {
            int[] NumOfVisiblePoints = new int[2];

            //the number of points in the maze before the point is collected
            NumOfVisiblePoints[0] = NumOfActivePoints;

            if (Cells[(int)newPoint.X, (int)newPoint.Y].HideScorePoint())
            {
                //represents whether the cell has been collected in this movement
                //(false if the scorepoint was already hidden from a player crossing it previously)

                NumOfActivePoints -= 1;
            }

            //the number of points in the maze after the point was collected
            NumOfVisiblePoints[1] = NumOfActivePoints;

            return NumOfVisiblePoints;
        }

        public void ClearMaze()
        {
            foreach (var cell in Cells)
            {
                cell.ClearScorePoint();
            }

            foreach (var wall in AllWallsH)
            {
                wall.Clear();
            }

            foreach (var wall in AllWallsV)
            {
                wall.Clear();
            }
        }

        public void SetScorePointColour(double effectVal)
        {
            int ColourIndex = (int)Math.Truncate(effectVal);

            if (ColourIndex >= 0 && ColourIndex < 3)
            {
                //sets colour of the scorepoints based on the effect of the powerup
                //

                ScorePt.CurrentSourceIndex = ColourIndex;
            }

            foreach (var cell in Cells)
            {
                if (cell.IsPointVisible())
                {
                    cell.DrawScorePoint();
                }
            }
        }

        #endregion

        public List<Point> GetAdjacentPoints(Point Current)
        {
            //Note: this method is for getting the adjacent cells via the graph
            //it determines which points are adjacent based off of the edges of the current cell

            List<Point> PointsList = new List<Point>();
            Edge currentEdge;

            for (int i = 0; i < 4; i++)
            {
                currentEdge = Cells[(int)Current.X, (int)Current.Y].GetEdgeFromDirection(i);

                if (currentEdge != null)
                {
                    PointsList.Add(currentEdge.GetTargetPoint());
                }
            }

            return PointsList;
        }

        public Point MoveFromPoint(Point startPt, int direction)
        {
            //returns the next cell in that direction if possible

            switch (direction)
            {
                case 0:
                    startPt.Y -= 1;
                    break;
                case 1:
                    startPt.X += 1;
                    break;
                case 2:
                    startPt.Y += 1;
                    break;
                case 3:
                    startPt.X -= 1;
                    break;
                default:
                    throw new Exception("Attempted to move in invalid direction");
            }

            bool validMove = IsValidCell(startPt);

            if (validMove)
            {
                return startPt;
            }
            else
            {
                throw new Exception("Attempt to move to invalid point");
            }
        }

        public bool CheckEdgeInDirection(Point position, int direction)
        {
            if (Cells[(int)position.X, (int)position.Y].GetEdgeFromDirection(direction) != null)
            {
                return true;
            }

            return false;
        }

        public int ConvertMovementToDirection(Point start, Point target)
        {
            //converts the movement between two points into the four directions of motion

            if (target.Y == start.Y - 1)
            {
                return 0;
            }
            else if (target.X == start.X + 1)
            {
                return 1;
            }
            else if (target.Y == start.Y + 1)
            {
                return 2;
            }
            else if (target.X == start.X - 1)
            {
                return 3;
            }

            throw new Exception("Attempt to convert two invalid points to a movement");
        }

        public int GetNumofScorePoints()
        {
            return NumOfActivePoints;
        }

    }
}
