using System.Collections.Generic;
using LottieUWP.Animation.Keyframe;
using LottieUWP.Value;

namespace LottieUWP.Model.Animatable
{
    public class AnimatableScaleValue : BaseAnimatableValue<ScaleXy, ScaleXy>
    {
        internal AnimatableScaleValue() : this(new ScaleXy())
        {
        }

        public AnimatableScaleValue(ScaleXy value) : base(value)
        {
        }

        public AnimatableScaleValue(List<Keyframe<ScaleXy>> keyframes) : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<ScaleXy, ScaleXy> CreateAnimation()
        {
            return new ScaleKeyframeAnimation(Keyframes);
        }
    }
}