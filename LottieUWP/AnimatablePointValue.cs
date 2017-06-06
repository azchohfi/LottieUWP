using System.Collections.Generic;
using Windows.Data.Json;

namespace LottieUWP
{
    internal class AnimatablePointValue : BaseAnimatableValue<PointF, PointF>
    {
        private AnimatablePointValue(IList<IKeyframe<PointF>> keyframes, PointF initialValue) : base(keyframes, initialValue)
        {
        }

        internal override PointF ConvertType(PointF value)
        {
            return value;
        }

        public override IBaseKeyframeAnimation<PointF> CreateAnimation()
        {
            if (!HasAnimation())
            {
                return new StaticKeyframeAnimation<PointF>(_initialValue);
            }
            return new PointKeyframeAnimation(Keyframes);
        }

        internal static class Factory
        {
            internal static AnimatablePointValue NewInstance(JsonObject json, LottieComposition composition)
            {
                var result = AnimatableValueParser<PointF>.NewInstance(json, composition.DpScale, composition, PointFFactory.Instance).ParseJson();
                return new AnimatablePointValue(result.Keyframes, result.InitialValue);
            }
        }
    }
}