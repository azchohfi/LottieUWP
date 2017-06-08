using System.Collections.Generic;
using System.Linq;

namespace LottieUWP
{
    internal class PathMeasure
    {
        private Path _path;
        private bool _forceClosed;
        private int _currentContourIndex;

        public PathMeasure(Path path, bool forceClosed)
        {
            _path = path;
            _forceClosed = forceClosed;
        }

        public PathMeasure()
        {
        }

        public void SetPath(Path path, bool forceClosed)
        {
            _currentContourIndex = 0;
            _path = path;
            _forceClosed = forceClosed;
        }

        public float Length
        {
            get
            {
                if (_path == null)
                    return 0;

                double sum = 0;
                var currentContour = _path.Points[_currentContourIndex];
                for (var i = 0; i < currentContour.Count - 1; i++)
                {
                    var p1 = currentContour[i];
                    var p2 = currentContour[i + 1];
                    var p3 = new PointF(p2.X - p1.X, p2.Y - p1.Y);
                    sum += p3.Length();
                }

                return (float)sum;
            }
        }

        public bool NextContour()
        {
            var result = _currentContourIndex < _path.Points.Count - 1;
            if (result)
                _currentContourIndex++;
            return result;
        }

        public void GetPosTan(float distance, out float[] pos)
        {
            var point = PathPointAtDistance(distance);

            pos = new[] { point.X, point.Y };
        }

        private PointF PathPointAtDistance(float distance)
        {
            double sum = 0;
            foreach (var points in _path.Points)
            {
                PointF pointF = null;
                int indexP2 = 0;
                if (PointAtDistance(distance, points, ref sum, ref pointF, ref indexP2))
                    return pointF;
            }

            return _path.Points.Last().Last();
        }

        private static bool PointAtDistance(float distance, List<PointF> points, ref double sum, ref PointF pointF, ref int indexP2)
        {
            for (var i = 0; i < points.Count - 1; i++)
            {
                var p1 = points[i];
                var p2 = points[i + 1];
                var d = new PointF(p2.X - p1.X, p2.Y - p1.Y).Length();
                if (sum + d >= distance)
                {
                    var p = 1 - (sum + d - distance) / d;
                    indexP2 = i + 1;
                    pointF = new PointF((float) (p1.X + (p2.X - p1.X) * p), (float) (p1.Y + (p2.Y - p1.Y) * p));
                    return true;
                }
                sum += d;
            }
            indexP2 = points.Count - 1;
            pointF = points.Last();
            return false;
        }

        public bool GetSegment(float startD, float stopD, ref Path dst, bool startWithMoveTo)
        {
            if (startD >= stopD)
            {
                return false;
            }

            var points = _path.Points[_currentContourIndex];

            PointF point1 = null;
            PointF point2 = null;
            double sum = 0;
            int indexP12 = -1;
            int indexP22 = -1;

            PointAtDistance(startD, points, ref sum, ref point1, ref indexP12);
            sum = 0;
            PointAtDistance(stopD, points, ref sum, ref point2, ref indexP22);

            if (point1 != null)
            {
                if (startWithMoveTo)
                {
                    dst.MoveTo(point1.X, point1.Y);
                }

                List<PointF> newPoints = new List<PointF>{ point1 }; // FirstPoint

                var middlePoints = points.Skip(indexP12).Take(indexP22 - indexP12).ToList();

                if(middlePoints.FirstOrDefault() == point1)
                    middlePoints.RemoveAt(0);

                if (point2 != null && middlePoints.LastOrDefault() == point2)
                    middlePoints.RemoveAt(middlePoints.Count - 1);

                newPoints.AddRange(middlePoints); // Path

                if(point2 != null)
                    newPoints.Add(point2); // LastPoint

                dst.AddPath(new Path
                {
                    Points = { newPoints }
                });
            }
            
            return true;
        }
    }
}