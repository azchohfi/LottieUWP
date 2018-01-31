using System;
using System.Numerics;
using LottieUWP.Utils;

namespace LottieUWP.Value
{
    /// <summary>
    /// <see cref="Value.LottieValueCallback{T}"/> that provides a value offset from the original animation 
    ///  rather than an absolute value.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LottieRelativePointValueCallback : LottieValueCallback<Vector2?>
    {
        public LottieRelativePointValueCallback()
        {
        }

        public LottieRelativePointValueCallback(Vector2 staticValue)
            : base(staticValue)
        {
        }

        public override Vector2? GetValue(LottieFrameInfo<Vector2?> frameInfo)
        {
            var point = new Vector2(
                MiscUtils.Lerp(
                    frameInfo.StartValue.Value.X,
                    frameInfo.EndValue.Value.X,
                    frameInfo.InterpolatedKeyframeProgress),
                MiscUtils.Lerp(
                    frameInfo.StartValue.Value.Y,
                    frameInfo.EndValue.Value.Y,
                    frameInfo.InterpolatedKeyframeProgress)
            );

            var offset = GetOffset(frameInfo);
            point.X += offset.X;
            point.Y += offset.Y;
            return point;
        }

        /// <summary>
        /// Override this to provide your own offset on every frame. 
        /// </summary>
        /// <param name="frameInfo"></param>
        /// <returns></returns>
        public Vector2 GetOffset(LottieFrameInfo<Vector2?> frameInfo)
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
