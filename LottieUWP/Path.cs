using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using System.Numerics;
using Microsoft.Graphics.Canvas.Geometry;
using MathNet.Numerics.LinearAlgebra.Single;
using Microsoft.Graphics.Canvas;

namespace LottieUWP
{
    public class Path
    {
        public interface IContour
        {
            void Transform(DenseMatrix matrix);
            IContour Copy();
            float Lenght { get; }
            PointF First { get; }
            PointF Last { get; }
            float[] Points { get; }
            PathIterator.ContourType Type { get; }
            void AddPathSegment(CanvasPathBuilder canvasPathBuilder, ref bool closed);
            void Offset(float dx, float dy);
            PointF PointAtDistance(float distance);
        }

        class ArcContour : IContour
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

            public float Lenght => (float)(Math.PI * (3 * (_a + _b) - Math.Sqrt((3 * _a + _b) * (_a + 3 * _b))) / (360 / _sweepAngle));

            public PointF First => _startPoint;

            public PointF Last => _endPoint;

            public float[] Points => new[] { _startPoint.X, _startPoint.Y, _endPoint.X, _endPoint.Y };

            public PathIterator.ContourType Type => PathIterator.ContourType.Arc;

            public void AddPathSegment(CanvasPathBuilder canvasPathBuilder, ref bool closed)
            {
                canvasPathBuilder.AddArc(new Vector2(_endPoint.X, _endPoint.Y), 
                    _a/2, _b/2, _sweepAngle, CanvasSweepDirection.Clockwise, CanvasArcSize.Small);

                closed = false;
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

        internal class BezierContour : IContour
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

            public float Lenght => (float)BezLength(StartPoint.X, StartPoint.Y, 
                Control1.X, Control1.Y,
                Control2.X, Control2.Y, 
                Vertex.X, Vertex.Y);

            internal static double BezLength(float c0X, float c0Y, float c1X, float c1Y, float c2X, float c2Y, float c3X, float c3Y)
            {
                const double steps = 1000d; // TODO: improve

                var length = 0d;
                float prevPtX = 0;
                float prevPtY = 0;

                for (var i = 0d; i < steps; i++)
                {
                    var pt = GetPointAtT(c0X, c0Y, c1X, c1Y, c2X, c2Y, c3X, c3Y, i / steps);

                    if (i > 0)
                    {
                        var x = pt.X - prevPtX;
                        var y = pt.Y - prevPtY;
                        length = length + Math.Sqrt(x * x + y * y);
                    }

                    prevPtX = pt.X;
                    prevPtY = pt.Y;
                }
                return length;
            }

            private static PointF GetPointAtT(float c0X, float c0Y, float c1X, float c1Y, float c2X, float c2Y, float c3X, float c3Y, double t)
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

                var ptX = (float)(c0X * t13 + t13A * c1X + t13B * c2X + t13C * c3X);
                var ptY = (float)(c0Y * t13 + t13A * c1Y + t13B * c2Y + t13C * c3Y);

                var pt = new PointF(ptX, ptY);
                return pt;
            }

            public PointF First => StartPoint;

            public PointF Last => Vertex;

            public float[] Points => new[] { Control1.X, Control1.Y, Control2.X, Control2.Y, Vertex.X, Vertex.Y };

            public PathIterator.ContourType Type => PathIterator.ContourType.Bezier;

            public void AddPathSegment(CanvasPathBuilder canvasPathBuilder, ref bool closed)
            {
                canvasPathBuilder.AddCubicBezier(
                    new Vector2(Control1.X, Control1.Y),
                    new Vector2(Control2.X, Control2.Y),
                    new Vector2(Vertex.X, Vertex.Y));

                closed = false;
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
                var d = BezLength(StartPoint.X, StartPoint.Y, Control1.X, Control1.Y, Control2.X, Control2.Y, Vertex.X, Vertex.Y);

                if (d >= distance)
                {
                    var p = 1 - (d - distance) / d;
                    if (double.IsNaN(p))
                        p = 0;
                    return GetPointAtT(StartPoint.X, StartPoint.Y, Control1.X, Control1.Y, Control2.X, Control2.Y, Vertex.X, Vertex.Y, p);
                }

                return null;
            }
        }

        class LineContour : IContour
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

            public float Lenght => (float)new PointF(_origin.X - _point.X, _origin.Y - _point.Y).Length();

            public PointF First => _origin;

            public PointF Last => _point;

            public float[] Points => new[] { _point.X, _point.Y };

            public PathIterator.ContourType Type => PathIterator.ContourType.Line;

            public void AddPathSegment(CanvasPathBuilder canvasPathBuilder, ref bool closed)
            {
                canvasPathBuilder.AddLine(_point.X, _point.Y);

                closed = false;
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

        class MoveToContour : IContour
        {
            private readonly PointF _point;

            public MoveToContour(float x, float y)
            {
                _point = new PointF(x, y);
            }

            public float Lenght => 0;

            public PointF First => _point;

            public PointF Last => _point;

            public float[] Points => new[] { _point.X, _point.Y };

            public PathIterator.ContourType Type => PathIterator.ContourType.MoveTo;

            public IContour Copy()
            {
                return new MoveToContour(_point.X, _point.Y);
            }

            public void AddPathSegment(CanvasPathBuilder canvasPathBuilder, ref bool closed)
            {
                if (!closed)
                {
                    canvasPathBuilder.EndFigure(CanvasFigureLoop.Open);
                }
                else
                {
                    closed = false;
                }
                canvasPathBuilder.BeginFigure(First.X, First.Y);
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

        class CloseContour : IContour
        {
            private readonly PointF _point;

            public CloseContour(float x, float y)
            {
                _point = new PointF(x, y);
            }

            public float Lenght => 0;

            public PointF First => _point;

            public PointF Last => _point;

            public float[] Points => new float[0];

            public PathIterator.ContourType Type => PathIterator.ContourType.Close;

            public IContour Copy()
            {
                return new CloseContour(_point.X, _point.Y);
            }

            public void AddPathSegment(CanvasPathBuilder canvasPathBuilder, ref bool closed)
            {
                if (!closed)
                {
                    canvasPathBuilder.EndFigure(CanvasFigureLoop.Closed);
                    closed = true;
                }
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

        public CanvasGeometry GetGeometry(CanvasDevice device)
        {
            var fill = FillType == PathFillType.Winding
                ? CanvasFilledRegionDetermination.Winding
                : CanvasFilledRegionDetermination.Alternate;
            //    FillRule = path.FillType == PathFillType.EvenOdd ? FillRule.EvenOdd : FillRule.Nonzero,

            var canvasPathBuilder = new CanvasPathBuilder(device);
            canvasPathBuilder.SetFilledRegionDetermination(fill);

            var closed = true;

            for (var i = 0; i < Contours.Count; i++)
            {
                Contours[i].AddPathSegment(canvasPathBuilder, ref closed);
            }

            if (!closed)
                canvasPathBuilder.EndFigure(CanvasFigureLoop.Open);

            return CanvasGeometry.CreatePath(canvasPathBuilder);
        }

        public void ComputeBounds(out Rect rect)
        {
            if (Contours.Count == 0)
            {
                RectExt.Set(ref rect, 0, 0, 0, 0);
                return;
            }

            var geometry = GetGeometry(CanvasDevice.GetSharedDevice());
            rect = geometry.ComputeBounds();
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

        public void Op(Path path1, Path path2, Op op)
        {
            // TODO
        }

        public void ArcTo(Rect rect, float startAngle, float sweepAngle)
        {
            var newArc = new ArcContour(new PointF(CurrentX, CurrentY), rect, startAngle, sweepAngle);
            Contours.Add(newArc);
        }

        public PointF PathPointAtDistance(float distance, out IContour contourAtDistance)
        {
            float sum = 0;
            foreach (var contour in Contours)
            {
                var contourLenght = contour.Lenght;
                if (distance - sum <= contourLenght)
                {
                    var point = contour.PointAtDistance(distance - sum);
                    if (point != null)
                    {
                        contourAtDistance = contour;
                        return point;
                    }
                }
                sum += contourLenght;
            }

            contourAtDistance = null;
            return null;
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