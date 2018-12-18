using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using LottieUWP.Animation.Keyframe;
using LottieUWP.Model;
using LottieUWP.Model.Content;
using LottieUWP.Model.Layer;
using LottieUWP.Utils;
using LottieUWP.Value;

namespace LottieUWP.Animation.Content
{
    internal class FillContent : IDrawingContent, IKeyPathElementContent
    {
        private readonly Path _path = new Path();
        private readonly Paint _paint = new Paint(Paint.AntiAliasFlag);
        private readonly BaseLayer _layer;
        private readonly bool _hidden;
        private readonly List<IPathContent> _paths = new List<IPathContent>();
        private readonly IBaseKeyframeAnimation<Color?, Color?> _colorAnimation;
        private readonly IBaseKeyframeAnimation<int?, int?> _opacityAnimation;
        private IBaseKeyframeAnimation<ColorFilter, ColorFilter> _colorFilterAnimation;
        private readonly ILottieDrawable _lottieDrawable;

        internal FillContent(ILottieDrawable lottieDrawable, BaseLayer layer, ShapeFill fill)
        {
            _layer = layer;
            Name = fill.Name;
            _hidden = fill.IsHidden;
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
            _opacityAnimation = fill.Opacity.CreateAnimation();
            _opacityAnimation.ValueChanged += (sender, args) =>
            {
                _lottieDrawable.InvalidateSelf();
            };
            layer.AddAnimation(_opacityAnimation);
        }

        public void SetContents(List<IContent> contentsBefore, List<IContent> contentsAfter)
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

        public string Name { get; }

        public void Draw(BitmapCanvas canvas, Matrix3X3 parentMatrix, byte parentAlpha)
        {
            if (_hidden)
            {
                return;
            }
            LottieLog.BeginSection("FillContent.Draw");
            _paint.Color = _colorAnimation.Value ?? Colors.White;
            var alpha = (byte)(parentAlpha / 255f * _opacityAnimation.Value / 100f * 255);
            _paint.Alpha = alpha;

            if (_colorFilterAnimation != null)
            {
                _paint.ColorFilter = _colorFilterAnimation.Value;
            }

            _path.Reset();
            for (var i = 0; i < _paths.Count; i++)
            {
                _path.AddPath(_paths[i].Path, parentMatrix);
            }

            canvas.DrawPath(_path, _paint);

            LottieLog.EndSection("FillContent.Draw");
        }

        public void GetBounds(out Rect outBounds, Matrix3X3 parentMatrix)
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

        public void ResolveKeyPath(KeyPath keyPath, int depth, List<KeyPath> accumulator, KeyPath currentPartialKeyPath)
        {
            MiscUtils.ResolveKeyPath(keyPath, depth, accumulator, currentPartialKeyPath, this);
        }

        public void AddValueCallback<T>(LottieProperty property, ILottieValueCallback<T> callback)
        {
            if (property == LottieProperty.Color)
            {
                _colorAnimation.SetValueCallback((ILottieValueCallback<Color?>)callback);
            }
            else if (property == LottieProperty.Opacity)
            {
                _opacityAnimation.SetValueCallback((ILottieValueCallback<int?>)callback);
            }
            else if (property == LottieProperty.ColorFilter)
            {
                if (callback == null)
                {
                    _colorFilterAnimation = null;
                }
                else
                {
                    _colorFilterAnimation = new ValueCallbackKeyframeAnimation<ColorFilter, ColorFilter>((ILottieValueCallback<ColorFilter>)callback);
                    _colorFilterAnimation.ValueChanged += (sender, args) =>
                    {
                        _lottieDrawable.InvalidateSelf();
                    };
                    _layer.AddAnimation(_colorFilterAnimation);
                }
            }
        }
    }
}