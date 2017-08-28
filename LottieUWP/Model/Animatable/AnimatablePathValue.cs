using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.Data.Json;
using LottieUWP.Animation;
using LottieUWP.Animation.Keyframe;
using LottieUWP.Utils;

namespace LottieUWP.Model.Animatable
{
    internal class AnimatablePathValue : IAnimatableValue<Vector2?>
    {
        internal static IAnimatableValue<Vector2?> CreateAnimatablePathOrSplitDimensionPath(JsonObject json, LottieComposition composition)
        {
            if (json.ContainsKey("k"))
            {
                return new AnimatablePathValue(json["k"], composition);
            }
            return new AnimatableSplitDimensionPathValue(AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("x"), composition), AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("y"), composition));
        }

        private readonly List<PathKeyframe> _keyframes = new List<PathKeyframe>();
        private readonly Vector2 _initialPoint;

        /// <summary>
        /// Create a default static animatable path.
        /// </summary>
        internal AnimatablePathValue()
        {
            _initialPoint = new Vector2(0, 0);
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
                Keyframe<PathKeyframe>.SetEndFrames<PathKeyframe, Vector2?>(_keyframes);
            }
            else
            {
                _initialPoint = JsonUtils.PointFromJsonArray(json.GetArray(), composition.DpScale);
            }
        }

        private bool HasKeyframes(IJsonValue json)
        {
            if (json.ValueType != JsonValueType.Array)
                return false;

            var firstObject = json.GetArray()[0];
            return firstObject.ValueType == JsonValueType.Object && firstObject.GetObject().ContainsKey("t");
        }

        public IBaseKeyframeAnimation<Vector2?> CreateAnimation()
        {
            if (!HasAnimation())
            {
                return new StaticKeyframeAnimation<Vector2?>(_initialPoint);
            }

            return new PathKeyframeAnimation(_keyframes.Cast<IKeyframe<Vector2?>>().ToList());
        }

        public bool HasAnimation()
        {
            return _keyframes.Count > 0;
        }

        public override string ToString()
        {
            return "initialPoint=" + _initialPoint;
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