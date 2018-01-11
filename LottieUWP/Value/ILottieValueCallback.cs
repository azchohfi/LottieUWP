using LottieUWP.Animation.Keyframe;

namespace LottieUWP.Value
{
    public interface ILottieValueCallback<T>
    {
        void SetAnimation(IBaseKeyframeAnimation animation);
        T GetValue(
            float startFrame,
            float endFrame,
            T startValue,
            T endValue,
            float linearKeyframeProgress,
            float interpolatedKeyframeProgress,
            float overallProgress);

        T GetValueInternal(
            float startFrame,
            float endFrame,
            T startValue,
            T endValue,
            float linearKeyframeProgress,
            float interpolatedKeyframeProgress,
            float overallProgress
        );
    }
}