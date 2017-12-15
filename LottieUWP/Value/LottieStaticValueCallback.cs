namespace LottieUWP.Value
{
    public class LottieStaticValueCallback<T> : ILottieValueCallback<T>
    {
        private readonly T _value;

        public LottieStaticValueCallback(T value)
        {
            _value = value;
        }

        public T GetValue(float startFrame, float endFrame, T startValue, T endValue, float linearKeyframeProgress, float interpolatedKeyframeProgress, float overallProgress)
        {
            return _value;
        }
    }
}
