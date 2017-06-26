using System.Collections.Generic;
using System.Linq;

namespace LottieUWP
{
    internal class PathMeasure
    {
        private CachedPathIteratorFactory _originalPathIterator;
        private Path _path;
        private int _currentContourIndex;

        public PathMeasure(Path path)
        {
            _originalPathIterator = new CachedPathIteratorFactory(new FullPathIterator(path));
            _path = path;
        }

        public PathMeasure()
        {
        }

        public void SetPath(Path path)
        {
            _currentContourIndex = 0;
            _originalPathIterator = new CachedPathIteratorFactory(new FullPathIterator(path));
            _path = path;
        }

        public float Length
        {
            get
            {
                if (_path?.Contours?.Count > 0)
                    return _originalPathIterator.Iterator().TotalLength;
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

            var point = _path.PathPointAtDistance(distance, out _);
            if (point != null)
            {
                pos = new[] { point.X, point.Y };
            }
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

            CachedPathIteratorFactory.CachedPathIterator iterator = _originalPathIterator.Iterator();

            float accLength = startD;
            bool isZeroLength = true;

            float[] points = new float[6];

            iterator.JumpToSegment(accLength);

            while (!iterator.Done && stopD - accLength > 0.1f)
            {
                var type = iterator.CurrentSegment(points, stopD - accLength);

                if (accLength - iterator.CurrentSegmentLength <= stopD)
                {
                    if (startWithMoveTo)
                    {
                        startWithMoveTo = false;

                        if (type != PathIterator.ContourType.MoveTo == false)
                        {
                            float[] lastPoint = new float[2];
                            iterator.GetCurrentSegmentEnd(lastPoint);
                            dst.MoveTo(lastPoint[0], lastPoint[1]);
                        }
                    }

                    isZeroLength = isZeroLength && iterator.CurrentSegmentLength > 0;
                    switch (type)
                    {
                        case PathIterator.ContourType.MoveTo:
                            dst.MoveTo(points[0], points[1]);
                            break;
                        case PathIterator.ContourType.Line:
                            dst.LineTo(points[0], points[1]);
                            break;
                        case PathIterator.ContourType.Close:
                            dst.Close();
                            break;
                        case PathIterator.ContourType.Bezier:
                        case PathIterator.ContourType.Arc:
                            dst.CubicTo(points[0], points[1],
                                points[2], points[3],
                                points[4], points[5]);
                            break;
                    }
                }

                accLength += iterator.CurrentSegmentLength;
                iterator.Next();
            }

            return !isZeroLength;
        }
    }
}