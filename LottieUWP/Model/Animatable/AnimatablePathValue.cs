using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LottieUWP.Animation;
using LottieUWP.Animation.Keyframe;
using LottieUWP.Parser;
using LottieUWP.Utils;
using Newtonsoft.Json;

namespace LottieUWP.Model.Animatable
{
    internal class AnimatablePathValue : IAnimatableValue<Vector2?, Vector2?>
    {
        internal static IAnimatableValue<Vector2?, Vector2?> CreateAnimatablePathOrSplitDimensionPath(JsonReader reader, LottieComposition composition)
        {
            AnimatablePathValue pathAnimation = null;
            AnimatableFloatValue xAnimation = null;
            AnimatableFloatValue yAnimation = null;

            var hasExpressions = false;

            reader.BeginObject();
            while (reader.Peek() != JsonToken.EndObject)
            {
                switch (reader.NextName())
                {
                    case "k":
                        pathAnimation = new AnimatablePathValue(reader, composition);
                        break;
                    case "x":
                        if (reader.Peek() == JsonToken.String)
                        {
                            hasExpressions = true;
                            reader.SkipValue();
                        }
                        else
                        {
                            xAnimation = AnimatableFloatValue.Factory.NewInstance(reader, composition);
                        }
                        break;
                    case "y":
                        if (reader.Peek() == JsonToken.String)
                        {
                            hasExpressions = true;
                            reader.SkipValue();
                        }
                        else
                        {
                            yAnimation = AnimatableFloatValue.Factory.NewInstance(reader, composition);
                        }
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }
            reader.EndObject();

            if (hasExpressions)
            {
                composition.AddWarning("Lottie doesn't support expressions.");
            }

            if (pathAnimation != null)
            {
                return pathAnimation;
            }
            return new AnimatableSplitDimensionPathValue(xAnimation, yAnimation);
        }

        private readonly List<Keyframe<Vector2?>> _keyframes = new List<Keyframe<Vector2?>>();

        /// <summary>
        /// Create a default static animatable path.
        /// </summary>
        internal AnimatablePathValue()
        {
            _keyframes.Add(new Keyframe<Vector2?>(new Vector2(0, 0)));
        }

        internal AnimatablePathValue(JsonReader reader, LottieComposition composition)
        {
            if (reader.Peek() == JsonToken.StartArray)
            {
                reader.BeginArray();
                while (reader.HasNext())
                {
                    var keyframe = PathKeyframe.PathKeyframeFactory.NewInstance(reader, composition, ValueFactory.Instance);
                    _keyframes.Add(keyframe);
                }
                reader.EndArray();
                KeyframesParser.SetEndFrames<Keyframe<Vector2?>, Vector2?>(_keyframes);
            }
            else
            {
                _keyframes.Add(new Keyframe<Vector2?>(JsonUtils.JsonToPoint(reader, Utils.Utils.DpScale())));
            }
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

            public Vector2? ValueFromObject(JsonReader reader, float scale)
            {
                return JsonUtils.JsonToPoint(reader, scale);
            }
        }
    }
}