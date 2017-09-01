using LottieUWP.Animation.Content;
using Microsoft.Graphics.Canvas.Geometry;

namespace LottieUWP
{
    public abstract class PathEffect
    {
        public abstract void Apply(CanvasStrokeStyle canvasStrokeStyle, Paint paint);
    }
}