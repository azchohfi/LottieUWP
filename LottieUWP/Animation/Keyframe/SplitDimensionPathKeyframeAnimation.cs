using LottieUWP.Value;
using System.Collections.Generic;
using System.Numerics;

namespace LottieUWP.Animation.Keyframe
{
    internal class SplitDimensionPathKeyframeAnimation : BaseKeyframeAnimation<Vector2?, Vector2?>
    {
        private Vector2 _point;
        private readonly IBaseKeyframeAnimation<float?, float?> _xAnimation;
        private readonly IBaseKeyframeAnimation<float?, float?> _yAnimation;

        internal SplitDimensionPathKeyframeAnimation(IBaseKeyframeAnimation<float?, float?> xAnimation, IBaseKeyframeAnimation<float?, float?> yAnimation)
            : base(new List<Keyframe<Vector2?>>())
        {
            _xAnimation = xAnimation;
            _yAnimation = yAnimation;
            // We need to call an initial setProgress so point gets set with the initial value. 
            Progress = Progress;
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

        public override Vector2? Value => GetValue(null, 0);

        public override Vector2? GetValue(Keyframe<Vector2?> keyframe, float keyframeProgress)
        {
            return _point;
        }
    }
}