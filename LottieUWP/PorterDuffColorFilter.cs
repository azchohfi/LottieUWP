using Windows.UI;

namespace LottieUWP
{
    public abstract class PorterDuffColorFilter : ColorFilter
    {
        private Color _color;
        private PorterDuff.Mode _mode;

        protected PorterDuffColorFilter(Color color, PorterDuff.Mode mode)
        {
            _color = color;
            _mode = mode;
        }
    }
}