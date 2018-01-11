using System.Collections.Generic;
using LottieUWP.Animation;
using LottieUWP.Animation.Keyframe;
using LottieUWP.Utils;

namespace LottieUWP.Model.Animatable
{
    internal class AnimatableFloatValue : BaseAnimatableValue<float?, float?>
    {
        private AnimatableFloatValue() : this(0f)
        {
        }

        private AnimatableFloatValue(float? value) : base(value)
        {
        }

        private AnimatableFloatValue(List<Keyframe<float?>> keyframes) : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<float?, float?> CreateAnimation()
        {
            return new FloatKeyframeAnimation(Keyframes);
        }

        private class ValueFactory : IAnimatableValueFactory<float?>
        {
            internal static readonly ValueFactory Instance = new ValueFactory();

            public virtual float? ValueFromObject(JsonReader reader, float scale)
            {
                return JsonUtils.ValueFromObject(reader) * scale;
            }
        }

        internal static class Factory
        {
            internal static AnimatableFloatValue NewInstance()
            {
                return new AnimatableFloatValue();
            }

            internal static AnimatableFloatValue NewInstance(JsonReader reader, LottieComposition composition, bool isDp = true)
            {
                var scale = isDp ? Utils.Utils.DpScale() : 1f;
                return new AnimatableFloatValue(AnimatableValueParser<float?>.NewInstance(reader, scale, composition, ValueFactory.Instance));
            }
        }
    }
}