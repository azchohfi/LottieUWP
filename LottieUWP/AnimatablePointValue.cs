using System.Collections.Generic;
using System.Numerics;
using Windows.Data.Json;

namespace LottieUWP
{
    internal class AnimatablePointValue : BaseAnimatableValue<Vector2?, Vector2?>
    {
        private AnimatablePointValue(List<IKeyframe<Vector2?>> keyframes, Vector2? initialValue) : base(keyframes, initialValue)
        {
        }

        protected override Vector2? ConvertType(Vector2? value)
        {
            return value;
        }

        public override IBaseKeyframeAnimation<Vector2?> CreateAnimation()
        {
            if (!HasAnimation())
            {
                return new StaticKeyframeAnimation<Vector2?>(_initialValue);
            }
            return new PointKeyframeAnimation(Keyframes);
        }

        internal static class Factory
        {
            internal static AnimatablePointValue NewInstance(JsonObject json, LottieComposition composition)
            {
                var result = AnimatableValueParser<Vector2?>.NewInstance(json, composition.DpScale, composition, PointFFactory.Instance).ParseJson();
                return new AnimatablePointValue(result.Keyframes, result.InitialValue);
            }
        }
    }
}