using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.Data.Json;
using LottieUWP.Animation;
using LottieUWP.Animation.Keyframe;
using LottieUWP.Utils;

namespace LottieUWP.Model.Animatable
{
    internal class AnimatablePathValue : IAnimatableValue<Vector2?, Vector2?>
    {
        internal static IAnimatableValue<Vector2?, Vector2?> CreateAnimatablePathOrSplitDimensionPath(JsonObject json, LottieComposition composition)
        {
            if (json.ContainsKey("k"))
            {
                return new AnimatablePathValue(json["k"], composition);
            }
            return new AnimatableSplitDimensionPathValue(AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("x"), composition), AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("y"), composition));
        }

        private readonly List<Keyframe<Vector2?>> _keyframes = new List<Keyframe<Vector2?>>();

        /// <summary>
        /// Create a default static animatable path.
        /// </summary>
        internal AnimatablePathValue()
        {
            _keyframes.Add(new Keyframe<Vector2?>(new Vector2(0, 0)));
        }

        internal AnimatablePathValue(IJsonValue json, LottieComposition composition)
        {
            if (HasKeyframes(json))
            {
                var jsonArray = json.GetArray();
                var length = jsonArray.Count;
                for (var i = 0; i < length; i++)
                {
                    var jsonKeyframe = jsonArray[i].GetObject();
                    var keyframe = PathKeyframe.PathKeyframeFactory.NewInstance(jsonKeyframe, composition, ValueFactory.Instance);
                    _keyframes.Add(keyframe);
                }
                Keyframe<Keyframe<Vector2?>>.SetEndFrames<Keyframe<Vector2?>, Vector2?>(_keyframes);
            }
            else
            {
                _keyframes.Add(new Keyframe<Vector2?>(JsonUtils.PointFromJsonArray(json.GetArray(), composition.DpScale)));
            }
        }

        private bool HasKeyframes(IJsonValue json)
        {
            if (json.ValueType != JsonValueType.Array)
                return false;

            var firstObject = json.GetArray()[0];
            return firstObject.ValueType == JsonValueType.Object && firstObject.GetObject().ContainsKey("t");
        }

        public IBaseKeyframeAnimation<Vector2?, Vector2?> CreateAnimation()
        {
            if (_keyframes[0].Static)
            {
                return new PointKeyframeAnimation(_keyframes);
            }

            return new PathKeyframeAnimation(_keyframes.ToList());
        }

        private class ValueFactory : IAnimatableValueFactory<Vector2?>
        {
            internal static readonly IAnimatableValueFactory<Vector2?> Instance = new ValueFactory();

            public Vector2? ValueFromObject(IJsonValue @object, float scale)
            {
                return JsonUtils.PointFromJsonArray(@object.GetArray(), scale);
            }
        }
    }
}