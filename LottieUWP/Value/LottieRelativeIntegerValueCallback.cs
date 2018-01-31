using LottieUWP.Utils;

namespace LottieUWP.Value
{
    /// <summary>
    /// <see cref="Value.LottieValueCallback{T}"/> that provides a value offset from the original animation 
    ///  rather than an absolute value.
    /// </summary>
    public abstract class LottieRelativeIntegerValueCallback : LottieValueCallback<int>
    {
        public override int GetValue(LottieFrameInfo<int> frameInfo)
        {
            int originalValue = MiscUtils.Lerp(
                frameInfo.StartValue,
                frameInfo.EndValue,
                frameInfo.InterpolatedKeyframeProgress
            );
            int newValue = GetOffset(frameInfo);
            return originalValue + newValue;
        }

        public abstract int GetOffset(LottieFrameInfo<int> frameInfo);
    }
}
