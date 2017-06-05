using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Data.Json;

namespace LottieUWP
{
    internal class AnimatableIntegerValue : BaseAnimatableValue<int?, int?>
    {
        private AnimatableIntegerValue() : base(100)
        {
        }

        internal AnimatableIntegerValue(IList<IKeyframe<int?>> keyframes, int? initialValue) : base(keyframes, initialValue)
        {
        }

        internal override int? ConvertType(int? value)
        {
            return value;
        }

        public override IBaseKeyframeAnimation<int?> CreateAnimation()
        {
            if (!HasAnimation())
            {
                return new StaticKeyframeAnimation<int?>(_initialValue);
            }

            return new IntegerKeyframeAnimation(Keyframes);
        }

        public override int? InitialValue => _initialValue;

        internal sealed class Factory
        {
            internal static AnimatableIntegerValue NewInstance()
            {
                return new AnimatableIntegerValue();
            }

            internal static AnimatableIntegerValue NewInstance(JsonObject json, LottieComposition composition)
            {
                if (json.ContainsKey("x"))
                {
                    Debug.WriteLine("Animation has expressions which are not supported.", L.Tag);
                }
                AnimatableValueParser<int?>.Result result = AnimatableValueParser<int?>.NewInstance(json, 1, composition, ValueFactory.Instance).ParseJson();
                int? initialValue = result.InitialValue;
                return new AnimatableIntegerValue(result.Keyframes, initialValue);
            }
        }

        private class ValueFactory : IAnimatableValueFactory<int?>
        {
            internal static readonly ValueFactory Instance = new ValueFactory();

            public virtual int? ValueFromObject(IJsonValue @object, float scale)
            {
                return (int?) Math.Round(JsonUtils.ValueFromObject(@object) * scale);
            }
        }
    }
}