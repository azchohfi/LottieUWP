using System;

namespace LottieUWP
{
    internal class AccelerateDecelerateInterpolator : IInterpolator
    {
        public float GetInterpolation(float f)
        {
            if (f < 0 || float.IsNaN(f))
                f = 0;
            if (f > 1)
                f = 1;
            return (float) (Math.Cos((f + 1) * Math.PI) / 2 + 0.5);
        }
    }
}