using LottieUWP.Value;
using System.Collections.Generic;

namespace LottieUWP.Animation.Keyframe
{
    internal class IntegerKeyframeAnimation : KeyframeAnimation<int?>
    {
        internal IntegerKeyframeAnimation(List<Keyframe<int?>> keyframes) : base(keyframes)
        {
        }

        public override int? GetValue(Keyframe<int?> keyframe, float keyframeProgress)
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

            return (int?)MathExt.Lerp(keyframe.StartValue.Value, keyframe.EndValue.Value, keyframeProgress);
        }
    }
}