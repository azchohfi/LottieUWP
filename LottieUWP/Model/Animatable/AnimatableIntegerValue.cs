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
        private AnimatableIntegerValue() : base(100)
        {
        }

        internal AnimatableIntegerValue(List<IKeyframe<int?>> keyframes, int? initialValue) : base(keyframes, initialValue)
        {
        }

        protected override int? ConvertType(int? value)
        {
            return value;
        }

        public override IBaseKeyframeAnimation<int?, int?> CreateAnimation()
        {
            if (!HasAnimation())
            {
                return new StaticKeyframeAnimation<int?, int?>(_initialValue);
            }

            return new IntegerKeyframeAnimation(Keyframes);
        }

        public override int? InitialValue => _initialValue;

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
                var result = AnimatableValueParser<int?>.NewInstance(json, 1, composition, ValueFactory.Instance).ParseJson();
                var initialValue = result.InitialValue;
                return new AnimatableIntegerValue(result.Keyframes, initialValue);
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