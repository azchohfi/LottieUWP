using System.Collections.Generic;

namespace LottieUWP
{

    internal class TextKeyframeAnimation : KeyframeAnimation<DocumentData>
    {
        internal TextKeyframeAnimation(IList<IKeyframe<DocumentData>> keyframes) : base(keyframes)
        {
        }

        public override DocumentData GetValue(IKeyframe<DocumentData> keyframe, float keyframeProgress)
        {
            return keyframe.StartValue;
        }
    }
}
