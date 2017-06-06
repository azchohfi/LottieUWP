using System;

namespace LottieUWP
{
    internal class MiscUtils
    {
        internal static PointF AddPoints(PointF p1, PointF p2)
        {
            return new PointF(p1.X + p2.X, p1.Y + p2.Y);
        }

        internal static void GetPathFromData(ShapeData shapeData, Path outPath)
        {
            outPath.Reset();
            var initialPoint = shapeData.InitialPoint;
            outPath.MoveTo(initialPoint.X, initialPoint.Y);
            var currentPoint = new PointF(initialPoint.X, initialPoint.Y);
            for (var i = 0; i < shapeData.Curves.Count; i++)
            {
                var curveData = shapeData.Curves[i];
                var cp1 = curveData.ControlPoint1;
                var cp2 = curveData.ControlPoint2;
                var vertex = curveData.Vertex;

                if (cp1.Equals(currentPoint) && cp2.Equals(vertex))
                {
                    // On some phones like Samsung phones, zero valued control points can cause artifacting.
                    // https://github.com/airbnb/lottie-android/issues/275
                    //
                    // This does its best to add a tiny value to the vertex without affecting the final
                    // animation as much as possible.
                    // outPath.rMoveTo(0.01f, 0.01f);
                    outPath.LineTo(vertex.X, vertex.Y);
                }
                else
                {
                    outPath.CubicTo(cp1.X, cp1.Y, cp2.X, cp2.Y, vertex.X, vertex.Y);
                }
                currentPoint.X = vertex.X;
                currentPoint.Y = vertex.Y;
            }
            if (shapeData.Closed)
            {
                outPath.Close();
            }
        }

        internal static float Lerp(float a, float b, float percentage)
        {
            return a + percentage * (b - a);
        }

        internal static double Lerp(double a, double b, double percentage)
        {
            return a + percentage * (b - a);
        }

        internal static int Lerp(int a, int b, float percentage)
        {
            return (int)(a + percentage * (b - a));
        }

        internal static int FloorMod(float x, float y)
        {
            return FloorMod((int)x, (int)y);
        }

        internal static int FloorMod(int x, int y)
        {
            return x - FloorDiv(x, y) * y;
        }

        private static int FloorDiv(int x, int y)
        {
            var r = x / y;
            // if the signs are different and modulo not zero, round down
            if ((x ^ y) < 0 && r * y != x)
            {
                r--;
            }
            return r;
        }

        internal static float Clamp(float number, float min, float max)
        {
            return Math.Max(min, Math.Min(max, number));
        }

        internal static double Clamp(double number, double min, double max)
        {
            return Math.Max(min, Math.Min(max, number));
        }
    }
}