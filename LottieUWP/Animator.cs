using System;
using System.Diagnostics;
using Windows.UI.Xaml;

namespace LottieUWP
{
    public abstract class Animator
    {
        public event EventHandler ValueChanged;
        private readonly DispatcherTimer _timer;
        private DateTime _lastTick;
        private bool _isReverse;
        private float _currentPlayTime;
        private const int TargetFps = 60;

        public bool Loop { get; set; }
        public long Duration { get; set; }

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
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Math.Floor(1000.0 / TargetFps))
            };
            _timer.Tick += TimerCallback;
        }

        public virtual void Start()
        {
            _lastTick = DateTime.Now;
            _timer.Start();
        }

        public void Cancel()
        {
            _timer.Stop();
        }

        protected virtual void TimerCallback(object sender, object e)
        {
            var tick = (float)(DateTime.Now - _lastTick).TotalMilliseconds;
            if (tick < _timer.Interval.TotalMilliseconds)
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
                    Cancel();
                }
            }
        }

        public void Reverse()
        {
            _isReverse = !_isReverse;
        }
    }
}