using System.Collections.Generic;

namespace LottieUWP
{
    internal class PathKeyframeAnimation : KeyframeAnimation<PointF>
    {
        private readonly PointF _point = new PointF();
        private float[] _pos = new float[2];
        private PathKeyframe _pathMeasureKeyframe;
        private PathMeasure _pathMeasure;

        internal PathKeyframeAnimation(IList<IKeyframe<PointF>> keyframes)
            : base(keyframes)
        {
        }

        public override PointF GetValue(IKeyframe<PointF> keyframe, float keyframeProgress)
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

            _pathMeasure.GetPosTan(keyframeProgress * _pathMeasure.Length, ref _pos);
            _point.Set(_pos[0], _pos[1]);
            return _point;
        }
    }
}