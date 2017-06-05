using System;
using System.Collections.Generic;
using Windows.UI.Xaml;

namespace LottieUWP
{
    public abstract partial class Animator
    {
        private readonly List<IAnimatorListener> _listeners = new List<IAnimatorListener>();
        private readonly DispatcherTimer _timer;
        private bool _isReverse;

        public bool Loop { get; set; }
        public long Duration { get; set; }
        public long CurrentPlayTime { get; set; }
        public bool Running => _timer != null;
        public float Progress => CurrentPlayTime / (float) Duration;

        protected Animator()
        {
            Duration = 300;
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            _timer.Tick += TimerCallback;
        }

        public virtual void Start()
        {
            _timer.Start();
        }

        public void Cancel()
        {
            _timer.Stop();
        }

        public void AddListener(IAnimatorListener listener)
        {
            _listeners.Add(listener);
        }

        public void RemoveListener(IAnimatorListener listener)
        {
            _listeners.Remove(listener);
        }

        protected virtual void TimerCallback(object sender, object e)
        {
            CurrentPlayTime += 16 * (_isReverse ? -1 : 1);

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