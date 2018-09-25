using System;
using System.Collections.Generic;
using LottieUWP.Value;

namespace LottieUWP.Animation.Keyframe
{
    internal class ScaleKeyframeAnimation : KeyframeAnimation<ScaleXy>
    {
        internal ScaleKeyframeAnimation(List<Keyframe<ScaleXy>> keyframes) : base(keyframes)
        {
        }

        public override ScaleXy GetValue(Keyframe<ScaleXy> keyframe, float keyframeProgress)
        {
            if (keyframe.StartValue == null || keyframe.EndValue == null)
            {
                throw new InvalidOperationException("Missing values for keyframe.");
            }
            var startTransform = keyframe.StartValue;
            var endTransform = keyframe.EndValue;

            if (ValueCallback != null)
            {
                var value = ValueCallback.GetValueInternal(keyframe.StartFrame.Value, keyframe.EndFrame.Value,
                    startTransform, endTransform,
                    keyframeProgress, LinearCurrentKeyframeProgress, Progress);
                if (value != null)
                {
                    return value;
                }
            }

            return new ScaleXy(MathExt.Lerp(startTransform.ScaleX, endTransform.ScaleX, keyframeProgress), MathExt.Lerp(startTransform.ScaleY, endTransform.ScaleY, keyframeProgress));
        }
    }
}