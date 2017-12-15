using System;
using System.Collections.Generic;
using Windows.Data.Json;
using LottieUWP.Animation;
using LottieUWP.Animation.Keyframe;
using LottieUWP.Utils;

namespace LottieUWP.Model.Animatable
{
    internal class AnimatableIntegerValue : BaseAnimatableValue<int?, int?>
    {
        private AnimatableIntegerValue() : this(100)
        {
        }

        private AnimatableIntegerValue(int? value) : base(value)
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

            internal static AnimatableIntegerValue NewInstance(JsonObject json, LottieComposition composition)
            {
                if (json != null && json.ContainsKey("x"))
                {
                    composition.AddWarning("Lottie doesn't support expressions.");
                }
                return new AnimatableIntegerValue(AnimatableValueParser<int?>.NewInstance(json, 1, composition, ValueFactory.Instance));
            }
        }

        private class ValueFactory : IAnimatableValueFactory<int?>
        {
            internal static readonly ValueFactory Instance = new ValueFactory();

            public int? ValueFromObject(IJsonValue @object, float scale)
            {
                return (int?) Math.Round(JsonUtils.ValueFromObject(@object) * scale);
            }
        }
    }
}