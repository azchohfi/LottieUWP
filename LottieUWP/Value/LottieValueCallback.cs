using LottieUWP.Animation.Keyframe;

namespace LottieUWP.Value
{
    /// <summary>
    /// Allows you to set a callback on a resolved <see cref="Model.KeyPath"/> to modify its animation values at runtime. 
    /// This API is not ready for public use yet. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class LottieValueCallback<T> : ILottieValueCallback<T>
    {
        IBaseKeyframeAnimation _animation;

        /// <summary>
        /// This can be set with <see cref="SetValue(T)"/> to use a value instead of deferring
        /// to the callback.
        /// </summary>
        private T _value;

        public abstract T GetValue(
            float startFrame,
            float endFrame,
            T startValue,
            T endValue,
            float linearKeyframeProgress,
            float interpolatedKeyframeProgress,
            float overallProgress);

        public void SetValue(T value)
        {
            if (_animation != null)
            {
                _value = value;
                _animation.OnValueChanged();
            }
        }

        public T GetValueInternal(
            float startFrame,
            float endFrame,
            T startValue,
            T endValue,
            float linearKeyframeProgress,
            float interpolatedKeyframeProgress,
            float overallProgress
        )
        {
            if (_value != null)
            {
                return _value;
            }
            return GetValue(startFrame, endFrame, startValue, endValue, linearKeyframeProgress,
                interpolatedKeyframeProgress, overallProgress);
        }

        public void SetAnimation(IBaseKeyframeAnimation animation)
        {
            _animation = animation;
        }
    }
}