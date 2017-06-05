using System;
using System.Collections.Generic;
using Windows.Foundation;
using MathNet.Numerics.LinearAlgebra.Single;
using Color = Windows.UI.Color;

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

        internal GradientStrokeContent(LottieDrawable lottieDrawable, BaseLayer layer, GradientStroke stroke) : base(lottieDrawable, layer, stroke.CapType.ToPaintCap(), stroke.JoinType.ToPaintJoin(), stroke.Opacity, stroke.Width, stroke.LineDashPattern, stroke.DashOffset)
        {
            Name = stroke.Name;
            _type = stroke.GradientType;
            _cacheSteps = (int)(lottieDrawable.Composition.Duration / CacheStepsMs);

            _colorAnimation = (KeyframeAnimation<GradientColor>)stroke.GradientColor.CreateAnimation();
            _colorAnimation.AddUpdateListener(this);
            layer.AddAnimation(_colorAnimation);

            _startPointAnimation = (KeyframeAnimation<PointF>)stroke.StartPoint.CreateAnimation();
            _startPointAnimation.AddUpdateListener(this);
            layer.AddAnimation(_startPointAnimation);

            _endPointAnimation = (KeyframeAnimation<PointF>)stroke.EndPoint.CreateAnimation();
            _endPointAnimation.AddUpdateListener(this);
            layer.AddAnimation(_endPointAnimation);
        }

        public override void Draw(BitmapCanvas canvas, DenseMatrix parentMatrix, int parentAlpha)
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
                int x0 = (int)(_boundsRect.Left + _boundsRect.Width / 2 + startPoint.X);
                int y0 = (int)(_boundsRect.Top + _boundsRect.Height / 2 + startPoint.Y);
                int x1 = (int)(_boundsRect.Left + _boundsRect.Width / 2 + endPoint.X);
                int y1 = (int)(_boundsRect.Top + _boundsRect.Height / 2 + endPoint.Y);
                gradient = new LinearGradient(x0, y0, x1, y1, colors, positions, Shader.TileMode.Clamp);
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
                int x0 = (int)(_boundsRect.Left + _boundsRect.Width / 2 + startPoint.X);
                int y0 = (int)(_boundsRect.Top + _boundsRect.Height / 2 + startPoint.Y);
                int x1 = (int)(_boundsRect.Left + _boundsRect.Width / 2 + endPoint.X);
                int y1 = (int)(_boundsRect.Top + _boundsRect.Height / 2 + endPoint.Y);
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