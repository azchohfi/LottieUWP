namespace LottieUWP
{
    public abstract class Animator
    {
        public virtual long Duration { get; set; }

        public abstract bool IsRunning { get; }

        protected Animator()
        {
            Duration = 300;
        }

        public virtual void Start()
        {
        }

        public virtual void End()
        {
            Cancel();
        }

        public virtual void Cancel()
        {
            AnimationCanceled();
        }

        protected virtual void AnimationCanceled()
        {
        }
    }
}