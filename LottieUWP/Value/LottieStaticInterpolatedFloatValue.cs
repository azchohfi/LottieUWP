using LottieUWP.Utils;

namespace LottieUWP.Value
{
    public class LottieStaticInterpolatedFloatValue : LottieStaticInterpolatedValue<float>
    {
        public LottieStaticInterpolatedFloatValue(float startValue, float endValue)
            : base(startValue, endValue)
        {
        }

        public LottieStaticInterpolatedFloatValue(float startValue, float endValue, IInterpolator interpolator)
            : base(startValue, endValue, interpolator)
        {
        }

        protected override float InterpolateValue(float startValue, float endValue, float progress)
        {
            return MiscUtils.Lerp(startValue, endValue, progress);
        }
    }
}
