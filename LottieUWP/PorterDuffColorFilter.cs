using Windows.UI;

namespace LottieUWP
{
    public abstract class PorterDuffColorFilter : ColorFilter
    {
        private Color _color;
        private readonly PorterDuff.Mode _mode;

        protected PorterDuffColorFilter(Color color, PorterDuff.Mode mode)
        {
            _color = color;
            _mode = mode;
        }

        public override Color Apply(Color color)
        {
            switch (_mode)
            {
                case PorterDuff.Mode.SrcAtop:
                    return color; // Todo
            }
            return color;
        }
    }
}