using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    public class Path
    {
        public PathFillType FillType { get; set; }

        public List<List<PointF>> Points { get; private set; }

        public float? CurrentX { get; private set; }
        public float? CurrentY { get; private set; }
        private bool _isClosed;

        public Path()
        {
            Points = new List<List<PointF>>();
            FillType = PathFillType.Winding;
            _isClosed = false;
        }

        public void Set(Path path)
        {
            Points = path.Points.Select(p => p.Select(p2 => new PointF(p2.X, p2.Y)).ToList()).ToList();
            FillType = path.FillType;
            CurrentX = path.CurrentX;
            CurrentY = path.CurrentY;
            _isClosed = path._isClosed;
        }

        public void Transform(DenseMatrix matrix)
        {
            for (int j = 0; j < Points.Count; j++)
            {
                TransformContour(matrix, Points[j]);
            }
            if (CurrentX != null && CurrentY != null)
            {
                var currentPoint = new List<PointF> {new PointF(CurrentX.Value, CurrentY.Value)};
                TransformContour(matrix, currentPoint);
                CurrentX = currentPoint[0].X;
                CurrentY = currentPoint[0].Y;
            }
        }

        private static void TransformContour(DenseMatrix matrix, List<PointF> contour)
        {
            var denseMatrix = new DenseMatrix(3, contour.Count);
            for (int i = 0; i < contour.Count; i++)
            {
                denseMatrix[0, i] = contour[i].X;
                denseMatrix[1, i] = contour[i].Y;
                denseMatrix[2, i] = 1;
            }
            var multiplied = matrix * denseMatrix;

            for (int i = 0; i < contour.Count; i++)
                contour[i].Set(multiplied[0, i], multiplied[1, i]);
        }

        public void ComputeBounds(out Rect rect, bool b)
        {
        }

        public void AddPath(Path path, DenseMatrix matrix)
        {
            var pathCopy = new Path();
            pathCopy.Set(path);
            pathCopy.Transform(matrix);
            Points.AddRange(pathCopy.Points);
        }

        public void AddPath(Path path)
        {
            Points.AddRange(path.Points.Select(p => p.Select(p2 => new PointF(p2.X, p2.Y)).ToList()).ToList());
        }

        public void Reset()
        {
            Points.Clear();
            CurrentX = null;
            CurrentY = null;
            FillType = PathFillType.Winding;
            _isClosed = false;
        }

        public void MoveTo(float x, float y)
        {
            CurrentX = x;
            CurrentY = y;
            _isClosed = false;
        }

        public void CubicTo(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            var lastContour = Points.LastOrDefault();
            if (lastContour == null)
            {
                lastContour = new List<PointF>();
                Points.Add(lastContour);
            }
            lastContour.AddRange(new List<PointF> { new PointF(CurrentX.Value, CurrentY.Value), new PointF(x1, y1), new PointF(x2, y2), new PointF(x3, y3) });
            CurrentX = x3;
            CurrentY = y3;
        }

        public void LineTo(float x, float y)
        {
            var lastContour = Points.LastOrDefault();
            if (lastContour == null)
            {
                lastContour = new List<PointF>();
                Points.Add(lastContour);
            }
            lastContour.AddRange(new List<PointF> { new PointF(CurrentX.Value, CurrentY.Value), new PointF(x, y) });
            CurrentX = x;
            CurrentY = y;
        }

        public void Offset(float dx, float dy)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                for (int j = 0; j < Points[i].Count; j++)
                {
                    Points[i][j].X += dx;
                    Points[i][j].Y += dy;
                }
            }
        }

        public void Close()
        {
            if (_isClosed == false)
            {
                _isClosed = true;

                var lastContour = Points.LastOrDefault();
                if (lastContour == null)
                {
                    return;
                }
                var firstPointOfLastContour = lastContour.First();
                var lastPointOfLastContour = lastContour.Last();
                if (!lastPointOfLastContour.Equals(firstPointOfLastContour))
                    lastContour.Add(new PointF(firstPointOfLastContour.X, firstPointOfLastContour.Y));
            }

            //Close the current contour. 
            //If the current point is not equal to the first point of the contour, 
            //a line segment is automatically added. 
        }

        public void Op(Path firstPath, Path remainderPath, Op op1)
        {

        }

        public void ArcTo(Rect rect, int p1, int p2, bool p3)
        {

        }
    }

    public enum Op
    {
        Union,
        ReverseDifference,
        Intersect,
        Xor
    }

    public enum PathFillType
    {
        EvenOdd,
        InverseWinding,
        Winding
    }
}