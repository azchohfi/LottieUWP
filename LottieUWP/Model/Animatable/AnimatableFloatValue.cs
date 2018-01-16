using System.Collections.Generic;
using LottieUWP.Animation;
using LottieUWP.Animation.Keyframe;

namespace LottieUWP.Model.Animatable
{
    public class AnimatableFloatValue : BaseAnimatableValue<float?, float?>
    {
        internal AnimatableFloatValue() : this(0f)
        {
        }

        private AnimatableFloatValue(float? value) : base(value)
        {
        }

        public AnimatableFloatValue(List<Keyframe<float?>> keyframes) : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<float?, float?> CreateAnimation()
        {
            return new FloatKeyframeAnimation(Keyframes);
        }
    }
}