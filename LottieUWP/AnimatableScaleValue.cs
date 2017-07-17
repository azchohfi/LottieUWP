using System.Collections.Generic;
using Windows.Data.Json;

namespace LottieUWP
{
    internal class AnimatableScaleValue : BaseAnimatableValue<ScaleXy, ScaleXy>
    {
        private AnimatableScaleValue() : base(new ScaleXy())
        {
        }

        internal AnimatableScaleValue(List<IKeyframe<ScaleXy>> keyframes, ScaleXy initialValue) : base(keyframes, initialValue)
        {
        }

        protected override ScaleXy ConvertType(ScaleXy value)
        {
            return value;
        }

        public override IBaseKeyframeAnimation<ScaleXy> CreateAnimation()
        {
            if (!HasAnimation())
            {
                return new StaticKeyframeAnimation<ScaleXy>(_initialValue);
            }
            return new ScaleKeyframeAnimation(Keyframes);
        }

        internal static class Factory
        {
            internal static AnimatableScaleValue NewInstance(JsonObject json, LottieComposition composition)
            {
                var result = AnimatableValueParser<ScaleXy>.NewInstance(json, 1, composition, ScaleXy.Factory.Instance).ParseJson();
                return new AnimatableScaleValue(result.Keyframes, result.InitialValue);
            }

            internal static AnimatableScaleValue NewInstance()
            {
                return new AnimatableScaleValue();
            }
        }
    }
}