using Windows.UI;

namespace LottieUWP
{
    internal class RadialGradient : Gradient
    {
        private float _x0;
        private float _y0;
        private float _r;
        private Color[] _colors;
        private float[] _positions;
        private TileMode _tileMode;

        public RadialGradient(float x0, float y0, float r, Color[] colors, float[] positions, TileMode tileMode)
        {
            _x0 = x0;
            _y0 = y0;
            _r = r;
            _colors = colors;
            _positions = positions;
            _tileMode = tileMode;
        }
    }
}