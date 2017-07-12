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
        private readonly CanvasRadialGradientBrush _canvasRadialGradientBrush;

        public RadialGradient(float x0, float y0, float r, Color[] colors, float[] positions)
        {
            _x0 = x0;
            _y0 = y0;
            _r = r;
            var canvasGradientStopCollection = new CanvasGradientStop[colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                canvasGradientStopCollection[i] = new CanvasGradientStop
                {
                    Color = colors[i],
                    Position = positions[i]
                };
            }

            _canvasRadialGradientBrush = new CanvasRadialGradientBrush(CanvasDevice.GetSharedDevice(),
                canvasGradientStopCollection,
                CanvasEdgeBehavior.Clamp, CanvasAlphaMode.Straight);
        }

        public override ICanvasBrush GetBrush(byte alpha)
        {
            var startP = VectorF.Build.Dense(3);
            LocalMatrix.Multiply(VectorF.Build.Dense(new[] { _x0, _y0, 1f }), startP);

            _canvasRadialGradientBrush.Center = new Vector2(startP[0], startP[1]);
            _canvasRadialGradientBrush.Opacity = alpha / 255f;
            _canvasRadialGradientBrush.RadiusX = _r;
            _canvasRadialGradientBrush.RadiusY = _r;
            return _canvasRadialGradientBrush;
        }
    }
}