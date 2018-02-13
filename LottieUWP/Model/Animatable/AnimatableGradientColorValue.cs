using System.Collections.Generic;
using LottieUWP.Value;
using LottieUWP.Animation.Keyframe;
using LottieUWP.Model.Content;

namespace LottieUWP.Model.Animatable
{
    public class AnimatableGradientColorValue : BaseAnimatableValue<GradientColor, GradientColor>
    {
        public AnimatableGradientColorValue(List<Keyframe<GradientColor>> keyframes) : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<GradientColor, GradientColor> CreateAnimation()
        {
            return new GradientColorKeyframeAnimation(Keyframes);
        }
    }
}