using Windows.UI.Text;
using Windows.UI.Xaml.Media;

namespace LottieUWP
{
    public class Typeface
    {
        private Typeface(FontFamily fontFamily, FontStyle style, FontWeight weight)
        {
            FontFamily = fontFamily;
            Style = style;
            Weight = weight;
        }

        public FontFamily FontFamily { get; }
        public FontStyle Style { get; }
        public FontWeight Weight { get; }

        public static Typeface Create(Typeface typeface, FontStyle style, FontWeight weight)
        {
            return new Typeface(typeface.FontFamily, style, weight);
        }

        public static Typeface CreateFromAsset(string path)
        {
            return new Typeface(new FontFamily(path), FontStyle.Normal, FontWeights.Normal);
        }
    }
}