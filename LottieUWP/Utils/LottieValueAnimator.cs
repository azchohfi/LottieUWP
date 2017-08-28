using System;

namespace LottieUWP.Utils
{
    /// <summary>
    /// This is a slightly modified <seealso cref="ValueAnimator"/> that allows us to update start and end values
    /// easily optimizing for the fact that we know that it's a value animator with 2 floats.
    /// </summary>
    internal class LottieValueAnimator : ValueAnimator
    {
        private bool _isReversed;
        private float _startProgress;
        private float _endProgress = 1f;
        private long _duration;

        internal LottieValueAnimator()
        {
            SetFloatValues(0f, 1f);
        }

        /*
          This allows us to reset the values if they were temporarily reset by
          UpdateValues(float, float, long, boolean)
        */

        protected override void AnimationEnded()
        {
            base.AnimationEnded();

            UpdateValues();
        }

        protected override void AnimationCanceled()
        {
            base.AnimationCanceled();

            UpdateValues();
        }

        public override long Duration
        {
            get => _duration;
            set
            {
                _duration = value;
                UpdateValues();
            }
        }

        internal virtual bool IsReversed
        {
            set
            {
                _isReversed = value;
                UpdateValues();
            }
        }

        internal virtual float StartProgress
        {
            set
            {
                _startProgress = value;
                UpdateValues();
            }
        }

        internal virtual float EndProgress
        {
            set
            {
                _endProgress = value;
                UpdateValues();
            }
        }

        /// <summary>
        /// This lets you set the start and end progress for a single play of the animator. After the next
        /// time the animation ends or is cancelled, the values will be reset to those set by
        /// <seealso cref="StartProgress"/> or <seealso cref="EndProgress"/>.
        /// </summary>
        internal virtual void UpdateValues(float startProgress, float endProgress)
        {
            var minValue = Math.Min(startProgress, endProgress);
            var maxValue = Math.Max(startProgress, endProgress);
            SetFloatValues(_isReversed ? maxValue : minValue, _isReversed ? minValue : maxValue);
            base.Duration = (long)(_duration * (maxValue - minValue));
        }

        private void UpdateValues()
        {
            UpdateValues(_startProgress, _endProgress);
        }
    }
}
