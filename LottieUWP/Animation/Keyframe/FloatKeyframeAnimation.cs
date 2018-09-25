using LottieUWP.Value;
using System.Collections.Generic;

namespace LottieUWP.Animation.Keyframe
{
    internal class FloatKeyframeAnimation : KeyframeAnimation<float?>
    {
        internal FloatKeyframeAnimation(List<Keyframe<float?>> keyframes) : base(keyframes)
        {
        }

        public override float? GetValue(Keyframe<float?> keyframe, float keyframeProgress)
        {
            if (keyframe.StartValue == null || keyframe.EndValue == null)
            {
                throw new System.InvalidOperationException("Missing values for keyframe.");
            }

            if (ValueCallback != null)
            {
                var value = ValueCallback.GetValueInternal(keyframe.StartFrame.Value, keyframe.EndFrame.Value, keyframe.StartValue, keyframe.EndValue, keyframeProgress, LinearCurrentKeyframeProgress, Progress);
                if (value != null)
                {
                    return value;
                }
            }

            return MathExt.Lerp(keyframe.StartValue.Value, keyframe.EndValue.Value, keyframeProgress);
        }
    }
}