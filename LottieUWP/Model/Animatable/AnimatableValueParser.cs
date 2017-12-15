using System.Collections.Generic;
using Windows.Data.Json;
using LottieUWP.Animation;

namespace LottieUWP.Model.Animatable
{
    internal class AnimatableValueParser<T>
    {
        private readonly JsonObject _json;
        private readonly float _scale;
        private readonly LottieComposition _composition;
        private readonly IAnimatableValueFactory<T> _valueFactory;

        private AnimatableValueParser(JsonObject json, float scale, LottieComposition composition, IAnimatableValueFactory<T> valueFactory)
        {
            _json = json;
            _scale = scale;
            _composition = composition;
            _valueFactory = valueFactory;
        }

        internal static List<Keyframe<T>> NewInstance(JsonObject json, float scale, LottieComposition composition, IAnimatableValueFactory<T> valueFactory)
        {
            var parser = new AnimatableValueParser<T>(json, scale, composition, valueFactory);
            return parser.ParseKeyframes();
        }

        private List<Keyframe<T>> ParseKeyframes()
        {
            var k = _json["k"];
            if (HasKeyframes(k))
            {
                return Keyframe<T>.KeyFrameFactory.ParseKeyframes(k.GetArray(), _composition, _scale, _valueFactory);
            }
            return ParseStaticValue();
        }

        private List<Keyframe<T>> ParseStaticValue()
        {
            T initialValue = _valueFactory.ValueFromObject(_json["k"], _scale);
            return new List<Keyframe<T>> { new Keyframe<T>(initialValue) };
        }

        private static bool HasKeyframes(IJsonValue json)
        {
            if (json.ValueType != JsonValueType.Array)
            {
                return false;
            }

            var firstObject = json.GetArray()[0];
            return firstObject.ValueType == JsonValueType.Object && firstObject.GetObject().ContainsKey("t");
        }
    }
}