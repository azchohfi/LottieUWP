using System.Numerics;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using VectorF = MathNet.Numerics.LinearAlgebra.Vector<float>;

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
            for (int i = 0; i < colors.Length; i++)
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
            LocalMatrix.Multiply(VectorF.Build.Dense(new[] { _x0, _y0, 1f }), startP);

            var canvasRadialGradientBrush = new CanvasRadialGradientBrush(device,
                _canvasGradientStopCollection,
                CanvasEdgeBehavior.Clamp, CanvasAlphaMode.Straight)
            {
                Center = new Vector2(startP[0], startP[1]),
                Opacity = alpha / 255f,
                RadiusX = _r,
                RadiusY = _r
            };
            return canvasRadialGradientBrush;
        }
    }
}