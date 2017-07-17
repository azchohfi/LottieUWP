using System.Numerics;

namespace LottieUWP
{
    internal class AnimatableSplitDimensionPathValue : IAnimatableValue<Vector2?>
    {
        private readonly AnimatableFloatValue _animatableXDimension;
        private readonly AnimatableFloatValue _animatableYDimension;

        internal AnimatableSplitDimensionPathValue(AnimatableFloatValue animatableXDimension, AnimatableFloatValue animatableYDimension)
        {
            _animatableXDimension = animatableXDimension;
            _animatableYDimension = animatableYDimension;
        }

        public IBaseKeyframeAnimation<Vector2?> CreateAnimation()
        {
            return new SplitDimensionPathKeyframeAnimation((KeyframeAnimation<float?>)_animatableXDimension.CreateAnimation(), (KeyframeAnimation<float?>)_animatableYDimension.CreateAnimation());
        }

        public bool HasAnimation()
        {
            return _animatableXDimension.HasAnimation() || _animatableYDimension.HasAnimation();
        }
    }
}