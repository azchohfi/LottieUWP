using System;
using System.Collections.Generic;

namespace LottieUWP
{
    public interface IBaseKeyframeAnimation
    {
        float Progress { get; set; }
        event EventHandler ValueChanged;
    }
    public interface IBaseKeyframeAnimation<out TA> : IBaseKeyframeAnimation
    {
        TA Value { get; }
    }

    /// <summary>
    /// </summary>
    /// <typeparam name="TK">Keyframe type</typeparam>
    /// <typeparam name="TA">Animation type</typeparam>
    public abstract class BaseKeyframeAnimation<TK, TA> : IBaseKeyframeAnimation<TA>
    {
        public virtual event EventHandler ValueChanged;
        private bool _isDiscrete;

        private readonly IList<IKeyframe<TK>> _keyframes;
        private float _progress;

        private IKeyframe<TK> _cachedKeyframe;

        internal BaseKeyframeAnimation(IList<IKeyframe<TK>> keyframes)
        {
            _keyframes = keyframes;
        }

        internal virtual void SetIsDiscrete()
        {
            _isDiscrete = true;
        }

        public virtual float Progress
        {
            set
            {
                if (value < 0 || float.IsNaN(value))
                    value = 0;
                if (value > 1)
                    value = 1;

                if (value < StartDelayProgress)
                {
                    value = 0f;
                }
                else if (value > EndProgress)
                {
                    value = 1f;
                }

                if (value == _progress)
                {
                    return;
                }
                _progress = value;

                OnValueChanged();
            }
            get => _progress;
        }

        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        private IKeyframe<TK> CurrentKeyframe
        {
            get
            {
                if (_keyframes.Count == 0)
                {
                    throw new InvalidOperationException("There are no keyframes");
                }

                if (_cachedKeyframe != null && _cachedKeyframe.ContainsProgress(_progress))
                {
                    return _cachedKeyframe;
                }

                var i = 0;
                var keyframe = _keyframes[0];
                if (_progress < keyframe.StartProgress)
                {
                    _cachedKeyframe = keyframe;
                    return keyframe;
                }

                while (!keyframe.ContainsProgress(_progress) && i < _keyframes.Count)
                {
                    keyframe = _keyframes[i];
                    i++;
                }
                _cachedKeyframe = keyframe;
                return keyframe;
            }
        }

        /// <summary>
        /// This wil be [0, 1] unless the interpolator has overshoot in which case getValue() should be
        /// able to handle values outside of that range.
        /// </summary>
        private float CurrentKeyframeProgress
        {
            get
            {
                if (_isDiscrete)
                {
                    return 0f;
                }

                var keyframe = CurrentKeyframe;
                if (keyframe.Static)
                {
                    return 0f;
                }
                var progressIntoFrame = _progress - keyframe.StartProgress;
                var keyframeProgress = keyframe.EndProgress - keyframe.StartProgress;

                return keyframe.Interpolator.GetInterpolation(progressIntoFrame / keyframeProgress);
            }
        }

        private float StartDelayProgress
        {
            get
            {
                var startDelayProgress = _keyframes.Count == 0 ? 0f : _keyframes[0].StartProgress;
                if (startDelayProgress < 0)
                    return 0;
                if (startDelayProgress > 1)
                    return 1;
                return startDelayProgress;
            }
        }

        private float EndProgress
        {
            get
            {
                var endProgress = _keyframes.Count == 0 ? 1f : _keyframes[_keyframes.Count - 1].EndProgress;
                if (endProgress < 0)
                    return 0;
                if (endProgress > 1)
                    return 1;
                return endProgress;
            }
        }

        public virtual TA Value => GetValue(CurrentKeyframe, CurrentKeyframeProgress);

        /// <summary>
        /// keyframeProgress will be [0, 1] unless the interpolator has overshoot in which case, this
        /// should be able to handle values outside of that range.
        /// </summary>
        public abstract TA GetValue(IKeyframe<TK> keyframe, float keyframeProgress);
    }
}