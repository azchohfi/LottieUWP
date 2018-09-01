using Microsoft.Graphics.Canvas;

namespace LottieUWP
{
    public interface ILottieDrawable
    {
        void InvalidateSelf();
        bool UseTextGlyphs();
        Typeface GetTypeface(string fontFamily, string style);
        TextDelegate TextDelegate { get; }
        CanvasBitmap GetImageAsset(string id);
        LottieComposition Composition { get; }
        bool EnableMergePathsForKitKatAndAbove();
    }
}
