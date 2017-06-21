using System;

namespace LottieUWP
{
    public class ValueAnimator : Animator
    {
        private ValueAnimator()
        {
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

        public event EventHandler<ValueAnimatorUpdateEventArgs> Update;

        private float _floatValue1;
        private float _floatValue2;
        private float _animatedValue;

        public IInterpolator Interpolator { get; set; }

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
            AnimatedValue = MathExt.Lerp(_floatValue1, _floatValue2, Interpolator.GetInterpolation(0));

            base.Start();
        }

        void OnAnimationUpdate()
        {
            Update?.Invoke(this, new ValueAnimatorUpdateEventArgs(this));
        }

        protected override void TimerCallback(object sender, object e)
        {
            base.TimerCallback(sender, e);

            AnimatedValue = MathExt.Lerp(_floatValue1, _floatValue2, Interpolator.GetInterpolation(Progress));
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