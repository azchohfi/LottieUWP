using System.Numerics;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;

namespace LottieUWP
{
    internal class LinearGradient : Gradient
    {
        private readonly float _x0;
        private readonly float _y0;
        private readonly float _x1;
        private readonly float _y1;
        private readonly CanvasGradientStop[] _canvasGradientStopCollection;

        public LinearGradient(float x0, float y0, float x1, float y1, Color[] colors, float[] positions)
        {
            _x0 = x0;
            _y0 = y0;
            _x1 = x1;
            _y1 = y1;
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
            var startPoint = new Vector2(_x0, _y0);
            var endPoint = new Vector2(_x1, _y1);

            startPoint = LocalMatrix.Transform(startPoint);
            endPoint = LocalMatrix.Transform(endPoint);

            var canvasLinearGradientBrush = new CanvasLinearGradientBrush(device,
                _canvasGradientStopCollection, CanvasEdgeBehavior.Clamp, CanvasAlphaMode.Straight)
            {
                StartPoint = startPoint,
                EndPoint = endPoint,
                Opacity = alpha / 255f
            };

            return canvasLinearGradientBrush;
        }
    }
}