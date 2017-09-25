using System;

namespace LottieUWP.Utils
{
    /// <summary>
    /// This is a slightly modified <seealso cref="ValueAnimator"/> that allows us to update start and end values
    /// easily optimizing for the fact that we know that it's a value animator with 2 floats.
    /// </summary>
    internal class LottieValueAnimator : ValueAnimator
    {
        private bool _systemAnimationsAreDisabled;
        private bool _isReversed;
        private float _minProgress;
        private float _maxProgress = 1f;
        private long _originalDuration;

        private float _progress;

        internal LottieValueAnimator()
        {
            SetFloatValues(0f, 1f);

            Update += OnAnimationUpdate;
        }

        /*
          This allows us to reset the values if they were temporarily reset by
          UpdateValues(float, float, long, boolean)
        */

        protected override void AnimationEnded()
        {
            base.AnimationEnded();

            UpdateValues(_minProgress, _maxProgress);
        }

        protected override void AnimationCanceled()
        {
            base.AnimationCanceled();

            UpdateValues(_minProgress, _maxProgress);
        }

        private void OnAnimationUpdate(object sender, ValueAnimatorUpdateEventArgs valueAnimatorUpdateEventArgs)
        {
            if (!_systemAnimationsAreDisabled && sender is ValueAnimator animation)
            {
                // On older devices, getAnimatedValue and getAnimatedFraction 
                // will always return 0 if animations are disabled. 
                _progress = animation.AnimatedValue;
            }
        }

        public override void Start()
        {
            if (_systemAnimationsAreDisabled)
            {
                Progress = MaxProgress;
            }
            else
            {
                base.Start();
            }
        }

        public void SystemAnimationsAreDisabled()
        {
            _systemAnimationsAreDisabled = true;
        }

        public override long Duration
        {
            set
            {
                _originalDuration = value;
                UpdateValues(_minProgress, _maxProgress);
            }
        }

        /** 
         * This progress is from 0 to 1 and doesn't take into account setMinProgress or setMaxProgress. 
         * In other words, if you have set the min and max progress to 0.2 and 0.4, setting this to 
         * 0.5f will set the progress to 0.5, not 0.3. However, the value will be clamped between 0.2 and 
         * 0.4 so the resulting progress would be 0.4. 
         */
        public new float Progress
        {
            get => _progress;
            set
            {
                if (_progress == value)
                {
                    return;
                }
                SetProgressInternal(value);
            }
        }

        /// <summary>
        /// Forces the animation to update even if the progress hasn't changed.
        /// </summary>
        public void ForceUpdate()
        {
            SetProgressInternal(Progress);
        }

        private void SetProgressInternal(float progress)
        {
            if (progress < _minProgress)
            {
                progress = _minProgress;
            }
            else if (progress > _maxProgress)
            {
                progress = _maxProgress;
            }
            _progress = progress;
            if (Duration > 0 && !_systemAnimationsAreDisabled)
            {
                float offsetProgress = (progress - _minProgress) / (_maxProgress - _minProgress);
                CurrentPlayTime = (long)(Duration * offsetProgress);
            }
        }

        internal virtual bool IsReversed
        {
            set
            {
                _isReversed = value;
                UpdateValues(_minProgress, _maxProgress);
            }
        }

        public float MinProgress
        {
            set
            {
                _minProgress = value;
                UpdateValues(_minProgress, _maxProgress);
            }
            get => _minProgress;
        }

        internal virtual float MaxProgress
        {
            get => _maxProgress;
            set
            {
                _maxProgress = value;
                UpdateValues(_minProgress, _maxProgress);
            }
        }
        public void ResumeAnimation()
        {
            float startingProgress = Progress;
            Start();
            // This has to call through setCurrentPlayTime for compatibility reasons. 
            Progress = startingProgress;
        }

        /// <summary>
        /// This lets you set the start and end progress for a single play of the animator. After the next
        /// time the animation ends or is cancelled, the values will be reset to those set by
        /// <seealso cref="MinProgress"/> or <seealso cref="MaxProgress"/>.
        /// </summary>
        internal virtual void UpdateValues(float startProgress, float endProgress)
        {
            var minValue = Math.Min(startProgress, endProgress);
            var maxValue = Math.Max(startProgress, endProgress);
            SetFloatValues(_isReversed ? maxValue : minValue, _isReversed ? minValue : maxValue);
            base.Duration = (long)(_originalDuration * (maxValue - minValue));
            Progress = Progress;
        }
    }
}
