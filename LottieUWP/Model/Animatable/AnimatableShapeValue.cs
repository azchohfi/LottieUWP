using System.Collections.Generic;
using LottieUWP.Value;
using LottieUWP.Animation.Keyframe;
using LottieUWP.Model.Content;

namespace LottieUWP.Model.Animatable
{
    public class AnimatableShapeValue : BaseAnimatableValue<ShapeData, Path>
    {
        public AnimatableShapeValue(List<Keyframe<ShapeData>> keyframes) : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<ShapeData, Path> CreateAnimation()
        {
            return new ShapeKeyframeAnimation(Keyframes);
        }
    }
}