using System.Collections.Generic;
using System.Linq;

namespace LottieUWP
{
    internal class PathMeasure
    {
        private Path _path;
        private int _currentContourIndex;

        public PathMeasure(Path path)
        {
            _path = path;
        }

        public PathMeasure()
        {
        }

        public void SetPath(Path path)
        {
            _currentContourIndex = 0;
            _path = path;
        }

        public float Length
        {
            get
            {
                if (_path?.Contours?.Count > 0)
                    return UsefulContours()[_currentContourIndex].Lenght;
                return 0;
            }
        }

        private List<Path.IContour> UsefulContours()
        {
            return _path.Contours.Where(c =>
                c is Path.LineContour ||
                c is Path.ArcContour ||
                c is Path.BezierContour
            ).ToList();
        }

        public bool NextContour()
        {
            var result = _currentContourIndex < UsefulContours().Count - 1;
            if (result)
                _currentContourIndex++;
            return result;
        }

        public void GetPosTan(float distance, ref float[] pos)
        {
            if (distance < 0)
                distance = 0;

            var length = Length;
            if (distance > length)
                distance = length;

            var point = PathPointAtDistance(distance, out _);
            if (point != null)
            {
                pos = new[] { point.X, point.Y };
            }
        }

        private PointF PathPointAtDistance(float distance, out Path.IContour contourAtDistance)
        {
            double sum = 0;
            foreach (var contour in _path.Contours)
            {
                if (contour.PointAtDistance(distance, ref sum, out var pointF))
                {
                    contourAtDistance = contour;
                    return pointF;
                }
            }

            contourAtDistance = null;
            return null;
        }

        public bool GetSegment(float startD, float stopD, ref Path dst, bool startWithMoveTo)
        {
            float length = Length;

            if (startD < 0)
            {
                startD = 0;
            }

            if (stopD > length)
            {
                stopD = length;
            }

            if (startD >= stopD)
            {
                return false;
            }

            float accLength = startD;
            bool isZeroLength = true;

            PathPointAtDistance(accLength, out var c);

            int index = _path.Contours.IndexOf(c);

            var e = _path.Contours.Skip(index).GetEnumerator();

            while (e.MoveNext() && stopD - accLength > 0.1f)
            {
                var point = PathPointAtDistance(stopD - accLength, out var contour);

                if (accLength - contour.Lenght <= stopD)
                {
                    if (startWithMoveTo)
                    {
                        startWithMoveTo = false;

                        if (contour is Path.MoveToContour == false)
                        {
                            dst.MoveTo(contour.Last.X, contour.Last.Y);
                        }
                    }

                    isZeroLength = isZeroLength && contour.Lenght > 0;
                    switch (contour)
                    {
                        case Path.MoveToContour _:
                            dst.MoveTo(point.X, point.Y);
                            break;
                        case Path.LineContour _:
                            dst.LineTo(point.X, point.Y);
                            break;
                        case Path.CloseContour _:
                            dst.Close();
                            break;
                        case Path.BezierContour bezier:
                            dst.CubicTo(bezier.Control1.X, bezier.Control1.Y,
                                bezier.Control2.X, bezier.Control2.Y,
                                bezier.Vertex.X, bezier.Vertex.Y);
                            break;
                    }
                }

                accLength += contour.Lenght;

                PathPointAtDistance(stopD - accLength, out contour);
            }

            e.Dispose();

            return !isZeroLength;
        }
    }
}