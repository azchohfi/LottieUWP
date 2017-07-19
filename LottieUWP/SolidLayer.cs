using Windows.Foundation;

namespace LottieUWP
{
    internal class SolidLayer : BaseLayer
    {
        private readonly Paint _paint = new Paint();

        internal SolidLayer(LottieDrawable lottieDrawable, Layer layerModel) : base(lottieDrawable, layerModel)
        {
            LayerModel = layerModel;

            _paint.Alpha = 0;
            _paint.Style = Paint.PaintStyle.Fill;
            _paint.Color = layerModel.SolidColor;
        }

        public override void DrawLayer(BitmapCanvas canvas, Matrix3X3 parentMatrix, byte parentAlpha)
        {
            int backgroundAlpha = LayerModel.SolidColor.A;
            if (backgroundAlpha == 0)
            {
                return;
            }

            var alpha = (byte)(backgroundAlpha / 255f * Transform.Opacity.Value / 100f * 255);
            _paint.Alpha = alpha;
            if (alpha > 0)
            {
                UpdateRect(parentMatrix);
                canvas.DrawRect(Rect, _paint);
            }
        }

        public override void GetBounds(out Rect outBounds, Matrix3X3 parentMatrix)
        {
            base.GetBounds(out outBounds, parentMatrix);
            UpdateRect(BoundsMatrix);
            RectExt.Set(ref outBounds, Rect);
        }

        private void UpdateRect(Matrix3X3 matrix)
        {
            RectExt.Set(ref Rect, 0, 0, LayerModel.SolidWidth, LayerModel.SolidHeight);
            matrix.MapRect(ref Rect);
        }

        public override void AddColorFilter(string layerName, string contentName, ColorFilter colorFilter)
        {
            _paint.ColorFilter = colorFilter;
        }
    }
}