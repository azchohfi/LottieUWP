using System;
using LottieUWP.Utils;

namespace LottieUWP.Value
{
    /// <summary>
    /// <see cref="Value.LottieValueCallback{T}"/> that provides a value offset from the original animation 
    ///  rather than an absolute value.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LottieRelativeFloatValueCallback : LottieValueCallback<float?>
    {
        public LottieRelativeFloatValueCallback()
        {
        }

        public LottieRelativeFloatValueCallback(float staticValue)
            : base(staticValue)
        {
        }

        public override float? GetValue(LottieFrameInfo<float?> frameInfo)
        {
            float originalValue = MiscUtils.Lerp(
                frameInfo.StartValue.Value,
                frameInfo.EndValue.Value,
                frameInfo.InterpolatedKeyframeProgress
            );
            float offset = GetOffset(frameInfo);
            return originalValue + offset;
        }

        public float GetOffset(LottieFrameInfo<float?> frameInfo)
        {
            if (Value == null)
            {
                throw new ArgumentException("You must provide a static value in the constructor " +
                                                   ", call setValue, or override getValue.");
            }
            return Value.Value;
        }
    }
}
