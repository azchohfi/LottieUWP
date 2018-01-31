using System.Numerics;

namespace LottieUWP.Value
{
    public class LottieStaticRelativePointValue : LottieRelativePointValueCallback
    {
        private readonly Vector2 _offset;

        public LottieStaticRelativePointValue(Vector2 offset)
        {
            _offset = offset;
        }

        public override Vector2 GetOffset(LottieFrameInfo<Vector2> frameInfo)
        {
            return _offset;
        }
    }
}
