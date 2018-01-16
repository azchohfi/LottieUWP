using System.Numerics;
using LottieUWP.Animation;
using LottieUWP.Animation.Keyframe;
using Newtonsoft.Json;

namespace LottieUWP.Parser
{
    public static class PathKeyframeParser
    {
        public static PathKeyframe Parse(JsonReader reader, LottieComposition composition, IValueParser<Vector2?> valueParser)
        {
            bool animated = reader.Peek() == JsonToken.StartObject;
            Keyframe<Vector2?> keyframe = KeyframeParser.Parse(reader, composition, Utils.Utils.DpScale(), valueParser, animated);

            return new PathKeyframe(composition, keyframe);
        }
    }
}
