namespace LottieUWP.Value
{
    public abstract class LottieStaticInterpolatedValue<T> : LottieValueCallback<T>
    {
        private readonly T _startValue;
        private readonly T _endValue;
        private readonly IInterpolator _interpolator;

        protected LottieStaticInterpolatedValue(T startValue, T endValue)
            : this(startValue, endValue, new LinearInterpolator())
        {
        }

        protected LottieStaticInterpolatedValue(T startValue, T endValue, IInterpolator interpolator)
        {
            _startValue = startValue;
            _endValue = endValue;
            _interpolator = interpolator;
        }

        public override T GetValue(float startFrame, float endFrame, T startValue, T endValue,
            float linearKeyframeProgress, float interpolatedKeyframeProgress, float overallProgress)
        {
            float progress = _interpolator.GetInterpolation(overallProgress);
            return InterpolateValue(_startValue, _endValue, progress);
        }

        protected abstract T InterpolateValue(T startValue, T endValue, float progress);
    }
}
