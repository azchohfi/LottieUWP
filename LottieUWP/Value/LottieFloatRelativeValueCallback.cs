using LottieUWP.Utils;

namespace LottieUWP.Value
{
    /// <summary>
    /// <see cref="LottieUWP.Value.ILottieValueCallback{T}"/> that provides a value offset from the original animation 
    ///  rather than an absolute value.
    /// </summary>
    public abstract class LottieFloatRelativeValueCallback : ILottieValueCallback<float>
    {
        public float GetValue(float sf, float ef, float sv, float ev, float lkp, float ikp, float p)
        {
            float originalValue = MiscUtils.Lerp(sv, ev, ikp);
            float offset = GetOffset(sf, ef, sv, ev, lkp, ikp, p);
            return originalValue + offset;
        }

        public abstract float GetOffset(float startFrame, float endFrame, float startValue, float endValue, float linearKeyframeProgress, float interpolatedKeyframeProgress, float overallProgress);
    }
}
