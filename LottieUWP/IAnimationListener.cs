namespace LottieUWP
{
    public static class BaseKeyframeAnimation
    {
        public interface IAnimationListener
        {
            void OnValueChanged();
        }
    }
}