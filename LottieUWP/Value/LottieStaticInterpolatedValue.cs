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

        public override T GetValue(LottieFrameInfo<T> frameInfo)
        {
            float progress = _interpolator.GetInterpolation(frameInfo.OverallProgress);
            return InterpolateValue(_startValue, _endValue, progress);
        }

        protected abstract T InterpolateValue(T startValue, T endValue, float progress);
    }
}
