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

        public override float GetOffset(float startFrame, float endFrame, float startValue, float endValue, float linearKeyframeProgress, float interpolatedKeyframeProgress, float overallProgress)
        {
            return _offset;
        }
    }
}
