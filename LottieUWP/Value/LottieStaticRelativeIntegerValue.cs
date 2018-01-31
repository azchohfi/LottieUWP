namespace LottieUWP.Value
{
    public class LottieStaticRelativeIntegerValue : LottieRelativeIntegerValueCallback
    {
        private readonly int _offset; 
 
        public LottieStaticRelativeIntegerValue(int offset)
        {
            _offset = offset;
        }

        public override int GetOffset(LottieFrameInfo<int> frameInfo)
        {
            return _offset;
        }
    }
}
