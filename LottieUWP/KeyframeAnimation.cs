using System.Collections.Generic;

namespace LottieUWP
{
    public abstract class KeyframeAnimation<T> : BaseKeyframeAnimation<T, T>
    {
        internal KeyframeAnimation(IList<IKeyframe<T>> keyframes) : base(keyframes)
        {
        }
    }
}
