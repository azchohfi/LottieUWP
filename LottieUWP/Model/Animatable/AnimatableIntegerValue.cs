using System;
using System.Collections.Generic;
using LottieUWP.Animation;
using LottieUWP.Animation.Keyframe;
using LottieUWP.Utils;

namespace LottieUWP.Model.Animatable
{
    public class AnimatableIntegerValue : BaseAnimatableValue<int?, int?>
    {
        private AnimatableIntegerValue() : this(100)
        {
        }

        internal AnimatableIntegerValue(int? value) : base(value)
        {
        }

        internal AnimatableIntegerValue(List<Keyframe<int?>> keyframes) : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<int?, int?> CreateAnimation()
        {
            return new IntegerKeyframeAnimation(Keyframes);
        }

        internal static class Factory
        {
            internal static AnimatableIntegerValue NewInstance()
            {
                return new AnimatableIntegerValue();
            }

            internal static AnimatableIntegerValue NewInstance(JsonReader reader, LottieComposition composition)
            {
                return new AnimatableIntegerValue(AnimatableValueParser<int?>.NewInstance(reader, 1, composition, ValueFactory.Instance));
            }
        }

        private class ValueFactory : IAnimatableValueFactory<int?>
        {
            internal static readonly ValueFactory Instance = new ValueFactory();

            public int? ValueFromObject(JsonReader reader, float scale)
            {
                return (int?) Math.Round(JsonUtils.ValueFromObject(reader) * scale);
            }
        }
    }
}