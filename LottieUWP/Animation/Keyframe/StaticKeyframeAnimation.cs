using System.Collections.Generic;

namespace LottieUWP.Animation.Keyframe
{
    internal class StaticKeyframeAnimation<TK, TA> : BaseKeyframeAnimation<TK, TA>
    {
        private readonly TA _initialValue;

        internal StaticKeyframeAnimation(TA initialValue) : base(new List<IKeyframe<TK>>())
        {
            _initialValue = initialValue;
        }

        public override float Progress
        {
            set => base.Progress = value;
        }

        /// <summary>
        /// If this doesn't return 1, then <see cref="set_Progress"/> will always clamp the progress 
        /// to 0.
        /// </summary>
        protected override float EndProgress => 1f;

        protected override void OnValueChanged()
        {
            if (ValueCallback != null)
            {
                base.OnValueChanged();
            }
        }

        public override TA Value => _initialValue;

        public override TA GetValue(IKeyframe<TK> keyframe, float keyframeProgress)
        {
            if (ValueCallback != null)
            {
                return ValueCallback.GetValue(0f, 0f, _initialValue, _initialValue, Progress, Progress, Progress);
            }
            return _initialValue;
        }
    }
}