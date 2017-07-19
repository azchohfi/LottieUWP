using System.Numerics;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;

namespace LottieUWP
{
    internal class RadialGradient : Gradient
    {
        private readonly float _x0;
        private readonly float _y0;
        private readonly float _r;
        private readonly CanvasGradientStop[] _canvasGradientStopCollection;

        public RadialGradient(float x0, float y0, float r, Color[] colors, float[] positions)
        {
            _x0 = x0;
            _y0 = y0;
            _r = r;
            _canvasGradientStopCollection = new CanvasGradientStop[colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                _canvasGradientStopCollection[i] = new CanvasGradientStop
                {
                    Color = colors[i],
                    Position = positions[i]
                };
            }
        }

        public override ICanvasBrush GetBrush(CanvasDevice device, byte alpha)
        {
            var center = new Vector2(_x0, _y0);
            center = LocalMatrix.Transform(center);

            var canvasRadialGradientBrush = new CanvasRadialGradientBrush(device,
                _canvasGradientStopCollection,
                CanvasEdgeBehavior.Clamp, CanvasAlphaMode.Straight)
            {
                Center = center,
                Opacity = alpha / 255f,
                RadiusX = _r,
                RadiusY = _r
            };
            return canvasRadialGradientBrush;
        }
    }
}