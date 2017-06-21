using System;
using System.Collections.Generic;
using Windows.Foundation;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    public class GradientStrokeContent : BaseStrokeContent
    {
        /// <summary>
        /// Cache the gradients such that it runs at 30fps.
        /// </summary>
        private const int CacheStepsMs = 32;

        private readonly Dictionary<long, LinearGradient> _linearGradientCache = new Dictionary<long, LinearGradient>();
        private readonly Dictionary<long, RadialGradient> _radialGradientCache = new Dictionary<long, RadialGradient>();
        private Rect _boundsRect;

        private readonly GradientType _type;
        private readonly int _cacheSteps;
        private readonly KeyframeAnimation<GradientColor> _colorAnimation;
        private readonly KeyframeAnimation<PointF> _startPointAnimation;
        private readonly KeyframeAnimation<PointF> _endPointAnimation;

        internal GradientStrokeContent(LottieDrawable lottieDrawable, BaseLayer layer, GradientStroke stroke) : base(lottieDrawable, layer, ShapeStroke.LineCapTypeToPaintCap(stroke.CapType), ShapeStroke.LineJoinTypeToPaintLineJoin(stroke.JoinType), stroke.Opacity, stroke.Width, stroke.LineDashPattern, stroke.DashOffset)
        {
            Name = stroke.Name;
            _type = stroke.GradientType;
            _cacheSteps = (int)(lottieDrawable.Composition.Duration / CacheStepsMs);

            _colorAnimation = (KeyframeAnimation<GradientColor>)stroke.GradientColor.CreateAnimation();
            _colorAnimation.ValueChanged += OnValueChanged;
            layer.AddAnimation(_colorAnimation);

            _startPointAnimation = (KeyframeAnimation<PointF>)stroke.StartPoint.CreateAnimation();
            _startPointAnimation.ValueChanged += OnValueChanged;
            layer.AddAnimation(_startPointAnimation);

            _endPointAnimation = (KeyframeAnimation<PointF>)stroke.EndPoint.CreateAnimation();
            _endPointAnimation.ValueChanged += OnValueChanged;
            layer.AddAnimation(_endPointAnimation);
        }

        public override void Draw(BitmapCanvas canvas, DenseMatrix parentMatrix, byte parentAlpha)
        {
            GetBounds(out _boundsRect, parentMatrix);
            if (_type == GradientType.Linear)
            {
                Paint.Shader = LinearGradient;
            }
            else
            {
                Paint.Shader = RadialGradient;
            }

            base.Draw(canvas, parentMatrix, parentAlpha);
        }

        public override void AddColorFilter(string layerName, string contentName, ColorFilter colorFilter)
        {
            //Do nothing
        }

        public override string Name { get; }

        private LinearGradient LinearGradient
        {
            get
            {
                var gradientHash = GradientHash;
                LinearGradient gradient;
                if (_linearGradientCache.TryGetValue(gradientHash, out gradient))
                {
                    return gradient;
                }
                var startPoint = _startPointAnimation.Value;
                var endPoint = _endPointAnimation.Value;
                var gradientColor = _colorAnimation.Value;
                var colors = gradientColor.Colors;
                var positions = gradientColor.Positions;
                var x0 = (int)(_boundsRect.Left + _boundsRect.Width / 2 + startPoint.X);
                var y0 = (int)(_boundsRect.Top + _boundsRect.Height / 2 + startPoint.Y);
                var x1 = (int)(_boundsRect.Left + _boundsRect.Width / 2 + endPoint.X);
                var y1 = (int)(_boundsRect.Top + _boundsRect.Height / 2 + endPoint.Y);
                gradient = new LinearGradient(x0, y0, x1, y1, colors, positions);
                _linearGradientCache.Add(gradientHash, gradient);
                return gradient;
            }
        }

        private RadialGradient RadialGradient
        {
            get
            {
                var gradientHash = GradientHash;
                RadialGradient gradient;
                if (_radialGradientCache.TryGetValue(gradientHash, out gradient))
                {
                    return gradient;
                }
                var startPoint = _startPointAnimation.Value;
                var endPoint = _endPointAnimation.Value;
                var gradientColor = _colorAnimation.Value;
                var colors = gradientColor.Colors;
                var positions = gradientColor.Positions;
                var x0 = (int)(_boundsRect.Left + _boundsRect.Width / 2 + startPoint.X);
                var y0 = (int)(_boundsRect.Top + _boundsRect.Height / 2 + startPoint.Y);
                var x1 = (int)(_boundsRect.Left + _boundsRect.Width / 2 + endPoint.X);
                var y1 = (int)(_boundsRect.Top + _boundsRect.Height / 2 + endPoint.Y);
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