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
                for (var i = 0; i < points.Count - 1; i++)
                {
                    var p1 = points[i];
                    var p2 = points[i + 1];
                    var d = new PointF(p2.X - p1.X, p2.Y - p1.Y).Length();
                    if (sum + d >= distance)
                    {
                        var p = (sum + d - distance) / d;
                        return new PointF((float)(p1.X + (p2.X - p1.X) * p), (float)(p1.Y + (p2.Y - p1.Y) * p));
                    }
                    sum += d;
                }
            }

            return _path.Points[0][0];
        }

        public void GetSegment(float startD, float stopD, out Path dst, bool startWithMoveTo)
        {
            // TODO
            dst = new Path();
        }
    }
}