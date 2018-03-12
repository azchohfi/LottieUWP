using System;
using System.Diagnostics;

namespace LottieUWP.Utils
{
    /// <summary>
    /// This is a slightly modified <seealso cref="ValueAnimator"/> that allows us to update start and end values
    /// easily optimizing for the fact that we know that it's a value animator with 2 floats.
    /// </summary>
    public class LottieValueAnimator : BaseLottieAnimator
    {
        private float _speed = 1f;
        private long _lastFrameTimeNs;
        private float _frame;
        private int _repeatCount;
        private float _minFrame = int.MinValue;
        private float _maxFrame = int.MaxValue;
        private LottieComposition _composition;
        private float _frameRate;
        protected bool _isRunning;
        
        /// <summary>
        /// Returns a float representing the current value of the animation from 0 to 1
        /// regardless of the animation speed, direction, or min and max frames.
        /// </summary>
        public float AnimatedValue => AnimatedValueAbsolute;

        /// <summary>
        /// Returns the current value of the animation from 0 to 1 regardless
        /// of the animation speed, direction, or min and max frames.
        /// </summary>
        public float AnimatedValueAbsolute
        {
            get
            {
                if (_composition == null)
                {
                    return 0;
                }
                return (_frame - _composition.StartFrame) / (_composition.EndFrame - _composition.StartFrame);
            }
        }

        /// <summary>
        /// Returns the current value of the currently playing animation taking into
        /// account direction, min and max frames.
        /// </summary>
        public override float AnimatedFraction
        {
            get
            {
                if (_composition == null)
                {
                    return 0;
                }
                if (IsReversed)
                {
                    return (MaxFrame - _frame) / (MaxFrame - MinFrame);
                }
                else
                {
                    return (_frame - MinFrame) / (MaxFrame - MinFrame);
                }
            }
        }

        public override long Duration => _composition == null ? 0 : (long)_composition.Duration;

        public float Frame
        {
            get => _frame;
            set
            {
                if (_frame == value)
                {
                    return;
                }
                _frame = MiscUtils.Clamp(value, MinFrame, MaxFrame);
                _lastFrameTimeNs = SystemnanoTime();
                OnAnimationUpdate();
            }
        }

        public override bool IsRunning => _isRunning;

        public override void DoFrame()
        {
            base.DoFrame();
            PostFrameCallback();
            if (_composition == null || !IsRunning)
            {
                return;
            }

            long now = SystemnanoTime();
            long timeSinceFrame = now - _lastFrameTimeNs;
            float frameDuration = FrameDurationNs;
            float dFrames = timeSinceFrame / frameDuration;

            _frame += IsReversed ? -dFrames : dFrames;
            bool ended = !MiscUtils.Contains(_frame, MinFrame, MaxFrame);
            _frame = MiscUtils.Clamp(_frame, MinFrame, MaxFrame);

            _lastFrameTimeNs = now;

            Debug.WriteLineIf(LottieLog.TraceEnabled, $"Tick milliseconds: {timeSinceFrame}", LottieLog.Tag);

            OnAnimationUpdate();
            if (ended)
            {
                if (RepeatCount != LottieDrawable.Infinite && _repeatCount >= RepeatCount)
                {
                    _frame = MaxFrame;
                    OnAnimationEnd(IsReversed);
                    RemoveFrameCallback();
                }
                else
                {
                    OnAnimationRepeat();
                    _repeatCount++;
                    if (RepeatMode == RepeatMode.Reverse)
                    {
                        ReverseAnimationSpeed();
                    }
                    else
                    {
                        _frame = MinFrame;
                    }
                    _lastFrameTimeNs = now;
                }
            }

            VerifyFrame();
        }

        private float FrameDurationNs
        {
            get
            {
                if (_composition == null)
                {
                    return float.MaxValue;
                }
                return Utils.SecondInNanos / _composition.FrameRate / Math.Abs(_speed);
            }
        }

        public override float FrameRate
        {
            get => _frameRate;
            set
            {
                _frameRate = value <= 1000 ? (value > 1 ? value : 1) : 1000;
                UpdateTimerInterval();
            }
        }

        public LottieComposition Composition
        {
            set
            {
                _composition = value;
                SetMinAndMaxFrames(_composition.StartFrame, _composition.EndFrame);
                FrameRate = _composition.FrameRate;
                Frame = _frame;
                _lastFrameTimeNs = SystemnanoTime();
            }
        }

        public float MinFrame
        {
            get
            {
                if (_composition == null)
                {
                    return 0;
                }
                return _minFrame == int.MinValue ? _composition.StartFrame : _minFrame;
            }
            set
            {
                SetMinAndMaxFrames(value, _maxFrame);
            }
        }

        public float MaxFrame
        {
            get
            {
                if (_composition == null)
                {
                    return 0;
                }
                return _maxFrame == int.MaxValue ? _composition.EndFrame : _maxFrame;
            }
            set
            {
                SetMinAndMaxFrames(_minFrame, value);
            }
        }

        public void SetMinAndMaxFrames(float minFrame, float maxFrame)
        {
            _minFrame = minFrame;
            _maxFrame = maxFrame;
            Frame = MiscUtils.Clamp(_frame, minFrame, maxFrame);
        }

        public void ReverseAnimationSpeed()
        {
            Speed = -Speed;
        }

        public float Speed
        {
            set => _speed = value;
            get => _speed;
        }

        public void PlayAnimation()
        {
            OnAnimationStart(IsReversed);
            Frame = IsReversed ? MaxFrame : MinFrame;
            _lastFrameTimeNs = SystemnanoTime();
            _repeatCount = 0;
            PostFrameCallback();
        }

        public void EndAnimation()
        {
            RemoveFrameCallback();
            OnAnimationEnd(IsReversed);
        }

        public void PauseAnimation()
        {
            RemoveFrameCallback();
        }

        public void ResumeAnimation()
        {
            PostFrameCallback();
            _lastFrameTimeNs = SystemnanoTime();
            if (IsReversed && Frame == MinFrame)
            {
                _frame = MaxFrame;
            }
            else if (!IsReversed && Frame == MaxFrame)
            {
                _frame = MinFrame;
            }
        }

        public override void Cancel()
        {
            OnAnimationCancel();
            RemoveFrameCallback();
        }

        private bool IsReversed => _speed < 0;

        protected virtual void PostFrameCallback()
        {
            PrivateStart();
            _isRunning = true;
        }

        protected override void RemoveFrameCallback()
        {
            base.RemoveFrameCallback();
            _isRunning = false;
        }

        private void VerifyFrame()
        {
            if (_composition == null)
            {
                return;
            }
            if (_frame < _minFrame || _frame > _maxFrame)
            {
                throw new InvalidOperationException($"Frame must be [{_minFrame},{_maxFrame}]. It is {_frame}");
            }
        }
    }
}
