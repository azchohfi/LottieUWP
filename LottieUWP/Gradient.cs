using Windows.UI.Xaml.Media;

namespace LottieUWP
{
    internal abstract class Gradient : Shader
    {
        public abstract Brush GetBrush(byte alpha);
    }
}