namespace LottieUWP.Value
{
    /// <summary>
    /// Static value version of <see cref="LottieFloatRelativeValueCallback"/>.
    /// </summary>
    public abstract class LottieStaticFloatRelativeValueCallback : LottieFloatRelativeValueCallback
    {
        private readonly float _offset;

        public LottieStaticFloatRelativeValueCallback(float offset)
        {
            _offset = offset;
        }

        public override float GetOffset(float startFrame, float endFrame, float startValue, float endValue, float linearKeyframeProgress, float interpolatedKeyframeProgress, float overallProgress)
        {
            return _offset;
        }
    }
}
