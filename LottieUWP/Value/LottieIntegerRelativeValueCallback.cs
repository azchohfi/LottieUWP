using LottieUWP.Utils;

namespace LottieUWP.Value
{
    /// <summary>
    /// <see cref="LottieUWP.Value.ILottieValueCallback{T}"/> that provides a value offset from the original animation 
    ///  rather than an absolute value.
    /// </summary>
    public abstract class LottieIntegerRelativeValueCallback : ILottieValueCallback<int>
    {
        public int GetValue(float sf, float ef, int sv, int ev, float lkp, float ikp, float p)
        {
            int originalValue = MiscUtils.Lerp(sv, ev, ikp);
            int newValue = GetOffset(sf, ef, sv, ev, lkp, ikp, p);
            return originalValue + newValue;
        }

        public abstract int GetOffset(float startFrame, float endFrame, int startValue, int endValue, float linearKeyframeProgress, float interpolatedKeyframeProgress, float overallProgress);
    }
}
