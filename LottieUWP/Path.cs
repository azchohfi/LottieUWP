using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    public class Path
    {
        public interface IContour
        {
            void Transform(DenseMatrix matrix);
            IContour Copy();
            float X { get; }
            float Y { get; }
            float XMax { get; }
            float YMax { get; }
            float Lenght { get; }
            PointF First { get; }
            PointF Last { get; }
            bool AddPathSegment(PathFigure pathFigure);
            void Offset(float dx, float dy);
            PointF PointAtDistance(float distance);
        }

        public class ArcContour : IContour
        {
            private readonly PointF _startPoint;
            private readonly PointF _endPoint;
            private readonly Rect _rect;
            private readonly float _startAngle;
            private readonly float _sweepAngle;
            private readonly float _a;
            private readonly float _b;

            public ArcContour(PointF startPoint, Rect rect, float startAngle, float sweepAngle)
            {
                _startPoint = startPoint;
                _rect = rect;
                _a = (float)(rect.Width / 2);
                _b = (float)(rect.Height / 2);
                _startAngle = startAngle;
                _sweepAngle = sweepAngle;

                _endPoint = GetPointAtAngle(startAngle + sweepAngle);
            }

            public void Transform(DenseMatrix matrix)
            {
                var points = new[] { _startPoint, _endPoint };

                var denseMatrix = new DenseMatrix(3, points.Length);
                for (var i = 0; i < points.Length; i++)
                {
                    denseMatrix[0, i] = points[i].X;
                    denseMatrix[1, i] = points[i].Y;
                    denseMatrix[2, i] = 1;
                }
                var multiplied = matrix * denseMatrix;

                _startPoint.Set(multiplied[0, 0], multiplied[1, 0]);
                _endPoint.Set(multiplied[0, 1], multiplied[1, 1]);
            }

            public IContour Copy()
            {
                return new ArcContour(_startPoint, _rect, _startAngle, _sweepAngle);
            }

            public float X => Math.Min(_startPoint.X, _endPoint.X);
            public float Y => Math.Min(_startPoint.Y, _endPoint.Y);
            public float XMax => Math.Max(_startPoint.X, _endPoint.X);
            public float YMax => Math.Max(_startPoint.Y, _endPoint.Y);

            public float Lenght => (float)(Math.PI * (3 * (_a + _b) - Math.Sqrt((3 * _a + _b) * (_a + 3 * _b))) / (360 / _sweepAngle));

            public PointF First => _startPoint;

            public PointF Last => _endPoint;

            public bool AddPathSegment(PathFigure pathFigure)
            {
                pathFigure.Segments.Add(new ArcSegment
                {
                    SweepDirection = SweepDirection.Clockwise,
                    RotationAngle = _sweepAngle,
                    IsLargeArc = false,
                    Point = new Point(_endPoint.X, _endPoint.Y),
                    Size = new Size(_a / 2, _b / 2)
                });

                return true;
            }

            public void Offset(float dx, float dy)
            {
                _startPoint.Offset(dx, dy);
                _endPoint.Offset(dx, dy);
            }

            public PointF PointAtDistance(float distance)
            {
                var p = distance / Lenght;

                var t = _startAngle + p * _sweepAngle;

                return GetPointAtAngle(t);
            }

            private PointF GetPointAtAngle(float t)
            {
                var u = Math.Tan(MathExt.ToRadians(t) / 2);

                var u2 = u * u;

                var x = _a * (1 - u2) / (u2 + 1);
                var y = 2 * _b * u / (u2 + 1);

                return new PointF((float)(_rect.Left + _a + x), (float)(_rect.Top + _b + y));
            }
        }

        public class BezierContour : IContour
        {
            public PointF StartPoint { get; }
            public PointF Control1 { get; }
            public PointF Control2 { get; }
            public PointF Vertex { get; }

            public BezierContour(PointF startPoint, PointF control1, PointF control2, PointF vertex)
            {
                StartPoint = new PointF(startPoint.X, startPoint.Y);
                Control1 = new PointF(control1.X, control1.Y);
                Control2 = new PointF(control2.X, control2.Y);
                Vertex = new PointF(vertex.X, vertex.Y);
            }

            public void Transform(DenseMatrix matrix)
            {
                var denseMatrix = new DenseMatrix(3, 4);
                var i = 0;
                foreach (var pointF in new[] { StartPoint, Control1, Control2, Vertex })
                {
                    denseMatrix[0, i] = pointF.X;
                    denseMatrix[1, i] = pointF.Y;
                    denseMatrix[2, i] = 1;
                    i++;
                }

                var multiplied = matrix * denseMatrix;

                StartPoint.Set(multiplied[0, 0], multiplied[1, 0]);
                Control1.Set(multiplied[0, 1], multiplied[1, 1]);
                Control2.Set(multiplied[0, 2], multiplied[1, 2]);
                Vertex.Set(multiplied[0, 3], multiplied[1, 3]);
            }

            public IContour Copy()
            {
                return new BezierContour(StartPoint, Control1, Control2, Vertex);
            }

            public float X => Math.Min(StartPoint.X, Math.Min(Control1.X, Math.Min(Control2.X, Vertex.X)));
            public float Y => Math.Min(StartPoint.Y, Math.Min(Control1.Y, Math.Min(Control2.Y, Vertex.Y)));
            public float XMax => Math.Max(StartPoint.X, Math.Max(Control1.X, Math.Max(Control2.X, Vertex.X)));
            public float YMax => Math.Max(StartPoint.Y, Math.Max(Control1.Y, Math.Max(Control2.Y, Vertex.Y)));
            public float Lenght => (float)BezLength(StartPoint, Control1, Control2, Vertex);

            private static double BezLength(PointF c0, PointF c1, PointF c2, PointF c3)
            {
                const double steps = 1000d; // TODO: improve

                var length = 0d;
                var prevPt = new PointF();

                for (var i = 0d; i < steps; i++)
                {
                    var pt = GetPointAtT(c0, c1, c2, c3, i / steps);

                    if (i > 0)
                    {
                        var x = pt.X - prevPt.X;
                        var y = pt.Y - prevPt.Y;
                        length = length + Math.Sqrt(x * x + y * y);
                    }

                    prevPt.X = pt.X;
                    prevPt.Y = pt.Y;
                }
                return length;
            }

            private static PointF GetPointAtT(PointF c0, PointF c1, PointF c2, PointF c3, double t)
            {
                var t1 = 1d - t;

                if (t1 < 5e-6)
                {
                    t = 1.0;
                    t1 = 0.0;
                }

                var t13 = t1 * t1 * t1;
                var t13A = 3 * t * (t1 * t1);
                var t13B = 3 * t * t * t1;
                var t13C = t * t * t;

                var ptX = (float)(c0.X * t13 + t13A * c1.X + t13B * c2.X + t13C * c3.X);
                var ptY = (float)(c0.Y * t13 + t13A * c1.Y + t13B * c2.Y + t13C * c3.Y);

                var pt = new PointF(ptX, ptY);
                return pt;
            }

            public PointF First => StartPoint;

            public PointF Last => Vertex;

            public bool AddPathSegment(PathFigure pathFigure)
            {
                pathFigure.Segments.Add(new BezierSegment
                {
                    Point1 = new Point(Control1.X, Control1.Y),
                    Point2 = new Point(Control2.X, Control2.Y),
                    Point3 = new Point(Vertex.X, Vertex.Y)
                });
                return true;
            }

            public void Offset(float dx, float dy)
            {
                StartPoint.Offset(dx, dy);
                Control1.Offset(dx, dy);
                Control2.Offset(dx, dy);
                Vertex.Offset(dx, dy);
            }

            public PointF PointAtDistance(float distance)
            {
                var d = BezLength(StartPoint, Control1, Control2, Vertex);

                if (d >= distance)
                {
                    var p = 1 - (d - distance) / d;
                    if (double.IsNaN(p))
                        p = 0;
                    return GetPointAtT(StartPoint, Control1, Control2, Vertex, p);
                }

                return null;
            }
        }

        public class LineContour : IContour
        {
            private readonly PointF _origin;
            private readonly PointF _point;

            public LineContour(float xOrigin, float yOrigin, float x, float y)
            {
                _origin = new PointF(xOrigin, yOrigin);
                _point = new PointF(x, y);
            }

            public void Transform(DenseMatrix matrix)
            {
                var denseMatrix = new DenseMatrix(3, 1)
                {
                    [0, 0] = _point.X,
                    [1, 0] = _point.Y,
                    [2, 0] = 1
                };

                var multiplied = matrix * denseMatrix;

                _point.Set(multiplied[0, 0], multiplied[1, 0]);
            }

            public IContour Copy()
            {
                return new LineContour(_origin.X, _origin.Y, _point.X, _point.Y);
            }

            public float X => Math.Min(_origin.X, _point.X);
            public float Y => Math.Min(_origin.Y, _point.Y);
            public float XMax => Math.Max(_origin.X, _point.X);
            public float YMax => Math.Max(_origin.Y, _point.Y);
            public float Lenght => (float)new PointF(_origin.X - _point.X, _origin.Y - _point.Y).Length();

            public PointF First => _origin;

            public PointF Last => _point;

            public bool AddPathSegment(PathFigure pathFigure)
            {
                pathFigure.Segments.Add(new LineSegment
                {
                    Point = new Point(_point.X, _point.Y)
                });
                return true;
            }

            public void Offset(float dx, float dy)
            {
                _origin.Offset(dx, dy);
                _point.Offset(dx, dy);
            }

            public PointF PointAtDistance(float distance)
            {
                var dist = new PointF(_point.X - _origin.X, _point.Y - _origin.Y);
                var d = dist.Length();
                if (d >= distance)
                {
                    var p = 1 - (d - distance) / d;
                    if (double.IsNaN(p))
                        p = 0;
                    return new PointF((float)(_origin.X + dist.X * p), (float)(_origin.Y + dist.Y * p));
                }

                return null;
            }
        }

        public class MoveToContour : IContour
        {
            private readonly PointF _point;

            public MoveToContour(float x, float y)
            {
                _point = new PointF(x, y);
            }

            public float X => _point.X;

            public float Y => _point.Y;

            public float XMax => _point.X;

            public float YMax => _point.Y;

            public float Lenght => 0;

            public PointF First => _point;

            public PointF Last => _point;

            public IContour Copy()
            {
                return new MoveToContour(_point.X, _point.Y);
            }

            public bool AddPathSegment(PathFigure pathFigure)
            {
                pathFigure.StartPoint = new Point(_point.X, _point.Y);
                return true;
            }

            public void Offset(float dx, float dy)
            {
                _point.Offset(dx, dy);
            }

            public PointF PointAtDistance(float distance)
            {
                if (distance == 0)
                {
                    return new PointF(_point.X, _point.Y);
                }

                return null;
            }

            public void Transform(DenseMatrix matrix)
            {
                var denseMatrix = new DenseMatrix(3, 1)
                {
                    [0, 0] = _point.X,
                    [1, 0] = _point.Y,
                    [2, 0] = 1
                };
                var multiplied = matrix * denseMatrix;

                _point.Set(multiplied[0, 0], multiplied[1, 0]);
            }
        }

        public class CloseContour : IContour
        {
            private readonly PointF _point;

            public CloseContour(float x, float y)
            {
                _point = new PointF(x, y);
            }

            public float X => _point.X;

            public float Y => _point.Y;

            public float XMax => _point.X;

            public float YMax => _point.Y;

            public float Lenght => 0;

            public PointF First => _point;

            public PointF Last => _point;

            public IContour Copy()
            {
                return new CloseContour(_point.X, _point.Y);
            }

            public bool AddPathSegment(PathFigure pathFigure)
            {
                pathFigure.IsClosed = true;
                return false;
            }

            public void Offset(float dx, float dy)
            {
                _point.Offset(dx, dy);
            }

            public PointF PointAtDistance(float distance)
            {
                if (distance == 0)
                {
                    return new PointF(_point.X, _point.Y);
                }

                return null;
            }

            public void Transform(DenseMatrix matrix)
            {
                var denseMatrix = new DenseMatrix(3, 1)
                {
                    [0, 0] = _point.X,
                    [1, 0] = _point.Y,
                    [2, 0] = 1
                };
                var multiplied = matrix * denseMatrix;

                _point.Set(multiplied[0, 0], multiplied[1, 0]);
            }
        }

        public PathFillType FillType { get; set; }

        public List<IContour> Contours { get; private set; }

        public Path()
        {
            Contours = new List<IContour>();
            FillType = PathFillType.Winding;
        }

        public void Set(Path path)
        {
            Contours = path.Contours.Select(p => p.Copy()).ToList();
            FillType = path.FillType;
        }

        public void Transform(DenseMatrix matrix)
        {
            for (var j = 0; j < Contours.Count; j++)
            {
                Contours[j].Transform(matrix);
            }
        }

        public void ComputeBounds(out Rect rect)
        {
            if (Contours.Count == 0)
            {
                RectExt.Set(ref rect, 0, 0, 0, 0);
                return;
            }

            double left = Contours.Min(p => p.X);
            double top = Contours.Min(p => p.Y);
            var right = Contours.Max(p => p.XMax);
            var bottom = Contours.Max(p => p.YMax);

            RectExt.Set(ref rect, left, top, right, bottom);
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
            FillType = PathFillType.Winding;
        }

        public void MoveTo(float x, float y)
        {
            Contours.Add(new MoveToContour(x, y));
        }

        public void CubicTo(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            var bezier = new BezierContour(
                new PointF(CurrentX, CurrentY),
                new PointF(x1, y1),
                new PointF(x2, y2),
                new PointF(x3, y3)
            );
            Contours.Add(bezier);
        }

        private float CurrentX => Contours.Last().Last.X;
        private float CurrentY => Contours.Last().Last.Y;

        public void LineTo(float x, float y)
        {
            var newLine = new LineContour(CurrentX, CurrentY, x, y);
            Contours.Add(newLine);
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
            var lastClose = Contours.LastOrDefault(c => c is CloseContour);
            PointF firstPoint;
            if (lastClose == null)
            {
                lastClose = Contours.FirstOrDefault();
                if (lastClose == null)
                {
                    return;
                }
                firstPoint = lastClose.First;
            }
            else
            {
                var index = Contours.IndexOf(lastClose) + 1;
                if (index >= Contours.Count)
                    return;
                firstPoint = Contours[index].First;
            }

            var close = new CloseContour(firstPoint.X, firstPoint.Y);
            Contours.Add(close);
        }

        public void Op(Path firstPath, Path remainderPath, Op op1)
        {

        }

        public void ArcTo(Rect rect, float startAngle, float sweepAngle)
        {
            var newArc = new ArcContour(new PointF(CurrentX, CurrentY), rect, startAngle, sweepAngle);
            Contours.Add(newArc);
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