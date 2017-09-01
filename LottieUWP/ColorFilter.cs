using LottieUWP.Animation.Content;
using Microsoft.Graphics.Canvas.Brushes;

namespace LottieUWP
{
    public abstract class ColorFilter
    {
        public abstract ICanvasBrush Apply(BitmapCanvas dst, ICanvasBrush brush);
    }
}