using System.Collections.Generic;
using LottieUWP.Model;

namespace LottieUWP.Animation.Keyframe
{

    internal class TextKeyframeAnimation : KeyframeAnimation<DocumentData>
    {
        internal TextKeyframeAnimation(List<IKeyframe<DocumentData>> keyframes) : base(keyframes)
        {
        }

        public override DocumentData GetValue(IKeyframe<DocumentData> keyframe, float keyframeProgress)
        {
            return keyframe.StartValue;
        }
    }
}
