using Microsoft.Graphics.Canvas.Brushes;

namespace LottieUWP
{
    internal abstract class Gradient : Shader
    {
        public abstract ICanvasBrush GetBrush(byte alpha);
    }
}