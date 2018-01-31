using System;
using LottieUWP.Animation.Keyframe;

namespace LottieUWP.Value
{
    /// <summary>
    /// Allows you to set a callback on a resolved <see cref="Model.KeyPath"/> to modify its animation values at runtime. 
    /// This API is not ready for public use yet. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LottieValueCallback<T> : ILottieValueCallback<T>
    {
        private readonly LottieFrameInfo<T> _frameInfo = new LottieFrameInfo<T>();

        IBaseKeyframeAnimation _animation;

        /// <summary>
        /// This can be set with <see cref="SetValue(T)"/> to use a value instead of deferring
        /// to the callback.
        /// </summary>
        private T _value;

        public LottieValueCallback()
        {
        }

        public LottieValueCallback(T staticValue)
        {
            _value = staticValue;
        }

        /// <summary>
        /// Override this if you haven't set a static value in the constructor or with SetValue.
        /// </summary>
        /// <param name="frameInfo"></param>
        /// <returns></returns>
        public virtual T GetValue(LottieFrameInfo<T> frameInfo)
        {
            if (_value == null)
            {
                throw new ArgumentException("You must provide a static value in the constructor " +
                                                   ", call SetValue, or override GetValue.");
            }
            return _value;
        }

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
            return GetValue(
                _frameInfo.Set(
                    startFrame,
                    endFrame,
                    startValue,
                    endValue,
                    linearKeyframeProgress,
                    interpolatedKeyframeProgress,
                    overallProgress
                )
            );
        }

        public void SetAnimation(IBaseKeyframeAnimation animation)
        {
            _animation = animation;
        }
    }
}