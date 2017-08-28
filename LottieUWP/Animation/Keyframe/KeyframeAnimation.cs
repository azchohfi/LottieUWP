using System.Collections.Generic;

namespace LottieUWP.Animation.Keyframe
{
    public abstract class KeyframeAnimation<T> : BaseKeyframeAnimation<T, T>
    {
        internal KeyframeAnimation(List<IKeyframe<T>> keyframes) : base(keyframes)
        {
        }
    }
}
