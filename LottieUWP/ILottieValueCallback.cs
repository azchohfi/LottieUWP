namespace LottieUWP
{
    /// <summary>
    /// Allows you to set a callback on a resolved {@link com.airbnb.lottie.model.KeyPath} to modify its animation values at runtime. 
    /// This API is not ready for public use yet. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ILottieValueCallback<T>
    {
        T GetValue(int startFrame, int endFrame, T startValue, T endValue, float linearKeyframeProgress, float interpolatedKeyframeProgress, float overallProgress); 
    }
}