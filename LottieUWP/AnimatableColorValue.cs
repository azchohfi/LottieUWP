using System.Collections.Generic;
using Windows.Data.Json;
using Windows.UI;

namespace LottieUWP
{
    internal class AnimatableColorValue : BaseAnimatableValue<Color, Color>
    {
        private AnimatableColorValue(IList<IKeyframe<Color>> keyframes, Color initialValue) : base(keyframes, initialValue)
        {
        }

        protected override Color ConvertType(Color value)
        {
            return value;
        }

        public override IBaseKeyframeAnimation<Color> CreateAnimation()
        {
            if (!HasAnimation())
            {
                return new StaticKeyframeAnimation<Color>(_initialValue);
            }
            return new ColorKeyframeAnimation(Keyframes);
        }

        public override string ToString()
        {
            return "AnimatableColorValue{" + "initialValue=" + _initialValue + '}';
        }

        internal static class Factory
        {
            internal static AnimatableColorValue NewInstance(JsonObject json, LottieComposition composition)
            {
                var result = AnimatableValueParser<Color>.NewInstance(json, 1f, composition, ColorFactory.Instance).ParseJson();
                return new AnimatableColorValue(result.Keyframes, result.InitialValue);
            }
        }
    }
}