using System.Collections.Generic;

namespace LottieUWP.Animation.Keyframe
{
    internal class FloatKeyframeAnimation : KeyframeAnimation<float?>
    {
        internal FloatKeyframeAnimation(List<IKeyframe<float?>> keyframes) : base(keyframes)
        {
        }

        public override float? GetValue(IKeyframe<float?> keyframe, float keyframeProgress)
        {
            if (keyframe.StartValue == null || keyframe.EndValue == null)
            {
                throw new System.InvalidOperationException("Missing values for keyframe.");
            }

            if (ValueCallback != null)
            {
                return ValueCallback.GetValue(keyframe.StartFrame.Value, keyframe.EndFrame.Value, keyframe.StartValue, keyframe.EndValue, keyframeProgress, LinearCurrentKeyframeProgress, Progress);
            }

            return MathExt.Lerp(keyframe.StartValue.Value, keyframe.EndValue.Value, keyframeProgress);
        }
    }
}