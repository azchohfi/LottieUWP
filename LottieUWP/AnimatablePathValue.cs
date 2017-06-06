using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;

namespace LottieUWP
{
    internal class AnimatablePathValue : IAnimatableValue<PointF>
    {
        internal static IAnimatableValue<PointF> CreateAnimatablePathOrSplitDimensionPath(JsonObject json, LottieComposition composition)
        {
            if (json.ContainsKey("k"))
            {
                return new AnimatablePathValue(json["k"], composition);
            }
            return new AnimatableSplitDimensionPathValue(AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("x"), composition), AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("y"), composition));
        }

        private readonly List<PathKeyframe> _keyframes = new List<PathKeyframe>();
        private readonly PointF _initialPoint;

        /// <summary>
        /// Create a default static animatable path.
        /// </summary>
        internal AnimatablePathValue()
        {
            _initialPoint = new PointF(0, 0);
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
                Keyframe<PathKeyframe>.SetEndFrames<IKeyframe<PointF>, PointF>(_keyframes.Cast<IKeyframe<PointF>>().ToList());
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

        public virtual IBaseKeyframeAnimation<PointF> CreateAnimation()
        {
            if (!HasAnimation())
            {
                return new StaticKeyframeAnimation<PointF>(_initialPoint);
            }

            return new PathKeyframeAnimation(_keyframes.Cast<IKeyframe<PointF>>().ToList());
        }

        public virtual bool HasAnimation()
        {
            return _keyframes.Count > 0;
        }

        public override string ToString()
        {
            return "initialPoint=" + _initialPoint;
        }

        private class ValueFactory : IAnimatableValueFactory<PointF>
        {
            internal static readonly IAnimatableValueFactory<PointF> Instance = new ValueFactory();

            public virtual PointF ValueFromObject(IJsonValue @object, float scale)
            {
                return JsonUtils.PointFromJsonArray(@object.GetArray(), scale);
            }
        }
    }
}