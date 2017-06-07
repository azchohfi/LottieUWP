using System;

namespace LottieUWP
{
    internal class AccelerateDecelerateInterpolator : IInterpolator
    {
        public float GetInterpolation(float f)
        {
            return (float) (Math.Cos((f + 1) * Math.PI) / 2 + 0.5);
        }
    }
}