namespace LottieUWP
{
    internal class AnimatableSplitDimensionPathValue : IAnimatableValue<PointF>
    {
        private readonly AnimatableFloatValue _animatableXDimension;
        private readonly AnimatableFloatValue _animatableYDimension;

        internal AnimatableSplitDimensionPathValue(AnimatableFloatValue animatableXDimension, AnimatableFloatValue animatableYDimension)
        {
            _animatableXDimension = animatableXDimension;
            _animatableYDimension = animatableYDimension;
        }

        public IBaseKeyframeAnimation<PointF> CreateAnimation()
        {
            return new SplitDimensionPathKeyframeAnimation((KeyframeAnimation<float?>)_animatableXDimension.CreateAnimation(), (KeyframeAnimation<float?>)_animatableYDimension.CreateAnimation());
        }

        public bool HasAnimation()
        {
            return _animatableXDimension.HasAnimation() || _animatableYDimension.HasAnimation();
        }
    }
}