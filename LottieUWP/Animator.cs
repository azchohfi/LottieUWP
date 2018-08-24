namespace LottieUWP
{
    public abstract class Animator
    {
        public virtual long Duration { get; set; }

        public abstract bool IsRunning { get; }

        protected Animator()
        {
        }

        public virtual void Cancel()
        {
            AnimationCanceled();
        }

        protected void AnimationCanceled()
        {
        }
    }
}