using System.Collections.Generic;
using LottieUWP.Value;
using LottieUWP.Animation.Keyframe;

namespace LottieUWP.Model.Animatable
{
    public class AnimatableTextFrame : BaseAnimatableValue<DocumentData, DocumentData>
    {
        public AnimatableTextFrame(List<Keyframe<DocumentData>> keyframes) : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<DocumentData, DocumentData> CreateAnimation()
        {
            return new TextKeyframeAnimation(Keyframes);
        }
    }
}
