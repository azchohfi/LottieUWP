using System.Collections.Generic;
using System.Numerics;
using LottieUWP.Animation;
using LottieUWP.Animation.Keyframe;

namespace LottieUWP.Model.Animatable
{
    public class AnimatablePointValue : BaseAnimatableValue<Vector2?, Vector2?>
    {
        private AnimatablePointValue(List<Keyframe<Vector2?>> keyframes) : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<Vector2?, Vector2?> CreateAnimation()
        {
            return new PointKeyframeAnimation(Keyframes);
        }

        internal static class Factory
        {
            internal static AnimatablePointValue NewInstance(JsonReader reader, LottieComposition composition)
            {
                return new AnimatablePointValue(AnimatableValueParser<Vector2?>.NewInstance(reader, Utils.Utils.DpScale(), composition, PointFFactory.Instance));
            }
        }
    }
}