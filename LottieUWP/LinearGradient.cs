using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace LottieUWP
{
    internal class LinearGradient : Gradient
    {
        private readonly float _x0;
        private readonly float _y0;
        private readonly float _x1;
        private readonly float _y1;
        private readonly LinearGradientBrush _linearGradientBrush;

        public LinearGradient(float x0, float y0, float x1, float y1, Color[] colors, float[] positions)
        {
            _x0 = x0;
            _y0 = y0;
            _x1 = x1;
            _y1 = y1;
            var gradientStopCollection = new GradientStopCollection();
            for (int i = 0; i < colors.Length; i++)
            {
                gradientStopCollection.Add(new GradientStop
                {
                    Color = colors[i],
                    Offset = positions[i]
                });
            }

            _linearGradientBrush = new LinearGradientBrush
            {
                GradientStops = gradientStopCollection,
                SpreadMethod = GradientSpreadMethod.Pad,
                MappingMode = BrushMappingMode.Absolute
            };
            
        }

        public override Brush GetBrush(byte alpha)
        {
            var matrixTransform = GetCurrentRenderTransform();

            var transformedZero = matrixTransform.TransformPoint(new Point(0, 0));

            var startPoint = new Point(transformedZero.X / 2 + _x0, transformedZero.Y / 2 + _y0);
            var endPoint = new Point(transformedZero.X / 2 + _x1, transformedZero.Y / 2 + _y1);

            _linearGradientBrush.StartPoint = startPoint;
            _linearGradientBrush.EndPoint = endPoint;
            _linearGradientBrush.Opacity = alpha / 255f;
            return _linearGradientBrush;
        }
    }
}