using LottieUWP.Utils;

namespace LottieUWP.Value
{
    /// <summary>
    /// <see cref="Value.LottieValueCallback{T}"/> that provides a value offset from the original animation 
    ///  rather than an absolute value.
    /// </summary>
    public abstract class LottieRelativeFloatValueCallback : LottieValueCallback<float>
    {
        public override float GetValue(LottieFrameInfo<float> frameInfo)
        {
            float originalValue = MiscUtils.Lerp(
                frameInfo.StartValue,
                frameInfo.EndValue,
                frameInfo.InterpolatedKeyframeProgress
            );
            float offset = GetOffset(frameInfo);
            return originalValue + offset;
        }

        public abstract float GetOffset(LottieFrameInfo<float> frameInfo);
    }
}
