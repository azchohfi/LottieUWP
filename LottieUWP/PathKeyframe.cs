using Windows.Data.Json;

namespace LottieUWP
{
    internal class PathKeyframe : Keyframe<PointF>
    {
        private Path _path;

        private PathKeyframe(LottieComposition composition, PointF startValue, PointF endValue, IInterpolator interpolator, float? startFrame, float? endFrame) : base(composition, startValue, endValue, interpolator, startFrame, endFrame)
        {
        }

        internal class PathKeyframeFactory
        {
            internal static PathKeyframe NewInstance(JsonObject json, LottieComposition composition, IAnimatableValueFactory<PointF> valueFactory)
            {
                var keyframe = KeyFrameFactory.NewInstance(json, composition, composition.DpScale, valueFactory);
                PointF cp1 = null;
                PointF cp2 = null;
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
                    pathKeyframe._path = Utils.CreatePath(keyframe.StartValue, keyframe.EndValue, cp1, cp2);
                }
                return pathKeyframe;
            }
        }

        /// <summary>
        /// This will be null if the startValue and endValue are the same.
        /// </summary>
        internal virtual Path Path => _path;
    }
}