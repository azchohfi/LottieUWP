namespace LottieUWP
{
    public interface IAnimatable
    {
        bool IsRunning { get; }
        void Start();
        void Stop();
    }
}
