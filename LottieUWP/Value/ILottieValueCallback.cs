namespace LottieUWP.Value
{
    /// <summary>
    /// Allows you to set a callback on a resolved <see cref="Model.KeyPath"/> to modify its animation values at runtime. 
    /// This API is not ready for public use yet. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ILottieValueCallback<T>
    {
        T GetValue(float startFrame, float endFrame, T startValue, T endValue, float linearKeyframeProgress, float interpolatedKeyframeProgress, float overallProgress);
    }
}