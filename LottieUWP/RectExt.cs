using Windows.Foundation;

namespace LottieUWP
{
    public static class RectExt
    {
        public static void Set(ref Rect rect, double x, double y, double width, double height)
        {
            rect.X = x;
            rect.Y = y;
            rect.Width = width > 0 ?  width : 0;
            rect.Height = height > 0 ? height : 0;
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
