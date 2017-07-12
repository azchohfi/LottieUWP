using Microsoft.Graphics.Canvas;

namespace LottieUWP
{
    /// <summary>
    /// Delegate to handle the loading of bitmaps that are not packaged in the assets of your app.
    /// </summary>
    public interface IImageAssetDelegate
    {
        CanvasBitmap FetchBitmap(LottieImageAsset asset);
    }
}