using System;
using Windows.Foundation;

namespace LottieUWP
{
    public static class RectExt
    {
        public static void Set(ref Rect rect, double left, double top, double right, double bottom)
        {
            rect.X = left;
            rect.Y = top;
            rect.Width = Math.Abs(right - left);
            rect.Height = Math.Abs(bottom - top);
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
