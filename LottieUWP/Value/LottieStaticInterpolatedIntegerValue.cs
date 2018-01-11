using LottieUWP.Utils;

namespace LottieUWP.Value
{
    public class LottieStaticInterpolatedIntegerValue : LottieStaticInterpolatedValue<int>
    {
        public LottieStaticInterpolatedIntegerValue(int startValue, int endValue)
            : base(startValue, endValue)
        {
        }

        public LottieStaticInterpolatedIntegerValue(int startValue, int endValue,
            IInterpolator interpolator)
            : base(startValue, endValue, interpolator)
        {
        }

        protected override int InterpolateValue(int startValue, int endValue, float progress)
        {
            return MiscUtils.Lerp(startValue, endValue, progress);
        }
    }
}