using System;
using System.Diagnostics;
using System.Threading;

namespace LottieUWP
{
    public class ValueAnimator : Animator
    {
        protected ValueAnimator()
        {
            _timerInterval = TimeSpan.FromMilliseconds(Math.Floor(1000.0 / TargetFps));
            Interpolator = new AccelerateDecelerateInterpolator();
        }

        public class ValueAnimatorUpdateEventArgs : EventArgs
        {
            public ValueAnimator Animation { get; }

            public ValueAnimatorUpdateEventArgs(ValueAnimator animation)
            {
                Animation = animation;
            }
        }

        public event EventHandler ValueChanged;
        public event EventHandler<ValueAnimatorUpdateEventArgs> Update;

        public void RemoveAllUpdateListeners()
        {
            Update = null;
        }

        public void RemoveAllListeners()
        {
            ValueChanged = null;
        }
        
        private float _floatValue1;
        private float _floatValue2;
        private float _animatedValue;
        private IInterpolator _interpolator;
        private DateTime _lastTick;
        private const int TargetFps = 60;
        private readonly TimeSpan _timerInterval;
        private Timer _timer;
        private float _currentPlayTime;
        private int _repeatedTimes;
        private bool _reverse;

        public float CurrentPlayTime
        {
            get => _currentPlayTime;
            set
            {
                _currentPlayTime = value;
                UpdateAnimatedValues(CurrentPlayTime / Duration, false);
                OnValueChanged();
            }
        }

        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        public int RepeatCount { get; set; } = LottieDrawable.Infinite;
        public RepeatMode RepeatMode { get; set; }

        public override bool IsRunning => _timer != null;

        public IInterpolator Interpolator
        {
            get => _interpolator;
            set
            {
                if (value == null)
                    value = new LinearInterpolator();
                _interpolator = value;
            }
        }

        public float AnimatedFraction { get; private set; }

        public float AnimatedValue
        {
            get => _animatedValue;
            private set
            {
                _animatedValue = value;
                OnAnimationUpdate();
            }
        }

        public override void Start()
        {
            _repeatedTimes = 0;
            _reverse = false;

            PrivateStart(true);

            base.Start();
        }

        private void PrivateStart(bool recreateTimer)
        {
            _lastTick = DateTime.Now;

            UpdateAnimatedValues(0);

            if (recreateTimer || _timer == null)
            {
                _timer?.Dispose();
                _timer = new Timer(TimerCallback, null, TimeSpan.Zero, _timerInterval);
            }
        }

        void OnAnimationUpdate()
        {
            Update?.Invoke(this, new ValueAnimatorUpdateEventArgs(this));
        }

        private void TimerCallback(object state)
        {
            var tick = (float)(DateTime.Now - _lastTick).TotalMilliseconds;
            if (tick < _timerInterval.TotalMilliseconds)
                tick = (float)Math.Floor(1000.0 / TargetFps);
            _lastTick = DateTime.Now;

            Debug.WriteLine($"Tick milliseconds: {tick}");

            CurrentPlayTime += tick;

            var progress = CurrentPlayTime / Duration;

            if (progress > 1)
            {
                if ((RepeatCount > 0 && _repeatedTimes < RepeatCount) || RepeatCount == LottieDrawable.Infinite)
                {
                    CurrentPlayTime = 0;
                    progress = 0;
                    _repeatedTimes++;

                    if (RepeatMode == RepeatMode.Reverse)
                    {
                        _reverse = !_reverse;
                    }

                    PrivateStart(false);
                }
                else
                {
                    PrivateCancel();
                }
            }

            UpdateAnimatedValues(progress);
        }

        private void UpdateAnimatedValues(float progress, bool notify = true)
        {
            AnimatedFraction = Interpolator.GetInterpolation(_reverse ? 1 - progress : progress);
            var animatedValue = MathExt.Lerp(_floatValue1, _floatValue2, AnimatedFraction);
            if (notify)
                AnimatedValue = animatedValue;
            else
                _animatedValue = animatedValue;
        }

        public override void End()
        {
            CurrentPlayTime = Duration;
            base.End();
        }

        public override void Cancel()
        {
            PrivateCancel();
            base.Cancel();
        }

        private void PrivateCancel()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        public void SetFloatValues(float floatValue1, float floatValue2)
        {
            _floatValue1 = floatValue1;
            _floatValue2 = floatValue2;
        }

        public static ValueAnimator OfFloat(float floatValue1, float floatValue2)
        {
            return new ValueAnimator
            {
                _floatValue1 = floatValue1,
                _floatValue2 = floatValue2
            };
        }
    }
}