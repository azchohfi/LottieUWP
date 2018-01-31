namespace LottieUWP.Value
{
    public class LottieStaticValue<T> : LottieValueCallback<T>
    {
        private readonly T _value;

        public LottieStaticValue(T value)
        {
            _value = value;
        }

        public override T GetValue(LottieFrameInfo<T> frameInfo)
        {
            return _value;
        }
    }
}
