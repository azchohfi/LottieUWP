using System.Collections.Generic;
using LottieUWP.Animation;
using LottieUWP.Animation.Keyframe;
using LottieUWP.Model.Content;

namespace LottieUWP.Model.Animatable
{
    internal class AnimatableShapeValue : BaseAnimatableValue<ShapeData, Path>
    {
        private AnimatableShapeValue(List<Keyframe<ShapeData>> keyframes) : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<ShapeData, Path> CreateAnimation()
        {
            return new ShapeKeyframeAnimation(Keyframes);
        }

        internal static class Factory
        {
            internal static AnimatableShapeValue NewInstance(JsonReader reader, LottieComposition composition)
            {
                return new AnimatableShapeValue(AnimatableValueParser<ShapeData>.NewInstance(reader, Utils.Utils.DpScale(), composition, ShapeData.Factory.Instance));
            }
        }
    }
}