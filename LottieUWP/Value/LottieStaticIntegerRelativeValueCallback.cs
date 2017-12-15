namespace LottieUWP.Value
{
    public class LottieStaticIntegerRelativeValueCallback : LottieIntegerRelativeValueCallback
    {
        private readonly int _offset; 
 
        public LottieStaticIntegerRelativeValueCallback(int offset)
        {
            _offset = offset;
        }

        public override int GetOffset(float startFrame, float endFrame, int startValue, int endValue, float linearKeyframeProgress, float interpolatedKeyframeProgress, float overallProgress)
        {
            return _offset;
        }
    }
}
