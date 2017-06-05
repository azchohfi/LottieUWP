using System.Collections.Generic;

namespace LottieUWP
{
    internal class FloatKeyframeAnimation : KeyframeAnimation<float?>
    {

        internal FloatKeyframeAnimation(IList<IKeyframe<float?>> keyframes) : base(keyframes)
        {
        }

        public override float? GetValue(IKeyframe<float?> keyframe, float keyframeProgress)
        {
            if (keyframe.StartValue == null || keyframe.EndValue == null)
            {
                throw new System.InvalidOperationException("Missing values for keyframe.");
            }
            return MathExt.Lerp(keyframe.StartValue.Value, keyframe.EndValue.Value, keyframeProgress);
        }
    }
}