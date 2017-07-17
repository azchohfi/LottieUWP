using System.Collections.Generic;
using System.Numerics;

namespace LottieUWP
{
    internal class PathKeyframeAnimation : KeyframeAnimation<Vector2?>
    {
        private PathKeyframe _pathMeasureKeyframe;
        private PathMeasure _pathMeasure;

        internal PathKeyframeAnimation(List<IKeyframe<Vector2?>> keyframes)
            : base(keyframes)
        {
        }

        public override Vector2? GetValue(IKeyframe<Vector2?> keyframe, float keyframeProgress)
        {
            var pathKeyframe = (PathKeyframe) keyframe;
            var path = pathKeyframe.Path;
            if (path == null || path.Contours.Count == 0)
            {
                return keyframe.StartValue;
            }

            if (_pathMeasureKeyframe != pathKeyframe)
            {
                _pathMeasure = new PathMeasure(path);
                _pathMeasureKeyframe = pathKeyframe;
            }

            return _pathMeasure.GetPosTan(keyframeProgress * _pathMeasure.Length);
        }
    }
}