using System.Numerics;

namespace LottieUWP.Value
{
    public class LottieStaticPointRelativeValueCallback : LottiePointRelativeValueCallback
    {
        private readonly Vector2 _offset;

        public LottieStaticPointRelativeValueCallback(Vector2 offset)
        {
            _offset = offset;
        }

        public override Vector2 GetOffset(float startFrame, float endFrame, Vector2 startValue, Vector2 endValue, float linearKeyframeProgress,
            float interpolatedKeyframeProgress, float overallProgress)
        {
            return _offset;
        }
    }
}
