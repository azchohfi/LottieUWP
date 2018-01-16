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
    public class AnimatablePathValue : IAnimatableValue<Vector2?, Vector2?>
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
                            xAnimation = AnimatableValueParser.ParseFloat(reader, composition);
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
                            yAnimation = AnimatableValueParser.ParseFloat(reader, composition);
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
        public AnimatablePathValue()
        {
            _keyframes.Add(new Keyframe<Vector2?>(new Vector2(0, 0)));
        }

        public AnimatablePathValue(JsonReader reader, LottieComposition composition)
        {
            if (reader.Peek() == JsonToken.StartArray)
            {
                reader.BeginArray();
                while (reader.HasNext())
                {
                    _keyframes.Add(PathKeyframeParser.Parse(reader, composition, PathParser.Instance));
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
    }
}