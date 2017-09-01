using System;
using System.Collections.Generic;

namespace LottieUWP.Animation.Keyframe
{
    internal interface IStaticKeyFrameAnimation
    {
    }

    internal class StaticKeyframeAnimation<TK, TA> : BaseKeyframeAnimation<TK, TA>, IStaticKeyFrameAnimation
    {
        private readonly TA _initialValue;

        internal StaticKeyframeAnimation(TA initialValue) : base(new List<IKeyframe<TK>>())
        {
            _initialValue = initialValue;
        }

        public override float Progress
        {
            set
            {
                // Do nothing
            }
        }


        public override event EventHandler ValueChanged
        {
            add
            {
                // Do nothing. 
            }
            remove
            {
                // Do nothing. 
            }
        }

        public override TA Value => _initialValue;

        public override TA GetValue(IKeyframe<TK> keyframe, float keyframeProgress)
        {
            return _initialValue;
        }
    }
}