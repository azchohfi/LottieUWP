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
        private long _compositionDuration;
        private float _speed = 1f;

        private float _value;
        private float _minValue;
        private float _maxValue = 1f;

        internal LottieValueAnimator()
        {
            Interpolator = null;
            Update += OnAnimationUpdate;
            UpdateValues();
        }

        private void OnAnimationUpdate(object sender, ValueAnimatorUpdateEventArgs valueAnimatorUpdateEventArgs)
        {
            // On older devices, getAnimatedValue and getAnimatedFraction 
            // will always return 0 if animations are disabled. 
            if (!_systemAnimationsAreDisabled && sender is ValueAnimator animation)
            {
                _value = animation.AnimatedValue;
            }
        }

        public void SystemAnimationsAreDisabled()
        {
            _systemAnimationsAreDisabled = true;
        }

        public long CompositionDuration
        {
            set
            {
                _compositionDuration = value;
                UpdateValues();
            }
        }

        /// <summary>
        /// Sets the current animator value. This will update the play time as well. 
        /// It will also be clamped to the values set with {@link #setMinValue(float)} and 
        /// <see cref="MaxValue"/>
        /// </summary>
        public float Value
        {
            set
            {
                _value = MiscUtils.Clamp(value, _minValue, _maxValue);
                _value = value;
                float distFromStart = IsReversed ? _maxValue - value : value - _minValue;
                float range = Math.Abs(_maxValue - _minValue);
                float animatedPercentage = distFromStart / range;
                if (Duration > 0)
                {
                    CurrentPlayTime = (long)(Duration * animatedPercentage);
                }
            }
            get => _value;
        }

        public float MinValue
        {
            set
            {
                if (value < 0)
                    value = 0;
                if (value > 1)
                    value = 1;
                if (value >= _maxValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(value),
                        "Min value must be smaller then max value.");
                }
                _minValue = value;
                UpdateValues();
            }
            get => _minValue;
        }

        public float MaxValue
        {
            set
            {
                if (value < 0)
                    value = 0;
                if (value > 1)
                    value = 1;
                if (value <= _minValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Max value must be greater than min value.");
                }
                _maxValue = value;
                UpdateValues();
            }
        }

        public void ReverseAnimationSpeed()
        {
            Speed = -Speed;
        }

        public float Speed
        {
            set
            {
                _speed = value;
                UpdateValues();
            }
            get => _speed;
        }

        public void PlayAnimation()
        {
            Start();
            Value = IsReversed ? _maxValue : _minValue;
        }

        public void PauseAnimation()
        {
            float value = _value;
            Cancel();
            Value = value;
        }

        public void ResumeAnimation()
        {
            float value = _value;
            if (IsReversed && _value == _minValue)
            {
                value = _maxValue;
            }
            else if (!IsReversed && _value == _maxValue)
            {
                value = _minValue;
            }
            Start();
            Value = value;
        }

        private bool IsReversed => _speed < 0;

        /// <summary>
        /// Update the float values of the animator, scales the duration for the current min/max range
        /// and updates the play time so that it matches the new min/max range.
        /// </summary>
        private void UpdateValues()
        {
            Duration = (long)(_compositionDuration * (_maxValue - _minValue) / Math.Abs(_speed));
            SetFloatValues(_speed < 0 ? _maxValue : _minValue, _speed < 0 ? _minValue : _maxValue);
            // This will force the play time to be correct for the current value.
            Value = _value;
        }
    }
}
