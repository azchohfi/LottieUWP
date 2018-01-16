using System.Collections.Generic;
using Windows.UI;
using LottieUWP.Animation;
using LottieUWP.Animation.Keyframe;

namespace LottieUWP.Model.Animatable
{
    public class AnimatableColorValue : BaseAnimatableValue<Color, Color>
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
            internal static AnimatableColorValue NewInstance(JsonReader reader, LottieComposition composition)
            {
                return new AnimatableColorValue(AnimatableValueParser<Color>.NewInstance(reader, 1f, composition, ColorFactory.Instance));
            }
        }
    }
}