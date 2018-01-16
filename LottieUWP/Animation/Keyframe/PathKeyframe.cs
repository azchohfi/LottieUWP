using System.Numerics;
using LottieUWP.Parser;
using Newtonsoft.Json;

namespace LottieUWP.Animation.Keyframe
{
    internal class PathKeyframe : Keyframe<Vector2?>
    {
        private PathKeyframe(LottieComposition composition, Keyframe<Vector2?> keyframe)
            : base(composition, keyframe.StartValue, keyframe.EndValue, keyframe.Interpolator, keyframe.StartFrame, keyframe.EndFrame)
        {
            var equals = EndValue != null && StartValue != null && StartValue.Equals(EndValue.Value);
            if (EndValue != null && !equals)
            {
                Path = Utils.Utils.CreatePath(StartValue.Value, EndValue.Value, keyframe.PathCp1, keyframe.PathCp2);
            }
        }

        internal static class PathKeyframeFactory
        {
            internal static PathKeyframe NewInstance(JsonReader reader, LottieComposition composition, IValueParser<Vector2?> valueParser)
            {
                bool animated = reader.Peek() == JsonToken.StartObject;
                var keyframe = KeyframeParser.Parse(reader, composition, Utils.Utils.DpScale(), valueParser, animated);
                return new PathKeyframe(composition, keyframe);
            }
        }

        /// <summary>
        /// This will be null if the startValue and endValue are the same.
        /// </summary>
        internal Path Path { get; }
    }
}