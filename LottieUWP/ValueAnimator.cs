using System;
using System.Diagnostics;
using System.Threading;

namespace LottieUWP
{
    public abstract class ValueAnimator : Animator
    {
        protected ValueAnimator()
        {
            _interpolator = new AccelerateDecelerateInterpolator();
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

        private IInterpolator _interpolator;
        private Timer _timer;

        public abstract float FrameRate { get; set; }

        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        public int RepeatCount { get; set; }
        public RepeatMode RepeatMode { get; set; }

        public override bool IsRunning => _timer != null;

        public virtual IInterpolator Interpolator
        {
            get => _interpolator;
            set
            {
                if (value == null)
                    value = new LinearInterpolator();
                _interpolator = value;
            }
        }

        public abstract float AnimatedFraction { get; }

        protected void OnAnimationUpdate()
        {
            Update?.Invoke(this, new ValueAnimatorUpdateEventArgs(this));
        }

        protected void PrivateStart()
        {
            if (_timer == null)
            {
                _timer?.Dispose();
                _timer = new Timer(TimerCallback, null, TimeSpan.Zero, GetTimerInterval());
            }
        }

        protected void UpdateTimerInterval()
        {
            _timer?.Change(TimeSpan.Zero, GetTimerInterval());
        }

        private TimeSpan GetTimerInterval()
        {
            return TimeSpan.FromTicks((long)Math.Floor(TimeSpan.TicksPerSecond / (decimal)FrameRate));
        }

        protected virtual void RemoveFrameCallback()
        {
            _timer?.Dispose();
            _timer = null;
        }

        private void TimerCallback(object state)
        {
            DoFrame();
        }

        public virtual void DoFrame()
        {
            OnValueChanged();
        }

        protected long SystemnanoTime()
        {
            long nano = 10000L * Stopwatch.GetTimestamp();
            nano /= TimeSpan.TicksPerMillisecond;
            nano *= 100L;
            return nano;
        }
    }
}