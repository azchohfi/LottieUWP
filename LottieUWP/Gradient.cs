using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;

namespace LottieUWP
{
    internal abstract class Gradient : Shader
    {
        public abstract ICanvasBrush GetBrush(CanvasDevice device, byte alpha);
    }
}