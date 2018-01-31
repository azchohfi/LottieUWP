namespace LottieUWP.Value
{
    internal class SimpleImplLottieValueCallback<T> : LottieValueCallback<T>
    {
        private readonly SimpleLottieValueCallback<T> _callback;

        public SimpleImplLottieValueCallback(SimpleLottieValueCallback<T> callback)
        {
            _callback = callback;
        }

        public override T GetValue(LottieFrameInfo<T> frameInfo)
        {
            if(_callback != null)
                return _callback.Invoke(frameInfo);
            return default(T);
        }
    }
}