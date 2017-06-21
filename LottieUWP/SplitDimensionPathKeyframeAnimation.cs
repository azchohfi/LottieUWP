using System.Collections.Generic;

namespace LottieUWP
{
    internal class SplitDimensionPathKeyframeAnimation : KeyframeAnimation<PointF>
    {
        private readonly PointF _point = new PointF();
        private readonly KeyframeAnimation<float?> _xAnimation;
        private readonly KeyframeAnimation<float?> _yAnimation;

        internal SplitDimensionPathKeyframeAnimation(KeyframeAnimation<float?> xAnimation, KeyframeAnimation<float?> yAnimation)
            : base(new List<IKeyframe<PointF>>())
        {
            _xAnimation = xAnimation;
            _yAnimation = yAnimation;
        }

        public override float Progress
        {
            set
            {
                _xAnimation.Progress = value;
                _yAnimation.Progress = value;
                _point.X = _xAnimation.Value ?? 0;
                _point.Y = _yAnimation.Value ?? 0;
                OnValueChanged();
            }
        }

        public override PointF Value => GetValue(null, 0);

        public override PointF GetValue(IKeyframe<PointF> keyframe, float keyframeProgress)
        {
            return _point;
        }
    }
}