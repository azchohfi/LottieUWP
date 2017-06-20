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
            bool IsEmpty { get; }
            float X { get; }
            float Y { get; }
            float XMax { get; }
            float YMax { get; }
            float Lenght { get; }
            PointF First { get; }
            PointF Last { get; }
            PathSegment GetPathSegment();
            void Offset(float dx, float dy);
            bool PointAtDistance(float distance, ref double sum, ref PointF pointF, ref int indexP2);
            bool GetSegment(float startD, float stopD, ref Path dst, bool startWithMoveTo);
        }

        public class Arc : IContour
        {
            private readonly PointF _startPoint;
            private readonly PointF _endPoint;
            private readonly float _width;
            private readonly float _height;
            private readonly float _sweepAngle;

            public Arc(PointF startPoint, PointF endPoint, float width, float height, float sweepAngle)
            {
                _startPoint = new PointF(startPoint.X, startPoint.Y);
                _endPoint = new PointF(endPoint.X, endPoint.Y);
                _width = width;
                _height = height;
                _sweepAngle = sweepAngle;
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
                return new Arc(_startPoint, _endPoint, _width, _height, _sweepAngle);
            }

            public bool IsEmpty => _startPoint.Equals(_endPoint);
            public float X => Math.Min(_startPoint.X, _endPoint.X);
            public float Y => Math.Min(_startPoint.Y, _endPoint.Y);
            public float XMax => Math.Max(_startPoint.X, _endPoint.X);
            public float YMax => Math.Max(_startPoint.Y, _endPoint.Y);

            // since angle is always 90º, lenght is always 1/4 of elipse lenght
            public float Lenght => (float)(Math.PI * (3 * (_width + _height) - Math.Sqrt((3 * _width + _height) * (_width + 3 * _height))) / 4);

            public PointF First => _startPoint;

            public PointF Last => _endPoint;

            public PathSegment GetPathSegment()
            {
                return new ArcSegment
                {
                    SweepDirection = SweepDirection.Clockwise,
                    RotationAngle = _sweepAngle,
                    Point = new Point(_endPoint.X, _endPoint.Y),
                    Size = new Size(_width, _height)
                };
            }

            public void Offset(float dx, float dy)
            {
                _endPoint.X += dx;
                _endPoint.Y += dy;
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
        }

        public class BezierCurve : IContour
        {
            private readonly List<PointF> _points;

            public BezierCurve(List<PointF> points)
            {
                _points = points.ToList();
            }

            public BezierCurve(params PointF[] points)
            {
                _points = points.ToList();
            }

            public void Transform(DenseMatrix matrix)
            {
                var denseMatrix = new DenseMatrix(3, _points.Count);
                for (var i = 0; i < _points.Count; i++)
                {
                    denseMatrix[0, i] = _points[i].X;
                    denseMatrix[1, i] = _points[i].Y;
                    denseMatrix[2, i] = 1;
                }
                var multiplied = matrix * denseMatrix;

                for (var i = 0; i < _points.Count; i++)
                    _points[i].Set(multiplied[0, i], multiplied[1, i]);
            }

            public IContour Copy()
            {
                return new BezierCurve(_points.Select(p2 => new PointF(p2.X, p2.Y)).ToList());
            }

            public bool IsEmpty => _points.Count == 0;
            public float X => _points.Min(p => p.X);
            public float Y => _points.Min(p => p.Y);
            public float XMax => _points.Max(p => p.X);
            public float YMax => _points.Max(p => p.Y);
            public float Lenght
            {
                get
                {
                    double sum = 0;
                    for (int i = 0; i + 3 < _points.Count; i += 3)
                    {
                        sum += BezLength(_points[i], _points[i + 1], _points[i + 2], _points[i + 3]);
                    }
                    return (float)sum;
                }
            }

            double BezLength(PointF c0, PointF c1, PointF c2, PointF c3)
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

                var ptX = (float) (c0.X * t13 + t13A * c1.X + t13B * c2.X + t13C * c3.X);
                var ptY = (float) (c0.Y * t13 + t13A * c1.Y + t13B * c2.Y + t13C * c3.Y);

                var pt = new PointF(ptX, ptY);
                return pt;
            }

            public PointF First => _points.First();

            public PointF Last => _points.Last();

            public PathSegment GetPathSegment()
            {
                var pointCollection = new PointCollection();
                foreach (var pointF in _points.Skip(1))
                {
                    pointCollection.Add(new Point(pointF.X, pointF.Y));
                }
                return new PolyBezierSegment
                {
                    Points = pointCollection
                };
            }

            public void Offset(float dx, float dy)
            {
                foreach (var pointF in _points)
                {
                    pointF.X += dx;
                    pointF.Y += dy;
                }
            }

            public bool PointAtDistance(float distance, ref double sum, ref PointF pointF, ref int indexP2)
            {
                for (int i = 0; i + 3 < _points.Count; i += 3)
                {
                    var d = BezLength(_points[i], _points[i + 1], _points[i + 2], _points[i + 3]);
                    if (sum + d >= distance)
                    {
                        var p = 1 - (sum + d - distance) / d;
                        if (double.IsNaN(p))
                            p = 0;
                        indexP2 = i + 1;
                        pointF = GetPointAtT(_points[i], _points[i + 1], _points[i + 2], _points[i + 3], p);
                        return true;
                    }
                    sum += d;
                }
                indexP2 = _points.Count - 1;
                pointF = _points.Last();
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

                    // TODO

                    var newPoints = new Line(new List<PointF> { point1 }); // FirstPoint

                    var middlePoints = _points.Skip(indexP12).Take(indexP22 - indexP12).ToList();

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

            public void AddBezier(PointF controlPoint1, PointF controlPoint2, PointF endPoint)
            {
                _points.AddRange(new[] { controlPoint1, controlPoint2, endPoint });
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

            public PointF First => this.First();

            public PointF Last => this.Last();

            public PathSegment GetPathSegment()
            {
                var pointCollection = new PointCollection();
                foreach (var pointF in this)
                {
                    pointCollection.Add(new Point(pointF.X, pointF.Y));
                }
                return new PolyLineSegment
                {
                    Points = pointCollection
                };
            }

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

        private float _currentX;
        private float _currentY;

        public Path()
        {
            Contours = new List<IContour>();
            FillType = PathFillType.Winding;
        }

        public void Set(Path path)
        {
            Contours = path.Contours.Select(p => p.Copy()).ToList();
            FillType = path.FillType;
            _currentX = path._currentX;
            _currentY = path._currentY;
        }

        public void Transform(DenseMatrix matrix)
        {
            for (var j = 0; j < Contours.Count; j++)
            {
                Contours[j].Transform(matrix);
            }

            var currentContour = new Line(new List<PointF> { new PointF(_currentX, _currentY) });
            currentContour.Transform(matrix);
            _currentX = currentContour[0].X;
            _currentY = currentContour[0].Y;
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
            _currentX = 0;
            _currentY = 0;
            FillType = PathFillType.Winding;
        }

        public void MoveTo(float x, float y)
        {
            _currentX = x;
            _currentY = y;
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
                    new PointF(_currentX, _currentY),
                    new PointF(x1, y1),
                    new PointF(x2, y2),
                    new PointF(x3, y3)
                );
                Contours.Add(bezier);
            }

            _currentX = x3;
            _currentY = y3;
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
                var newLine = new Line(new List<PointF> { new PointF(_currentX, _currentY), new PointF(x, y) });
                Contours.Add(newLine);
            }

            _currentX = x;
            _currentY = y;
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
            var firstContour = Contours.FirstOrDefault();
            if (firstContour == null)
            {
                return;
            }

            var firstPointOfFirstContour = firstContour.First;
            if (firstPointOfFirstContour.X != _currentX || firstPointOfFirstContour.Y != _currentY)
                LineTo(firstPointOfFirstContour.X, firstPointOfFirstContour.Y);
        }

        public void Op(Path firstPath, Path remainderPath, Op op1)
        {

        }

        public void ArcTo(float x, float y, float sweepAngle)
        {
            var newArc = new Arc(new PointF(_currentX, _currentY), new PointF(x, y), Math.Abs(_currentX - x), Math.Abs(_currentY - y), sweepAngle);
            Contours.Add(newArc);

            var last = newArc.Last;
            _currentX = last.X;
            _currentY = last.Y;
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