namespace LottieUWP
{
    /// <summary>
    /// Delegate to handle the loading of fonts that are not packaged in the assets of your app or don't
    /// have the same file name.
    /// </summary>
    /// <seealso cref="LottieDrawable.FontAssetDelegate"></seealso>
    public class FontAssetDelegate
    {
        /// <summary>
        /// Override this if you want to return a Typeface from a font family.
        /// </summary>
        public Typeface FetchFont(string fontFamily)
        {
            return null;
        }

        /// <summary>
        /// Override this if you want to specify the asset path for a given font family.
        /// </summary>
        public string GetFontPath(string fontFamily)
        {
            return null;
        }
    }
}
