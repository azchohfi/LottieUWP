using System;
using System.Collections.Generic;
using Windows.Foundation;
using MathNet.Numerics.LinearAlgebra.Single;
using Color = Windows.UI.Color;

namespace LottieUWP
{
    internal class GradientFillContent : IDrawingContent, BaseKeyframeAnimation.IAnimationListener
    {
        /// <summary>
        /// Cache the gradients such that it runs at 30fps.
        /// </summary>
        private const int CacheStepsMs = 32;

        private readonly Dictionary<long, LinearGradient> _linearGradientCache = new Dictionary<long, LinearGradient>();
        private readonly Dictionary<long, RadialGradient> _radialGradientCache = new Dictionary<long, RadialGradient>();
        private readonly DenseMatrix _shaderMatrix = new DenseMatrix(3, 3);
        private readonly Path _path = new Path();
        private readonly Paint _paint = new Paint(Paint.AntiAliasFlag);
        private Rect _boundsRect;
        private readonly IList<IPathContent> _paths = new List<IPathContent>();
        private readonly GradientType _type;
        private readonly KeyframeAnimation<GradientColor> _colorAnimation;
        private readonly KeyframeAnimation<int?> _opacityAnimation;
        private readonly KeyframeAnimation<PointF> _startPointAnimation;
        private readonly KeyframeAnimation<PointF> _endPointAnimation;
        private readonly LottieDrawable _lottieDrawable;
        private readonly int _cacheSteps;

        internal GradientFillContent(LottieDrawable lottieDrawable, BaseLayer layer, GradientFill fill)
        {
            Name = fill.Name;
            _lottieDrawable = lottieDrawable;
            _type = fill.GradientType;
            _path.FillType = fill.FillType;
            _cacheSteps = (int)(lottieDrawable.Composition.Duration / CacheStepsMs);

            _colorAnimation = (KeyframeAnimation<GradientColor>)fill.GradientColor.CreateAnimation();
            _colorAnimation.AddUpdateListener(this);
            layer.AddAnimation(_colorAnimation);

            _opacityAnimation = (KeyframeAnimation<int?>)fill.Opacity.CreateAnimation();
            _opacityAnimation.AddUpdateListener(this);
            layer.AddAnimation(_opacityAnimation);

            _startPointAnimation = (KeyframeAnimation<PointF>)fill.StartPoint.CreateAnimation();
            _startPointAnimation.AddUpdateListener(this);
            layer.AddAnimation(_startPointAnimation);

            _endPointAnimation = (KeyframeAnimation<PointF>)fill.EndPoint.CreateAnimation();
            _endPointAnimation.AddUpdateListener(this);
            layer.AddAnimation(_endPointAnimation);
        }

        public void OnValueChanged()
        {
            _lottieDrawable.InvalidateSelf();
        }

        public void SetContents(IList<IContent> contentsBefore, IList<IContent> contentsAfter)
        {
            for (int i = 0; i < contentsAfter.Count; i++)
            {
                var pathContent = contentsAfter[i] as IPathContent;
                if (pathContent != null)
                {
                    _paths.Add(pathContent);
                }
            }
        }

        public void Draw(BitmapCanvas canvas, DenseMatrix parentMatrix, int parentAlpha)
        {
            _path.Reset();
            for (int i = 0; i < _paths.Count; i++)
            {
                _path.AddPath(_paths[i].Path, parentMatrix);
            }

            _path.ComputeBounds(out _boundsRect, false);

            Shader shader;
            if (_type == GradientType.Linear)
            {
                shader = LinearGradient;
            }
            else
            {
                shader = RadialGradient;
            }
            _shaderMatrix.Set(parentMatrix);
            shader.LocalMatrix = _shaderMatrix;
            _paint.Shader = shader;

            int alpha = (int)(parentAlpha / 255f * _opacityAnimation.Value / 100f * 255);
            _paint.Alpha = alpha;

            canvas.DrawPath(_path, _paint);
        }

        public void GetBounds(out Rect outBounds, DenseMatrix parentMatrix)
        {
            _path.Reset();
            for (int i = 0; i < _paths.Count; i++)
            {
                _path.AddPath(_paths[i].Path, parentMatrix);
            }

            _path.ComputeBounds(out outBounds, false);
            // Add padding to account for rounding errors.
            RectExt.Set(ref outBounds, outBounds.Left - 1, outBounds.Top - 1, outBounds.Right + 1, outBounds.Bottom + 1);
        }

        public void AddColorFilter(string layerName, string contentName, ColorFilter colorFilter)
        {
            // Do nothing
        }

        public string Name { get; }

        private LinearGradient LinearGradient
        {
            get
            {
                int gradientHash = GradientHash;
                LinearGradient gradient;
                if (_linearGradientCache.TryGetValue(gradientHash, out gradient))
                {
                    return gradient;
                }
                PointF startPoint = _startPointAnimation.Value;
                PointF endPoint = _endPointAnimation.Value;
                GradientColor gradientColor = _colorAnimation.Value;
                Color[] colors = gradientColor.Colors;
                float[] positions = gradientColor.Positions;
                gradient = new LinearGradient(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, colors, positions, Shader.TileMode.Clamp);
                _linearGradientCache.Add(gradientHash, gradient);
                return gradient;
            }
        }

        private RadialGradient RadialGradient
        {
            get
            {
                int gradientHash = GradientHash;
                RadialGradient gradient;
                if (_radialGradientCache.TryGetValue(gradientHash, out gradient))
                {
                    return gradient;
                }
                PointF startPoint = _startPointAnimation.Value;
                PointF endPoint = _endPointAnimation.Value;
                GradientColor gradientColor = _colorAnimation.Value;
                Color[] colors = gradientColor.Colors;
                float[] positions = gradientColor.Positions;
                float x0 = startPoint.X;
                float y0 = startPoint.Y;
                float x1 = endPoint.X;
                float y1 = endPoint.Y;
                float r = (float)MathExt.Hypot(x1 - x0, y1 - y0);
                gradient = new RadialGradient(x0, y0, r, colors, positions, Shader.TileMode.Clamp);
                _radialGradientCache.Add(gradientHash, gradient);
                return gradient;
            }
        }

        private int GradientHash
        {
            get
            {
                int startPointProgress = (int)Math.Round(_startPointAnimation.Progress * _cacheSteps);
                int endPointProgress = (int)Math.Round(_endPointAnimation.Progress * _cacheSteps);
                int colorProgress = (int)Math.Round(_colorAnimation.Progress * _cacheSteps);
                int hash = 17;
                if (startPointProgress != 0)
                    hash = hash * 31 * startPointProgress;
                if (endPointProgress != 0)
                    hash = hash * 31 * endPointProgress;
                if (colorProgress != 0)
                    hash = hash * 31 * colorProgress;
                return hash;
            }
        }
    }
}