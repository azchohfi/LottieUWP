using System;
using Windows.Graphics.Display;
using Windows.UI;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    internal static class Utils
    {
        private static readonly PathMeasure PathMeasure = new PathMeasure();
        private static Path _tempPath = new Path();
        private static Path _tempPath2 = new Path();
        private static float[] _points = new float[4];
        private static readonly float Sqrt2 = (float)Math.Sqrt(2);

        internal static Path CreatePath(PointF startPoint, PointF endPoint, PointF cp1, PointF cp2)
        {
            var path = new Path();
            path.MoveTo(startPoint.X, startPoint.Y);

            if (cp1 != null && cp2 != null && (cp1.LengthSquared() != 0 || cp2.LengthSquared() != 0))
            {
                path.CubicTo(startPoint.X + cp1.X, startPoint.Y + cp1.Y, endPoint.X + cp2.X, endPoint.Y + cp2.Y, endPoint.X, endPoint.Y);
            }
            else
            {
                path.LineTo(endPoint.X, endPoint.Y);
            }
            return path;
        }

        public static void CloseQuietly(this IDisposable closeable)
        {
            if (closeable != null)
            {
                try
                {
                    closeable.Dispose();
                }
                //catch (RuntimeException rethrown)
                //{
                //    throw rethrown;
                //}
                catch (Exception)
                {
                    // Really quietly
                }
            }
        }

        internal static int GetScreenWidth()
        {
            return (int) DisplayInformation.GetForCurrentView().ScreenWidthInRawPixels;
        }

        internal static int GetScreenHeight()
        {
            return (int)DisplayInformation.GetForCurrentView().ScreenHeightInRawPixels;
        }

        internal static float GetScale(DenseMatrix matrix)
        {
            _points[0] = 0;
            _points[1] = 0;
            // Use sqrt(2) so that the hypotenuse is of length 1.
            _points[2] = Sqrt2;
            _points[3] = Sqrt2;
            matrix.MapPoints(ref _points);
            var dx = _points[2] - _points[0];
            var dy = _points[3] - _points[1];

            // TODO: figure out why the result needs to be divided by 2.
            return (float)MathExt.Hypot(dx, dy) / 2f;
        }

        internal static void ApplyTrimPathIfNeeded(Path path, TrimPathContent trimPath)
        {
            if (trimPath == null)
            {
                return;
            }
            ApplyTrimPathIfNeeded(path, trimPath.Start.Value.Value / 100f, trimPath.End.Value.Value / 100f, trimPath.Offset.Value.Value / 360f);
        }

        internal static void ApplyTrimPathIfNeeded(Path path, float startValue, float endValue, float offsetValue)
        {
            PathMeasure.SetPath(path, false);

            var length = PathMeasure.Length;
            if (startValue == 1f && endValue == 0f)
            {
                return;
            }
            if (length == 0f || Math.Abs(endValue - startValue - 1) < .01)
            {
                return;
            }
            var start = length * startValue;
            var end = length * endValue;
            var newStart = Math.Min(start, end);
            var newEnd = Math.Max(start, end);

            var offset = offsetValue * length;
            newStart += offset;
            newEnd += offset;

            // If the trim path has rotated around the path, we need to shift it back.
            if (newStart >= length && newEnd >= length)
            {
                newStart = MiscUtils.FloorMod(newStart, length);
                newEnd = MiscUtils.FloorMod(newEnd, length);
            }

            if (newStart < 0)
            {
                newStart = MiscUtils.FloorMod(newStart, length);
            }
            if (newEnd < 0)
            {
                newEnd = MiscUtils.FloorMod(newEnd, length);
            }

            // If the start and end are equals, return an empty path.
            if (newStart == newEnd)
            {
                path.Reset();
                return;
            }

            if (newStart >= newEnd)
            {
                newStart -= length;
            }

            _tempPath.Reset();
            PathMeasure.GetSegment(newStart, newEnd, ref _tempPath, true);

            if (newEnd > length)
            {
                _tempPath2.Reset();
                PathMeasure.GetSegment(0, newEnd % length, ref _tempPath2, true);
                _tempPath.AddPath(_tempPath2);
            }
            else if (newStart < 0)
            {
                _tempPath2.Reset();
                PathMeasure.GetSegment(length + newStart, length, ref _tempPath2, true);
                _tempPath.AddPath(_tempPath2);
            }
            path.Set(_tempPath);
        }

        public static Color GetSolidColorBrush(string hex)
        {
            var index = 1; // Skip '#'
            // '#AARRGGBB'
            byte a = 255;
            if (hex.Length == 9)
            {
                a = (byte)Convert.ToUInt32(hex.Substring(index, 2), 16);
                index += 2;
            }
            var r = (byte)Convert.ToUInt32(hex.Substring(index, 2), 16);
            index += 2;
            var g = (byte)Convert.ToUInt32(hex.Substring(index, 2), 16);
            index += 2;
            var b = (byte)Convert.ToUInt32(hex.Substring(index, 2), 16);
            return Color.FromArgb(a, r, g, b);
        }
    }
}