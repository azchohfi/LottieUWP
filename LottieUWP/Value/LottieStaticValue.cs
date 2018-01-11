namespace LottieUWP.Value
{
    public class LottieStaticValue<T> : LottieValueCallback<T>
    {
        private readonly T _value;

        public LottieStaticValue(T value)
        {
            _value = value;
        }

        public override T GetValue(float startFrame, float endFrame, T startValue, T endValue, float linearKeyframeProgress, float interpolatedKeyframeProgress, float overallProgress)
        {
            return _value;
        }
    }
}
