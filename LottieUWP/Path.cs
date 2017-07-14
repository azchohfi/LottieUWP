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
            float[] Points { get; }
            PathIterator.ContourType Type { get; }
            void AddPathSegment(CanvasPathBuilder canvasPathBuilder, ref bool closed);
            void Offset(float dx, float dy);
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
            public PointF Control1 { get; }
            public PointF Control2 { get; }
            public PointF Vertex { get; }

            public BezierContour(PointF control1, PointF control2, PointF vertex)
            {
                Control1 = new PointF(control1.X, control1.Y);
                Control2 = new PointF(control2.X, control2.Y);
                Vertex = new PointF(vertex.X, vertex.Y);
            }

            public void Transform(DenseMatrix matrix)
            {
                var denseMatrix = new DenseMatrix(3, 3);
                var i = 0;
                foreach (var pointF in new[] { Control1, Control2, Vertex })
                {
                    denseMatrix[0, i] = pointF.X;
                    denseMatrix[1, i] = pointF.Y;
                    denseMatrix[2, i] = 1;
                    i++;
                }

                var multiplied = matrix * denseMatrix;

                Control1.Set(multiplied[0, 0], multiplied[1, 0]);
                Control2.Set(multiplied[0, 1], multiplied[1, 1]);
                Vertex.Set(multiplied[0, 2], multiplied[1, 2]);
            }

            public IContour Copy()
            {
                return new BezierContour(Control1, Control2, Vertex);
            }

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
                Control1.Offset(dx, dy);
                Control2.Offset(dx, dy);
                Vertex.Offset(dx, dy);
            }
        }

        class LineContour : IContour
        {
            private readonly PointF _point;

            public LineContour(float x, float y)
            {
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
                return new LineContour(_point.X, _point.Y);
            }

            public float[] Points => new[] { _point.X, _point.Y };

            public PathIterator.ContourType Type => PathIterator.ContourType.Line;

            public void AddPathSegment(CanvasPathBuilder canvasPathBuilder, ref bool closed)
            {
                canvasPathBuilder.AddLine(_point.X, _point.Y);

                closed = false;
            }

            public void Offset(float dx, float dy)
            {
                _point.Offset(dx, dy);
            }
        }

        class MoveToContour : IContour
        {
            private readonly PointF _point;

            public MoveToContour(float x, float y)
            {
                _point = new PointF(x, y);
            }

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
                canvasPathBuilder.BeginFigure(_point.X, _point.Y);
            }

            public void Offset(float dx, float dy)
            {
                _point.Offset(dx, dy);
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
            public float[] Points => new float[0];

            public PathIterator.ContourType Type => PathIterator.ContourType.Close;

            public IContour Copy()
            {
                return new CloseContour();
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
            }

            public void Transform(DenseMatrix matrix)
            {
            }
        }

        public PathFillType FillType { get; set; }

        public List<IContour> Contours { get; }

        public Path()
        {
            Contours = new List<IContour>();
            FillType = PathFillType.Winding;
        }

        public void Set(Path path)
        {
            Contours.Clear();
            Contours.AddRange(path.Contours.Select(p => p.Copy()));
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
                new PointF(x1, y1),
                new PointF(x2, y2),
                new PointF(x3, y3)
            );
            Contours.Add(bezier);
        }

        public void LineTo(float x, float y)
        {
            var newLine = new LineContour(x, y);
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
            Contours.Add(new CloseContour());
        }

        /*
         Set this path to the result of applying the Op to the two specified paths. The resulting path will be constructed from non-overlapping contours. The curve order is reduced where possible so that cubics may be turned into quadratics, and quadratics maybe turned into lines.
          Path1: The first operand (for difference, the minuend)
          Path2: The second operand (for difference, the subtrahend)
        */
        public void Op(Path path1, Path path2, CanvasGeometryCombine op)
        {
            // TODO
        }

        public void ArcTo(float x, float y, Rect rect, float startAngle, float sweepAngle)
        {
            var newArc = new ArcContour(new PointF(x, y), rect, startAngle, sweepAngle);
            Contours.Add(newArc);
        }
    }

    public enum PathFillType
    {
        EvenOdd,
        InverseWinding,
        Winding
    }
}