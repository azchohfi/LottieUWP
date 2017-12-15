using System.Collections.Generic;
using Windows.Data.Json;
using Windows.UI;
using LottieUWP.Animation;
using LottieUWP.Animation.Keyframe;

namespace LottieUWP.Model.Animatable
{
    internal class AnimatableColorValue : BaseAnimatableValue<Color, Color>
    {
        private AnimatableColorValue(List<Keyframe<Color>> keyframes) : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<Color, Color> CreateAnimation()
        {
            return new ColorKeyframeAnimation(Keyframes);
        }

        internal static class Factory
        {
            internal static AnimatableColorValue NewInstance(JsonObject json, LottieComposition composition)
            {
                return new AnimatableColorValue(AnimatableValueParser<Color>.NewInstance(json, 1f, composition, ColorFactory.Instance));
            }
        }
    }
}