using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    public class Path
    {
        public interface IContour
        {
            void Transform(DenseMatrix matrix);
            IContour Copy();
            bool IsEmpty { get; }
            float X { get; }
            float Y { get; }
            float XMax { get; }
            float YMax { get; }
            float Lenght { get; }
            List<PointF> Points { get; }
            void Offset(float dx, float dy);
            bool PointAtDistance(float distance, ref double sum, ref PointF pointF, ref int indexP2);
            bool GetSegment(float startD, float stopD, ref Path dst, bool startWithMoveTo);
        }

        public class BezierCurve : IContour
        {
            public List<PointF> Points { get; }

            public BezierCurve(List<PointF> points)
            {
                Points = points.ToList();
            }

            public BezierCurve(params PointF[] points)
            {
                Points = points.ToList();
            }

            public void Transform(DenseMatrix matrix)
            {
                var denseMatrix = new DenseMatrix(3, Points.Count);
                for (var i = 0; i < Points.Count; i++)
                {
                    denseMatrix[0, i] = Points[i].X;
                    denseMatrix[1, i] = Points[i].Y;
                    denseMatrix[2, i] = 1;
                }
                var multiplied = matrix * denseMatrix;

                for (var i = 0; i < Points.Count; i++)
                    Points[i].Set(multiplied[0, i], multiplied[1, i]);
            }

            public IContour Copy()
            {
                return new BezierCurve(Points.Select(p2 => new PointF(p2.X, p2.Y)).ToList());
            }

            public bool IsEmpty => Points.Count == 0;
            public float X => Points.Min(p => p.X);
            public float Y => Points.Min(p => p.Y);
            public float XMax => Points.Max(p => p.X);
            public float YMax => Points.Max(p => p.Y);
            public float Lenght
            {
                get
                {
                    return 10;
                }
            }

            public void Offset(float dx, float dy)
            {
                foreach (var pointF in Points)
                {
                    pointF.X += dx;
                    pointF.Y += dy;
                }
            }

            public bool PointAtDistance(float distance, ref double sum, ref PointF pointF, ref int indexP2)
            {
                // TODO
                return false;
            }

            public bool GetSegment(float startD, float stopD, ref Path dst, bool startWithMoveTo)
            {
                // TODO
                return false;
            }

            public void AddBezier(PointF controlPoint1, PointF controlPoint2, PointF endPoint)
            {
                Points.AddRange(new[] { controlPoint1, controlPoint2, endPoint });
            }
        }

        public class Line : List<PointF>, IContour
        {
            public Line(IEnumerable<PointF> enumerable)
                : base(enumerable)
            {
            }

            public void Transform(DenseMatrix matrix)
            {
                var denseMatrix = new DenseMatrix(3, Count);
                for (var i = 0; i < Count; i++)
                {
                    denseMatrix[0, i] = this[i].X;
                    denseMatrix[1, i] = this[i].Y;
                    denseMatrix[2, i] = 1;
                }
                var multiplied = matrix * denseMatrix;

                for (var i = 0; i < Count; i++)
                    this[i].Set(multiplied[0, i], multiplied[1, i]);
            }

            public IContour Copy()
            {
                return new Line(this.Select(p2 => new PointF(p2.X, p2.Y)).ToList());
            }

            public bool IsEmpty => Count == 0;
            public float X => this.Min(point => point.X);
            public float Y => this.Min(point => point.Y);
            public float XMax => this.Max(point => point.X);
            public float YMax => this.Max(point => point.Y);
            public float Lenght
            {
                get
                {
                    double sum = 0;
                    for (var i = 0; i < Count - 1; i++)
                    {
                        var p1 = this[i];
                        var p2 = this[i + 1];
                        var p3 = new PointF(p2.X - p1.X, p2.Y - p1.Y);
                        sum += p3.Length();
                    }

                    return (float)sum;
                }
            }

            public List<PointF> Points => this;

            public void Offset(float dx, float dy)
            {
                for (var j = 0; j < Count; j++)
                {
                    this[j].X += dx;
                    this[j].Y += dy;
                }
            }

            public bool PointAtDistance(float distance, ref double sum, ref PointF pointF, ref int indexP2)
            {
                for (var i = 0; i < Count - 1; i++)
                {
                    var p1 = this[i];
                    var p2 = this[i + 1];
                    var d = new PointF(p2.X - p1.X, p2.Y - p1.Y).Length();
                    if (sum + d >= distance)
                    {
                        var p = 1 - (sum + d - distance) / d;
                        if (double.IsNaN(p))
                            p = 0;
                        indexP2 = i + 1;
                        pointF = new PointF((float)(p1.X + (p2.X - p1.X) * p), (float)(p1.Y + (p2.Y - p1.Y) * p));
                        return true;
                    }
                    sum += d;
                }
                indexP2 = Count - 1;
                pointF = this.Last();
                return false;
            }

            public bool GetSegment(float startD, float stopD, ref Path dst, bool startWithMoveTo)
            {
                PointF point1 = null;
                PointF point2 = null;
                double sum = 0;
                int indexP12 = -1;
                int indexP22 = -1;

                PointAtDistance(startD, ref sum, ref point1, ref indexP12);
                sum = 0;
                PointAtDistance(stopD, ref sum, ref point2, ref indexP22);

                if (point1 != null)
                {
                    if (startWithMoveTo)
                    {
                        dst.MoveTo(point1.X, point1.Y);
                    }

                    var newPoints = new Line(new List<PointF> { point1 }); // FirstPoint

                    var middlePoints = this.Skip(indexP12).Take(indexP22 - indexP12).ToList();

                    if (middlePoints.FirstOrDefault() == point1)
                        middlePoints.RemoveAt(0);

                    if (point2 != null && middlePoints.LastOrDefault() == point2)
                        middlePoints.RemoveAt(middlePoints.Count - 1);

                    newPoints.AddRange(middlePoints); // Path

                    if (point2 != null)
                        newPoints.Add(point2); // LastPoint

                    dst.AddPath(new Path
                    {
                        Contours = { newPoints }
                    });
                }

                return true;
            }
        }

        public PathFillType FillType { get; set; }

        public List<IContour> Contours { get; private set; }

        public float CurrentX { get; private set; }
        public float CurrentY { get; private set; }

        public Path()
        {
            Contours = new List<IContour>();
            FillType = PathFillType.Winding;
        }

        public void Set(Path path)
        {
            Contours = path.Contours.Select(p => p.Copy()).ToList();
            FillType = path.FillType;
            CurrentX = path.CurrentX;
            CurrentY = path.CurrentY;
        }

        public void Transform(DenseMatrix matrix)
        {
            for (var j = 0; j < Contours.Count; j++)
            {
                Contours[j].Transform(matrix);
            }

            var currentContour = new Line(new List<PointF> { new PointF(CurrentX, CurrentY) });
            currentContour.Transform(matrix);
            CurrentX = currentContour[0].X;
            CurrentY = currentContour[0].Y;
        }

        public void ComputeBounds(out Rect rect)
        {
            if (Contours.Sum(p => p.IsEmpty ? 0 : 1) <= 1)
            {
                RectExt.Set(ref rect, 0, 0, 0, 0);
                return;
            }

            double x = Contours.Min(p => p.X);
            double y = Contours.Min(p => p.Y);
            var width = Contours.Max(p => p.XMax) - x;
            var height = Contours.Max(p => p.YMax) - y;

            RectExt.Set(ref rect, x, y, width, height);
        }

        public void AddPath(Path path, DenseMatrix matrix)
        {
            var pathCopy = new Path();
            pathCopy.Set(path);
            pathCopy.Transform(matrix);
            Contours.AddRange(pathCopy.Contours);
        }

        public void AddPath(Path path)
        {
            Contours.AddRange(path.Contours.Select(p => p.Copy()).ToList());
        }

        public void Reset()
        {
            Contours.Clear();
            CurrentX = 0;
            CurrentY = 0;
            FillType = PathFillType.Winding;
        }

        public void MoveTo(float x, float y)
        {
            CurrentX = x;
            CurrentY = y;
        }

        public void CubicTo(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            var lastContour = Contours.LastOrDefault();
            if (lastContour is BezierCurve bezierCurve)
            {
                bezierCurve.AddBezier(
                    new PointF(x1, y1),
                    new PointF(x2, y2),
                    new PointF(x3, y3));
            }
            else
            {
                var bezier = new BezierCurve(
                    new PointF(CurrentX, CurrentY),
                    new PointF(x1, y1),
                    new PointF(x2, y2),
                    new PointF(x3, y3)
                );
                Contours.Add(bezier);
            }

            CurrentX = x3;
            CurrentY = y3;
        }

        public void LineTo(float x, float y)
        {
            var lastContour = Contours.LastOrDefault();
            if (lastContour is Line line)
            {
                var points = new Line(new List<PointF> { new PointF(x, y) });
                line.AddRange(points);
            }
            else
            {
                var newLine = new Line(new List<PointF> { new PointF(CurrentX, CurrentY), new PointF(x, y) });
                Contours.Add(newLine);
            }

            CurrentX = x;
            CurrentY = y;
        }

        public void Offset(float dx, float dy)
        {
            for (var i = 0; i < Contours.Count; i++)
            {
                Contours[i].Offset(dx, dy);
            }
        }

        public void Close()
        {
            var lastContour = Contours.LastOrDefault();
            if (lastContour == null)
            {
                return;
            }

            var firstPointOfLastContour = lastContour.Points.First();
            var lastPointOfLastContour = lastContour.Points.Last();
            if (!lastPointOfLastContour.Equals(firstPointOfLastContour))
                LineTo(firstPointOfLastContour.X, firstPointOfLastContour.Y);

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