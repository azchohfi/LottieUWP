using System;
using System.Collections.Generic;
using Windows.Foundation;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    internal class GradientFillContent : IDrawingContent
    {
        /// <summary>
        /// Cache the gradients such that it runs at 30fps.
        /// </summary>
        private const int CacheStepsMs = 32;

        private readonly Dictionary<long, LinearGradient> _linearGradientCache = new Dictionary<long, LinearGradient>();
        private readonly Dictionary<long, RadialGradient> _radialGradientCache = new Dictionary<long, RadialGradient>();
        private readonly DenseMatrix _shaderMatrix = DenseMatrix.CreateIdentity(3);
        private readonly Path _path = new Path();
        private readonly Paint _paint = new Paint(Paint.AntiAliasFlag);
        //private Rect _boundsRect;
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
            _colorAnimation.ValueChanged += OnValueChanged;
            layer.AddAnimation(_colorAnimation);

            _opacityAnimation = (KeyframeAnimation<int?>)fill.Opacity.CreateAnimation();
            _opacityAnimation.ValueChanged += OnValueChanged;
            layer.AddAnimation(_opacityAnimation);

            _startPointAnimation = (KeyframeAnimation<PointF>)fill.StartPoint.CreateAnimation();
            _startPointAnimation.ValueChanged += OnValueChanged;
            layer.AddAnimation(_startPointAnimation);

            _endPointAnimation = (KeyframeAnimation<PointF>)fill.EndPoint.CreateAnimation();
            _endPointAnimation.ValueChanged += OnValueChanged;
            layer.AddAnimation(_endPointAnimation);
        }

        private void OnValueChanged(object sender, EventArgs eventArgs)
        {
            _lottieDrawable.InvalidateSelf();
        }

        public void SetContents(IList<IContent> contentsBefore, IList<IContent> contentsAfter)
        {
            for (var i = 0; i < contentsAfter.Count; i++)
            {
                if (contentsAfter[i] is IPathContent pathContent)
                {
                    _paths.Add(pathContent);
                }
            }
        }

        public void Draw(BitmapCanvas canvas, DenseMatrix parentMatrix, byte parentAlpha)
        {
            LottieLog.BeginSection("GradientFillContent.Draw");
            _path.Reset();
            for (var i = 0; i < _paths.Count; i++)
            {
                _path.AddPath(_paths[i].Path, parentMatrix);
            }

            //_path.ComputeBounds(out _boundsRect);

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

            var alpha = (byte)(parentAlpha / 255f * _opacityAnimation.Value / 100f * 255);
            _paint.Alpha = alpha;

            canvas.DrawPath(_path, _paint);
            LottieLog.EndSection("GradientFillContent.Draw");
        }

        public void GetBounds(out Rect outBounds, DenseMatrix parentMatrix)
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

        public void AddColorFilter(string layerName, string contentName, ColorFilter colorFilter)
        {
            // Do nothing
        }

        public string Name { get; }

        private LinearGradient LinearGradient
        {
            get
            {
                var gradientHash = GradientHash;
                if (_linearGradientCache.TryGetValue(gradientHash, out LinearGradient gradient))
                {
                    return gradient;
                }
                var startPoint = _startPointAnimation.Value;
                var endPoint = _endPointAnimation.Value;
                var gradientColor = _colorAnimation.Value;
                var colors = gradientColor.Colors;
                var positions = gradientColor.Positions;
                gradient = new LinearGradient(startPoint.X, startPoint.Y, endPoint.X, endPoint.Y, colors, positions);
                _linearGradientCache.Add(gradientHash, gradient);
                return gradient;
            }
        }

        private RadialGradient RadialGradient
        {
            get
            {
                var gradientHash = GradientHash;
                if (_radialGradientCache.TryGetValue(gradientHash, out RadialGradient gradient))
                {
                    return gradient;
                }
                var startPoint = _startPointAnimation.Value;
                var endPoint = _endPointAnimation.Value;
                var gradientColor = _colorAnimation.Value;
                var colors = gradientColor.Colors;
                var positions = gradientColor.Positions;
                var x0 = startPoint.X;
                var y0 = startPoint.Y;
                var x1 = endPoint.X;
                var y1 = endPoint.Y;
                var r = (float)MathExt.Hypot(x1 - x0, y1 - y0);
                gradient = new RadialGradient(x0, y0, r, colors, positions);
                _radialGradientCache.Add(gradientHash, gradient);
                return gradient;
            }
        }

        private int GradientHash
        {
            get
            {
                var startPointProgress = (int)Math.Round(_startPointAnimation.Progress * _cacheSteps);
                var endPointProgress = (int)Math.Round(_endPointAnimation.Progress * _cacheSteps);
                var colorProgress = (int)Math.Round(_colorAnimation.Progress * _cacheSteps);
                var hash = 17;
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