namespace LottieUWP.Value
{
    /// <summary>
    /// Delegate interface for <see cref="LottieValueCallback{T}"/>. This is helpful for the Kotlin API because you can use a SAM conversion to write the 
    /// callback as a single abstract method block like this: 
    /// animationView.AddValueCallback(keyPath, LottieProperty.TransformOpacity) { 50 }
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="frameInfo"></param>
    /// <returns></returns>
    public delegate T SimpleLottieValueCallback<T>(LottieFrameInfo<T> frameInfo);
}
