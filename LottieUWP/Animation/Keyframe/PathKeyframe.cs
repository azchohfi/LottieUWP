using System.Numerics;
using Windows.Data.Json;
using LottieUWP.Utils;

namespace LottieUWP.Animation.Keyframe
{
    internal class PathKeyframe : Keyframe<Vector2?>
    {
        private Path _path;

        private PathKeyframe(LottieComposition composition, Vector2? startValue, Vector2? endValue, IInterpolator interpolator, float? startFrame, float? endFrame) : base(composition, startValue, endValue, interpolator, startFrame, endFrame)
        {
        }

        internal static class PathKeyframeFactory
        {
            internal static PathKeyframe NewInstance(JsonObject json, LottieComposition composition, IAnimatableValueFactory<Vector2?> valueFactory)
            {
                var keyframe = KeyFrameFactory.NewInstance(json, composition, composition.DpScale, valueFactory);
                Vector2? cp1 = null;
                Vector2? cp2 = null;
                var tiJson = json.GetNamedArray("ti", null);
                var toJson = json.GetNamedArray("to", null);
                if (tiJson != null && toJson != null)
                {
                    cp1 = JsonUtils.PointFromJsonArray(toJson, composition.DpScale);
                    cp2 = JsonUtils.PointFromJsonArray(tiJson, composition.DpScale);
                }

                var pathKeyframe = new PathKeyframe(composition, keyframe.StartValue, keyframe.EndValue, keyframe.Interpolator, keyframe.StartFrame, keyframe.EndFrame);

                var equals = keyframe.EndValue != null && keyframe.StartValue != null && keyframe.StartValue.Equals(keyframe.EndValue);

                if (pathKeyframe.EndValue != null && !equals)
                {
                    pathKeyframe._path = Utils.Utils.CreatePath(keyframe.StartValue.Value, keyframe.EndValue.Value, cp1, cp2);
                }
                return pathKeyframe;
            }
        }

        /// <summary>
        /// This will be null if the startValue and endValue are the same.
        /// </summary>
        internal Path Path => _path;
    }
}