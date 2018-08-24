using Windows.UI;
using LottieUWP.Utils;

namespace LottieUWP.Model.Content
{
    public class GradientColor
    {
        private readonly float[] _positions;
        private readonly Color[] _colors;

        internal GradientColor(float[] positions, Color[] colors)
        {
            _positions = positions;
            _colors = colors;
        }

        internal float[] Positions => _positions;

        internal Color[] Colors => _colors;

        internal int Size => _colors.Length;

        internal void Lerp(GradientColor gc1, GradientColor gc2, float progress)
        {
            if (gc1._colors.Length != gc2._colors.Length)
            {
                throw new System.ArgumentException("Cannot interpolate between gradients. Lengths vary (" + gc1._colors.Length + " vs " + gc2._colors.Length + ")");
            }

            for (var i = 0; i < gc1._colors.Length; i++)
            {
                _positions[i] = MiscUtils.Lerp(gc1._positions[i], gc2._positions[i], progress);

                var gamma = GammaEvaluator.Evaluate(progress, gc1._colors[i], gc2._colors[i]);
                
                _colors[i] = gamma;
            }
        }
    }
}