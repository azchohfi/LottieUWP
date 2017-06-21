using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    internal class FillContent : IDrawingContent
    {
        private readonly Path _path = new Path();
        private readonly Paint _paint = new Paint(Paint.AntiAliasFlag);
        private readonly IList<IPathContent> _paths = new List<IPathContent>();
        private readonly IBaseKeyframeAnimation<Color> _colorAnimation;
        private readonly KeyframeAnimation<int?> _opacityAnimation;
        private readonly LottieDrawable _lottieDrawable;

        internal FillContent(LottieDrawable lottieDrawable, BaseLayer layer, ShapeFill fill)
        {
            Name = fill.Name;
            _lottieDrawable = lottieDrawable;
            if (fill.Color == null || fill.Opacity == null)
            {
                _colorAnimation = null;
                _opacityAnimation = null;
                return;
            }

            _path.FillType = fill.FillType;

            _colorAnimation = fill.Color.CreateAnimation();
            _colorAnimation.ValueChanged += (sender, args) =>
            {
                _lottieDrawable.InvalidateSelf();
            };
            layer.AddAnimation(_colorAnimation);
            _opacityAnimation = (KeyframeAnimation<int?>)fill.Opacity.CreateAnimation();
            _opacityAnimation.ValueChanged += (sender, args) =>
            {
                _lottieDrawable.InvalidateSelf();
            };
            layer.AddAnimation(_opacityAnimation);
        }

        public virtual void SetContents(IList<IContent> contentsBefore, IList<IContent> contentsAfter)
        {
            for (var i = 0; i < contentsAfter.Count; i++)
            {
                var content = contentsAfter[i];
                if (content is IPathContent pathContent)
                {
                    _paths.Add(pathContent);
                }
            }
        }

        public virtual string Name { get; }

        public virtual void AddColorFilter(string layerName, string contentName, ColorFilter colorFilter)
        {
            _paint.ColorFilter = colorFilter;
        }

        public virtual void Draw(BitmapCanvas canvas, DenseMatrix parentMatrix, byte parentAlpha)
        {
            _paint.Color = _colorAnimation.Value;
            var alpha = (byte)(parentAlpha / 255f * _opacityAnimation.Value / 100f * 255);
            _paint.Alpha = alpha;

            _path.Reset();
            for (var i = 0; i < _paths.Count; i++)
            {
                _path.AddPath(_paths[i].Path, parentMatrix);
            }

            canvas.DrawPath(_path, _paint);
        }

        public virtual void GetBounds(out Rect outBounds, DenseMatrix parentMatrix)
        {
            _path.Reset();
            for (var i = 0; i < _paths.Count; i++)
            {
                _path.AddPath(_paths[i].Path, parentMatrix);
            }
            _path.ComputeBounds(out outBounds);
            // Add padding to account for rounding errors.
            RectExt.Set(ref outBounds, outBounds.Left - 1, outBounds.Top - 1, outBounds.Right + 1, outBounds.Bottom + 1);
        }
    }
}