using LottieUWP.Utils;

namespace LottieUWP.Value
{
    // ReSharper disable once UnusedMember.Global
    public class LottieInterpolatedIntegerValue : LottieInterpolatedValue<int>
    {
        public LottieInterpolatedIntegerValue(int startValue, int endValue)
            : base(startValue, endValue)
        {
        }

        public LottieInterpolatedIntegerValue(int startValue, int endValue, IInterpolator interpolator)
            : base(startValue, endValue, interpolator)
        {
        }

        protected override int InterpolateValue(int startValue, int endValue, float progress)
        {
            return MiscUtils.Lerp(startValue, endValue, progress);
        }
    }
}