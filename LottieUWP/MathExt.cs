using System;

namespace LottieUWP
{
    public static class MathExt
    {
        internal static double Hypot(double x, double y)
        {
            return Math.Sqrt(x * x + y * y);
        }

        public static float Lerp(float value1, float value2, float amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        public static double ToRadians(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}