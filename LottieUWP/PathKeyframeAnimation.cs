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
            PathKeyframe pathKeyframe = (PathKeyframe) keyframe;
            Path path = pathKeyframe.Path;
            if (path == null)
            {
                return keyframe.StartValue;
            }

            if (_pathMeasureKeyframe != pathKeyframe)
            {
                _pathMeasure = new PathMeasure(path, false);
                _pathMeasureKeyframe = pathKeyframe;
            }

            _pathMeasure.GetPosTan(keyframeProgress * _pathMeasure.Length, out _pos);
            _point.Set(_pos[0], _pos[1]);
            return _point;
        }
    }
}