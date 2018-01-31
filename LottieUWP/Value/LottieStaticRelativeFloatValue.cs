namespace LottieUWP.Value
{
    /// <summary>
    /// Static value version of <see cref="LottieRelativeFloatValueCallback"/>.
    /// </summary>
    public abstract class LottieStaticRelativeFloatValue : LottieRelativeFloatValueCallback
    {
        private readonly float _offset;

        public LottieStaticRelativeFloatValue(float offset)
        {
            _offset = offset;
        }

        public override float GetOffset(LottieFrameInfo<float> frameInfo)
        {
            return _offset;
        }
    }
}
