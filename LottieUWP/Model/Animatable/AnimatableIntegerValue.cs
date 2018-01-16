using System.Collections.Generic;
using LottieUWP.Animation;
using LottieUWP.Animation.Keyframe;

namespace LottieUWP.Model.Animatable
{
    public class AnimatableIntegerValue : BaseAnimatableValue<int?, int?>
    {
        public AnimatableIntegerValue() : this(100)
        {
        }

        public AnimatableIntegerValue(int? value) : base(value)
        {
        }

        public AnimatableIntegerValue(List<Keyframe<int?>> keyframes) : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<int?, int?> CreateAnimation()
        {
            return new IntegerKeyframeAnimation(Keyframes);
        }
    }
}