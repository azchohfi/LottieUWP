using System.Collections.Generic;
using Windows.Data.Json;
using Windows.UI;
using LottieUWP.Animation;
using LottieUWP.Animation.Keyframe;

namespace LottieUWP.Model.Animatable
{
    internal class AnimatableColorValue : BaseAnimatableValue<Color, Color>
    {
        private AnimatableColorValue(List<IKeyframe<Color>> keyframes, Color initialValue) : base(keyframes, initialValue)
        {
        }

        protected override Color ConvertType(Color value)
        {
            return value;
        }

        public override IBaseKeyframeAnimation<Color, Color> CreateAnimation()
        {
            if (!HasAnimation())
            {
                return new StaticKeyframeAnimation<Color, Color>(_initialValue);
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