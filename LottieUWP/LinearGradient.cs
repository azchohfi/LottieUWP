using System.Numerics;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using VectorF = MathNet.Numerics.LinearAlgebra.Vector<float>;

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
            var startP = VectorF.Build.Dense(3);
            var endP = VectorF.Build.Dense(3);
            LocalMatrix.Multiply(VectorF.Build.Dense(new[] { _x0, _y0, 1f }), startP);
            LocalMatrix.Multiply(VectorF.Build.Dense(new[] { _x1, _y1, 1f }), endP);

            var canvasLinearGradientBrush = new CanvasLinearGradientBrush(device,
                _canvasGradientStopCollection, CanvasEdgeBehavior.Clamp, CanvasAlphaMode.Straight)
            {
                StartPoint = new Vector2(startP[0], startP[1]),
                EndPoint = new Vector2(endP[0], endP[1]),
                Opacity = alpha / 255f
            };

            return canvasLinearGradientBrush;
        }
    }
}