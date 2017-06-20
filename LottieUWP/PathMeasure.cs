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
                if (_path?.Contours?.Count > 0)
                    return _path.Contours[_currentContourIndex].Lenght;
                return 0;
            }
        }

        public bool NextContour()
        {
            var result = _currentContourIndex < _path.Contours.Count - 1;
            if (result)
                _currentContourIndex++;
            return result;
        }

        public void GetPosTan(float distance, out float[] pos)
        {
            if (distance < 0)
                distance = 0;

            var length = Length;
            if (distance > length)
                distance = length;

            var point = PathPointAtDistance(distance);

            pos = new[] { point.X, point.Y };
        }

        private PointF PathPointAtDistance(float distance)
        {
            double sum = 0;
            foreach (var contour in _path.Contours)
            {
                PointF pointF = null;
                int indexP2 = 0;
                if (contour.PointAtDistance(distance, ref sum, ref pointF, ref indexP2))
                    return pointF;
            }

            return _path.Contours.Last().Last;
        }

        public bool GetSegment(float startD, float stopD, ref Path dst, bool startWithMoveTo)
        {
            if (startD >= stopD)
            {
                return false;
            }

            var contour = _path.Contours[_currentContourIndex];

            return contour.GetSegment(startD, stopD, ref dst, startWithMoveTo);
        }
    }
}