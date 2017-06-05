namespace LottieUWP
{
    public interface IOnCompositionLoadedListener
    {
        /// <summary>
        /// Composition will be null if there was an error loading it. Check logcat for more details. 
        /// </summary>
        void OnCompositionLoaded(LottieComposition composition);
    }
}