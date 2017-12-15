using System.Collections.Generic;

namespace LottieUWP.Animation.Keyframe
{
    public abstract class KeyframeAnimation<T> : BaseKeyframeAnimation<T, T>
    {
        internal KeyframeAnimation(List<Keyframe<T>> keyframes) : base(keyframes)
        {
        }
    }
}
