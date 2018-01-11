using System.Numerics;
using LottieUWP.Utils;

namespace LottieUWP.Value
{
    public class LottieStaticInterpolatedPointValue : LottieStaticInterpolatedValue<Vector2>
    {
        private Vector2 _point;

        public LottieStaticInterpolatedPointValue(Vector2 startValue, Vector2 endValue)
        : base(startValue, endValue)
        {
        }

        public LottieStaticInterpolatedPointValue(Vector2 startValue, Vector2 endValue, IInterpolator interpolator)
        : base(startValue, endValue, interpolator)
        {
        }

        protected override Vector2 InterpolateValue(Vector2 startValue, Vector2 endValue, float progress)
        {
            _point.X = MiscUtils.Lerp(startValue.X, endValue.X, progress);
            _point.Y = MiscUtils.Lerp(startValue.Y, endValue.Y, progress);
            return _point;
        }
    }
}
