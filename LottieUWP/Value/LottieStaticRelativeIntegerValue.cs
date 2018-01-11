namespace LottieUWP.Value
{
    public class LottieStaticRelativeIntegerValue : LottieRelativeIntegerValueCallback
    {
        private readonly int _offset; 
 
        public LottieStaticRelativeIntegerValue(int offset)
        {
            _offset = offset;
        }

        public override int GetOffset(float startFrame, float endFrame, int startValue, int endValue, float linearKeyframeProgress, float interpolatedKeyframeProgress, float overallProgress)
        {
            return _offset;
        }
    }
}
