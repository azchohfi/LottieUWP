using System.Collections.Generic;
using System.Numerics;

namespace LottieUWP.Animation.Keyframe
{
    internal class PointKeyframeAnimation : KeyframeAnimation<Vector2?>
    {
        private Vector2 _point;

        internal PointKeyframeAnimation(List<IKeyframe<Vector2?>> keyframes) : base(keyframes)
        {
        }

        public override Vector2? GetValue(IKeyframe<Vector2?> keyframe, float keyframeProgress)
        {
            if (keyframe.StartValue == null || keyframe.EndValue == null)
            {
                throw new System.InvalidOperationException("Missing values for keyframe.");
            }

            var startPoint = keyframe.StartValue;
            var endPoint = keyframe.EndValue;

            _point.X = startPoint.Value.X + keyframeProgress * (endPoint.Value.X - startPoint.Value.X);
            _point.Y = startPoint.Value.Y + keyframeProgress * (endPoint.Value.Y - startPoint.Value.Y);

            return _point;
        }
    }
}