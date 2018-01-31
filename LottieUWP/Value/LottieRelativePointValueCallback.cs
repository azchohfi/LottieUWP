using System.Numerics;
using LottieUWP.Utils;

namespace LottieUWP.Value
{
    /// <summary>
    /// <see cref="Value.LottieValueCallback{T}"/> that provides a value offset from the original animation 
    ///  rather than an absolute value.
    /// </summary>
    public abstract class LottieRelativePointValueCallback : LottieValueCallback<Vector2>
    {
        public override Vector2 GetValue(LottieFrameInfo<Vector2> frameInfo)
        {
            var point = new Vector2(
                MiscUtils.Lerp(
                    frameInfo.StartValue.X,
                    frameInfo.EndValue.X,
                    frameInfo.InterpolatedKeyframeProgress),
                MiscUtils.Lerp(
                    frameInfo.StartValue.Y,
                    frameInfo.EndValue.Y,
                    frameInfo.InterpolatedKeyframeProgress)
            );

            var offset = GetOffset(frameInfo);
            point.X += offset.X;
            point.Y += offset.Y;
            return point;
        }

        public abstract Vector2 GetOffset(LottieFrameInfo<Vector2> frameInfo);
    }
}
