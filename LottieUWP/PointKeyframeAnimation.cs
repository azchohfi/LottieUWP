using System.Collections.Generic;

namespace LottieUWP
{
    internal class PointKeyframeAnimation : KeyframeAnimation<PointF>
    {
        private readonly PointF _point = new PointF();

        internal PointKeyframeAnimation(IList<IKeyframe<PointF>> keyframes) : base(keyframes)
        {
        }

        public override PointF GetValue(IKeyframe<PointF> keyframe, float keyframeProgress)
        {
            if (keyframe.StartValue == null || keyframe.EndValue == null)
            {
                throw new System.InvalidOperationException("Missing values for keyframe.");
            }

            var startPoint = keyframe.StartValue;
            var endPoint = keyframe.EndValue;

            _point.Set(startPoint.X + keyframeProgress * (endPoint.X - startPoint.X), startPoint.Y + keyframeProgress * (endPoint.Y - startPoint.Y));
            return _point;
        }
    }
}