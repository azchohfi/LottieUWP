using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using System.Numerics;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas;

namespace LottieUWP
{
    public class Path
    {
        public interface IContour
        {
            void Transform(Matrix3X3 matrix);
            IContour Copy();
            float[] Points { get; }
            PathIterator.ContourType Type { get; }
            void AddPathSegment(CanvasPathBuilder canvasPathBuilder, ref bool closed);
            void Offset(float dx, float dy);
        }

        class ArcContour : IContour
        {
            private Vector2 _startPoint;
            private Vector2 _endPoint;
            private readonly Rect _rect;
            private readonly float _startAngle;
            private readonly float _sweepAngle;
            private readonly float _a;
            private readonly float _b;

            public ArcContour(Vector2 startPoint, Rect rect, float startAngle, float sweepAngle)
            {
                _startPoint = startPoint;
                _rect = rect;
                _a = (float)(rect.Width / 2);
                _b = (float)(rect.Height / 2);
                _startAngle = startAngle;
                _sweepAngle = sweepAngle;

                _endPoint = GetPointAtAngle(startAngle + sweepAngle);
            }

            public void Transform(Matrix3X3 matrix)
            {
                _startPoint = matrix.Transform(_startPoint);
                _endPoint = matrix.Transform(_endPoint);
            }

            public IContour Copy()
            {
                return new ArcContour(_startPoint, _rect, _startAngle, _sweepAngle);
            }

            public float[] Points => new[] { _startPoint.X, _startPoint.Y, _endPoint.X, _endPoint.Y };

            public PathIterator.ContourType Type => PathIterator.ContourType.Arc;

            public void AddPathSegment(CanvasPathBuilder canvasPathBuilder, ref bool closed)
            {
                canvasPathBuilder.AddArc(_endPoint, _a/2, _b/2, _sweepAngle, CanvasSweepDirection.Clockwise, CanvasArcSize.Small);

                closed = false;
            }

            public void Offset(float dx, float dy)
            {
                _startPoint.X += dx;
                _startPoint.Y += dy;
                _endPoint.X += dx;
                _endPoint.Y += dy;
            }

            private Vector2 GetPointAtAngle(float t)
            {
                var u = Math.Tan(MathExt.ToRadians(t) / 2);

                var u2 = u * u;

                var x = _a * (1 - u2) / (u2 + 1);
                var y = 2 * _b * u / (u2 + 1);

                return new Vector2((float)(_rect.Left + _a + x), (float)(_rect.Top + _b + y));
            }
        }

        internal class BezierContour : IContour
        {
            private Vector2 _control1;
            private Vector2 _control2;
            private Vector2 _vertex;

            public BezierContour(Vector2 control1, Vector2 control2, Vector2 vertex)
            {
                _control1 = control1;
                _control2 = control2;
                _vertex = vertex;
            }

            public void Transform(Matrix3X3 matrix)
            {
                _control1 = matrix.Transform(_control1);
                _control2 = matrix.Transform(_control2);
                _vertex = matrix.Transform(_vertex);
            }

            public IContour Copy()
            {
                return new BezierContour(_control1, _control2, _vertex);
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

            private static Vector2 GetPointAtT(float c0X, float c0Y, float c1X, float c1Y, float c2X, float c2Y, float c3X, float c3Y, double t)
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

                return new Vector2(ptX, ptY);
            }

            public float[] Points => new[] { _control1.X, _control1.Y, _control2.X, _control2.Y, _vertex.X, _vertex.Y };

            public PathIterator.ContourType Type => PathIterator.ContourType.Bezier;

            public void AddPathSegment(CanvasPathBuilder canvasPathBuilder, ref bool closed)
            {
                canvasPathBuilder.AddCubicBezier(_control1, _control2, _vertex);

                closed = false;
            }

            public void Offset(float dx, float dy)
            {
                _control1.X += dx;
                _control1.Y += dy;
                _control2.X += dx;
                _control2.Y += dy;
                _vertex.X += dx;
                _vertex.Y += dy;
            }
        }

        class LineContour : IContour
        {
            private readonly float[] _points = new float[2];

            public LineContour(float x, float y)
            {
                _points[0] = x;
                _points[1] = y;
            }

            public void Transform(Matrix3X3 matrix)
            {
                var p = new Vector2(_points[0], _points[1]);

                p = matrix.Transform(p);

                _points[0] = p.X;
                _points[1] = p.Y;
            }

            public IContour Copy()
            {
                return new LineContour(_points[0], _points[1]);
            }

            public float[] Points => _points;

            public PathIterator.ContourType Type => PathIterator.ContourType.Line;

            public void AddPathSegment(CanvasPathBuilder canvasPathBuilder, ref bool closed)
            {
                canvasPathBuilder.AddLine(_points[0], _points[1]);

                closed = false;
            }

            public void Offset(float dx, float dy)
            {
                _points[0] += dx;
                _points[1] += dy;
            }
        }

        class MoveToContour : IContour
        {
            private readonly float[] _points = new float[2];

            public MoveToContour(float x, float y)
            {
                _points[0] = x;
                _points[1] = y;
            }

            public float[] Points => _points;

            public PathIterator.ContourType Type => PathIterator.ContourType.MoveTo;

            public IContour Copy()
            {
                return new MoveToContour(_points[0], _points[1]);
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
                canvasPathBuilder.BeginFigure(_points[0], _points[1]);
            }

            public void Offset(float dx, float dy)
            {
                _points[0] += dx;
                _points[1] += dy;
            }

            public void Transform(Matrix3X3 matrix)
            {
                var p = new Vector2(_points[0], _points[1]);

                p = matrix.Transform(p);

                _points[0] = p.X;
                _points[1] = p.Y;
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

            public void Transform(Matrix3X3 matrix)
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

        public void Transform(Matrix3X3 matrix)
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

        public void AddPath(Path path, Matrix3X3 matrix)
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
                new Vector2(x1, y1),
                new Vector2(x2, y2),
                new Vector2(x3, y3)
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
            var newArc = new ArcContour(new Vector2(x, y), rect, startAngle, sweepAngle);
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