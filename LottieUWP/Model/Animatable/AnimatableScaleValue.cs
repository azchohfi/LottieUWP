using System.Collections.Generic;
using LottieUWP.Animation;
using LottieUWP.Animation.Keyframe;
using LottieUWP.Value;

namespace LottieUWP.Model.Animatable
{
    internal class AnimatableScaleValue : BaseAnimatableValue<ScaleXy, ScaleXy>
    {
        private AnimatableScaleValue() : this(new ScaleXy())
        {
        }

        internal AnimatableScaleValue(ScaleXy value) : base(value)
        {
        }

        internal AnimatableScaleValue(List<Keyframe<ScaleXy>> keyframes) : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<ScaleXy, ScaleXy> CreateAnimation()
        {
            return new ScaleKeyframeAnimation(Keyframes);
        }

        internal static class Factory
        {
            internal static AnimatableScaleValue NewInstance(JsonReader reader, LottieComposition composition)
            {
                return new AnimatableScaleValue(AnimatableValueParser<ScaleXy>.NewInstance(reader, 1, composition, ScaleXy.Factory.Instance));
            }

            internal static AnimatableScaleValue NewInstance()
            {
                return new AnimatableScaleValue();
            }
        }
    }
}