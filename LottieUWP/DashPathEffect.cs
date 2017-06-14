using Windows.UI;
using Windows.UI.Xaml.Media;

namespace LottieUWP
{
    internal class DashPathEffect : PathEffect
    {
        public DoubleCollection Intervals { get; }
        public double Phase { get; }

        public DashPathEffect(double[] intervals, double phase)
        {
            Intervals = new DoubleCollection();
            for (var i = 0; i < intervals.Length; i++)
            {
                Intervals.Add(intervals[i]);
            }
            Phase = phase;
            Color = Colors.Black;
        }
    }
}