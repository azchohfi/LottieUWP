using Windows.UI;

namespace LottieUWP
{
    public abstract class PathEffect
    {
        public Color Color { get; protected set; }

        public Color GetColor(Paint paint)
        {
            var color = Color;
            color.A = (byte) (color.A * paint.Alpha / 255);
            return color;
        }
    }
}