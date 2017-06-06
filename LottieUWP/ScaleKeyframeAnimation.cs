using System;
using System.Collections.Generic;

namespace LottieUWP
{
    internal class ScaleKeyframeAnimation : KeyframeAnimation<ScaleXy>
    {
        internal ScaleKeyframeAnimation(IList<IKeyframe<ScaleXy>> keyframes) : base(keyframes)
        {
        }

        public override ScaleXy GetValue(IKeyframe<ScaleXy> keyframe, float keyframeProgress)
        {
            if (keyframe.StartValue == null || keyframe.EndValue == null)
            {
                throw new InvalidOperationException("Missing values for keyframe.");
            }
            var startTransform = keyframe.StartValue;
            var endTransform = keyframe.EndValue;
            return new ScaleXy(MathExt.Lerp(startTransform.ScaleX, endTransform.ScaleX, keyframeProgress), MathExt.Lerp(startTransform.ScaleY, endTransform.ScaleY, keyframeProgress));
        }
    }
}