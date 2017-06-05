using System.Collections.Generic;
using Windows.UI;

namespace LottieUWP
{
    internal class ColorKeyframeAnimation : KeyframeAnimation<Color>
    {

        internal ColorKeyframeAnimation(IList<IKeyframe<Color>> keyframes) : base(keyframes)
        {
        }

        public override Color GetValue(IKeyframe<Color> keyframe, float keyframeProgress)
        {
            if (keyframe.StartValue == null || keyframe.EndValue == null)
            {
                throw new System.InvalidOperationException("Missing values for keyframe.");
            }
            var startColor = keyframe.StartValue;
            var endColor = keyframe.EndValue;

            return GammaEvaluator.Evaluate(keyframeProgress, startColor, endColor);
        }
    }
}