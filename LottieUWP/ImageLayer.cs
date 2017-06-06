using System;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    internal class ImageLayer : BaseLayer
    {
        private readonly Paint _paint = new Paint(Paint.AntiAliasFlag | Paint.FilterBitmapFlag);
        private Rect _src;
        private Rect _dst;
        private readonly float _density;

        internal ImageLayer(LottieDrawable lottieDrawable, Layer layerModel, float density) : base(lottieDrawable, layerModel)
        {
            _density = density;
        }

        public override void DrawLayer(BitmapCanvas canvas, DenseMatrix parentMatrix, int parentAlpha)
        {
            var bitmap = Bitmap;
            if (bitmap == null)
            {
                return;
            }
            _paint.Alpha = parentAlpha;
            canvas.Save();
            canvas.Concat(parentMatrix);
            RectExt.Set(ref _src, 0, 0, bitmap.PixelWidth, bitmap.PixelHeight);
            RectExt.Set(ref _dst, 0, 0, (int)(bitmap.PixelWidth * _density), (int)(bitmap.PixelHeight * _density));
            canvas.DrawBitmap(bitmap, _src, _dst, _paint);
            canvas.Restore();
        }

        public override void GetBounds(out Rect outBounds, DenseMatrix parentMatrix)
        {
            base.GetBounds(out outBounds, parentMatrix);
            var bitmap = Bitmap;
            if (bitmap != null)
            {
                RectExt.Set(ref outBounds, outBounds.Left, outBounds.Top, Math.Min(outBounds.Right, bitmap.PixelWidth), Math.Min(outBounds.Bottom, bitmap.PixelHeight));
                BoundsMatrix.MapRect(ref outBounds);
            }
        }

        private WriteableBitmap Bitmap
        {
            get
            {
                var refId = _layerModel.RefId;
                return LottieDrawable.GetImageAsset(refId);
            }
        }

        public override void AddColorFilter(string layerName, string contentName, ColorFilter colorFilter)
        {
            _paint.ColorFilter = colorFilter;
        }
    }
}