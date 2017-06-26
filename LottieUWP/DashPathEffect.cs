using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace LottieUWP
{
    internal class DashPathEffect : PathEffect
    {
        private readonly DoubleCollection _intervals;
        private readonly double _phase;

        public DashPathEffect(double[] intervals, double phase)
        {
            _intervals = new DoubleCollection();
            for (var i = 0; i < intervals.Length; i++)
            {
                _intervals.Add(intervals[i]);
            }
            _phase = phase;
        }

        public override void Apply(Shape shape, Paint paint)
        {
            if (paint.Style == Paint.PaintStyle.Stroke)
            {
                shape.StrokeDashArray = _intervals;
                shape.StrokeDashOffset = _phase;
            }
        }
    }
}