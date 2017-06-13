using Windows.UI;

namespace LottieUWP
{
    internal class DashPathEffect : PathEffect
    {
        public float[] Intervals { get; }
        public float Phase { get; }

        public DashPathEffect(float[] intervals, float phase)
        {
            Intervals = intervals;
            Phase = phase;
            Color = Colors.Black;
        }
    }
}