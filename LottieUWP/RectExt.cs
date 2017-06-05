using Windows.Foundation;

namespace LottieUWP
{
    public static class RectExt
    {
        public static void Set(ref Rect rect, double x, double y, double width, double height)
        {
            rect.X = x;
            rect.Y = y;
            rect.Width = width;
            rect.Height = height;
        }

        public static void Set(ref Rect rect, Rect newRect)
        {
            rect.X = newRect.X;
            rect.Y = newRect.Y;
            rect.Width = newRect.Width;
            rect.Height = newRect.Height;
        }
    }
}
