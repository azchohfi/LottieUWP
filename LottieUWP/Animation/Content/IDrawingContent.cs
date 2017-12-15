using Windows.Foundation;

namespace LottieUWP.Animation.Content
{
    internal interface IDrawingContent : IContent
    {
        void Draw(BitmapCanvas canvas, Matrix3X3 parentMatrix, byte alpha);
        void GetBounds(out Rect outBounds, Matrix3X3 parentMatrix);
    }
}