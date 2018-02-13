using System.Numerics;
using LottieUWP.Value;
using LottieUWP.Animation.Keyframe;
using Newtonsoft.Json;

namespace LottieUWP.Parser
{
    static class PathKeyframeParser
    {
        internal static PathKeyframe Parse(JsonReader reader, LottieComposition composition)
        {
            bool animated = reader.Peek() == JsonToken.StartObject;
            Keyframe<Vector2?> keyframe = KeyframeParser.Parse(reader, composition, Utils.Utils.DpScale(), PathParser.Instance, animated);

            return new PathKeyframe(composition, keyframe);
        }
    }
}
