using Windows.UI;

namespace LottieUWP
{
    /// <summary>
    /// A color filter with a predefined transfer mode that applies the specified color on top of the
    /// original color. As there are many other transfer modes, please take a look at the definition
    /// of PorterDuff.Mode.SRC_ATOP to find one that suits your needs.
    /// This site has a great explanation of Porter/Duff compositing algebra as well as a visual
    /// representation of many of the transfer modes:
    /// http://ssp.impulsetrain.com/porterduff.html
    /// </summary>
    public class SimpleColorFilter : PorterDuffColorFilter
    {
        public SimpleColorFilter(Color color) : base(color, PorterDuff.Mode.SrcAtop)
        {
        }
    }

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