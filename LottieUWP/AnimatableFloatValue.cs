using System.Collections.Generic;
using System.Diagnostics;
using Windows.Data.Json;

namespace LottieUWP
{
    internal class AnimatableFloatValue : BaseAnimatableValue<float?, float?>
    {
        private AnimatableFloatValue() : base(0f)
        {
        }

        private AnimatableFloatValue(IList<IKeyframe<float?>> keyframes, float? initialValue) : base(keyframes, initialValue)
        {
        }

        internal override float? ConvertType(float? value)
        {
            return value;
        }

        public override IBaseKeyframeAnimation<float?> CreateAnimation()
        {
            if (!HasAnimation())
            {
                return new StaticKeyframeAnimation<float?>(_initialValue);
            }

            return new FloatKeyframeAnimation(Keyframes);
        }

        public override float? InitialValue => _initialValue;

        private class ValueFactory : IAnimatableValueFactory<float?>
        {
            internal static readonly ValueFactory Instance = new ValueFactory();

            public virtual float? ValueFromObject(IJsonValue @object, float scale)
            {
                return JsonUtils.ValueFromObject(@object) * scale;
            }
        }

        internal static class Factory
        {
            internal static AnimatableFloatValue NewInstance()
            {
                return new AnimatableFloatValue();
            }

            internal static AnimatableFloatValue NewInstance(JsonObject json, LottieComposition composition, bool isDp = true)
            {
                var scale = isDp ? composition.DpScale : 1f;
                if (json.ContainsKey("x"))
                {
                    Debug.WriteLine("Animation has expressions which are not supported.", "LOTTIE");
                }
                var result = AnimatableValueParser<float?>.NewInstance(json, scale, composition, ValueFactory.Instance).ParseJson();
                return new AnimatableFloatValue(result.Keyframes, result.InitialValue);
            }
        }
    }

}