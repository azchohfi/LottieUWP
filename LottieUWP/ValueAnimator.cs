using System.Collections.Generic;

namespace LottieUWP
{
    public class ValueAnimator : Animator
    {
        private ValueAnimator()
        {
        }

        readonly List<LottieDrawable.IValueAnimatorAnimatorUpdateListener> _updateListeners = new List<LottieDrawable.IValueAnimatorAnimatorUpdateListener>();

        public void AddUpdateListener(LottieDrawable.IValueAnimatorAnimatorUpdateListener updateListener)
        {
            _updateListeners.Add(updateListener);
        }

        public void RemoveUpdateListener(LottieDrawable.IValueAnimatorAnimatorUpdateListener updateListener)
        {
            _updateListeners.Remove(updateListener);
        }

        private float _floatValue1;
        private float _floatValue2;

        public IInterpolator Interpolator { get; set; }
        public float AnimatedValue { get; private set; }

        public override void Start()
        {
            AnimatedValue = MathExt.Lerp(_floatValue1, _floatValue2, Interpolator.GetInterpolation(0));

            OnAnimationUpdate();

            base.Start();
        }

        void OnAnimationUpdate()
        {
            foreach (var listener in _updateListeners)
            {
                listener.OnAnimationUpdate(this);
            }
        }

        protected override void TimerCallback(object sender, object e)
        {
            base.TimerCallback(sender, e);

            AnimatedValue = MathExt.Lerp(_floatValue1, _floatValue2, Interpolator.GetInterpolation(Progress));

            OnAnimationUpdate();
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