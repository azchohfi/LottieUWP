using Windows.Foundation;

namespace LottieUWP
{
    public static class RectExt
    {
        public static void Set(ref Rect rect, double left, double top, double right, double bottom)
        {
            rect.X = left;
            rect.Y = top;
            var width = right - left;
            rect.Width = width > 0 ?  width : 0;
            var height = bottom - top;
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
