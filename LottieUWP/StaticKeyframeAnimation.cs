using System.Collections.Generic;

namespace LottieUWP
{
    internal interface IStaticKeyFrameAnimation
    {
    }

    internal class StaticKeyframeAnimation<T> : KeyframeAnimation<T>, IStaticKeyFrameAnimation
    {
        private readonly T _initialValue;

        internal StaticKeyframeAnimation(T initialValue) : base(new List<IKeyframe<T>>())
        {
            _initialValue = initialValue;
        }

        public override float Progress
        {
            set
            {
                // Do nothing
            }
        }

        public override void AddUpdateListener(BaseKeyframeAnimation.IAnimationListener listener)
        {
            // Do nothing. 
         }

        public override T Value => _initialValue;

        public override T GetValue(IKeyframe<T> keyframe, float keyframeProgress)
        {
            return _initialValue;
        }
    }
}