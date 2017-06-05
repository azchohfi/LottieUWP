namespace LottieUWP
{
    internal class CubicCurveData
    {
        private readonly PointF _controlPoint1;
        private readonly PointF _controlPoint2;
        private readonly PointF _vertex;

        internal CubicCurveData()
        {
            _controlPoint1 = new PointF();
            _controlPoint2 = new PointF();
            _vertex = new PointF();
        }

        internal CubicCurveData(PointF controlPoint1, PointF controlPoint2, PointF vertex)
        {
            _controlPoint1 = controlPoint1;
            _controlPoint2 = controlPoint2;
            _vertex = vertex;
        }

        internal virtual void SetControlPoint1(float x, float y)
        {
            _controlPoint1.X = x;
            _controlPoint1.Y = y;
        }

        internal virtual PointF ControlPoint1 => _controlPoint1;

        internal virtual void SetControlPoint2(float x, float y)
        {
            _controlPoint2.X = x;
            _controlPoint2.Y = y;
        }

        internal virtual PointF ControlPoint2 => _controlPoint2;

        internal virtual void SetVertex(float x, float y)
        {
            _vertex.X = x;
            _vertex.Y = y;
        }

        internal virtual PointF Vertex => _vertex;
    }
}