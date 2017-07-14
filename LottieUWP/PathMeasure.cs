using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;

namespace LottieUWP
{
    internal class PathMeasure
    {
        private CachedPathIteratorFactory _originalPathIterator;
        private Path _path;
        private CanvasGeometry _geometry;

        public PathMeasure(Path path)
        {
            _originalPathIterator = new CachedPathIteratorFactory(new FullPathIterator(path));
            _path = path;
            _geometry = _path.GetGeometry(CanvasDevice.GetSharedDevice());
            Length = _geometry.ComputePathLength();
        }

        public PathMeasure()
        {
        }

        public void SetPath(Path path)
        {
            _originalPathIterator = new CachedPathIteratorFactory(new FullPathIterator(path));
            _path = path;
            _geometry = _path.GetGeometry(CanvasDevice.GetSharedDevice());
            Length = _geometry.ComputePathLength();
        }

        public float Length { get; private set; }

        public PointF GetPosTan(float distance)
        {
            if (distance < 0)
                distance = 0;

            var length = Length;
            if (distance > length)
                distance = length;

            var vect = _geometry.ComputePointOnPath(distance);

            return new PointF(vect.X, vect.Y);
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