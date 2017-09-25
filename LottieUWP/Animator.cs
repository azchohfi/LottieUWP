using System;
using System.Diagnostics;
using System.Threading;

namespace LottieUWP
{
    public abstract class Animator
    {
        public event EventHandler ValueChanged;
        private Timer _timer;
        private readonly TimeSpan _timerInterval;
        private DateTime _lastTick;
        private bool _isReverse;
        private float _currentPlayTime;
        private const int TargetFps = 60;

        public bool Loop { get; set; }
        public virtual long Duration { get; set; }

        public float CurrentPlayTime
        {
            get => _currentPlayTime;
            set
            {
                _currentPlayTime = value;
                OnValueChanged();
            }
        }

        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool IsRunning => _timer != null;
        public float Progress
        {
            get => CurrentPlayTime / Duration;
            set => CurrentPlayTime = Duration * value;
        }

        protected Animator()
        {
            Duration = 300;
            _timerInterval = TimeSpan.FromMilliseconds(Math.Floor(1000.0 / TargetFps));
        }

        public virtual void Start()
        {
            _lastTick = DateTime.Now;

            _timer?.Dispose();
            _timer = new Timer(TimerCallback, null, TimeSpan.Zero, _timerInterval);
        }

        public void End()
        {
            CurrentPlayTime = Duration;
            Cancel();
        }

        public void Cancel()
        {
            PrivateCancel();
            AnimationCanceled();
        }

        private void PrivateCancel()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        protected virtual void TimerCallback(object state)
        {
            var tick = (float)(DateTime.Now - _lastTick).TotalMilliseconds;
            if (tick < _timerInterval.TotalMilliseconds)
                tick = (float)Math.Floor(1000.0 / TargetFps);
            _lastTick = DateTime.Now;

            Debug.WriteLine($"Tick milliseconds: {tick}");

            CurrentPlayTime += tick * (_isReverse ? -1 : 1);

            if (Progress > 1)
            {
                if (Loop)
                {
                    CurrentPlayTime = 0;
                    Start();
                }
                else
                {
                    AnimationEnded();
                    PrivateCancel();
                }
            }
        }

        protected virtual void AnimationEnded()
        {
        }

        protected virtual void AnimationCanceled()
        {
        }

        public void Reverse()
        {
            _isReverse = !_isReverse;
        }
    }
}