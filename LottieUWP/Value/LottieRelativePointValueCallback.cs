using System.Numerics;
using LottieUWP.Utils;

namespace LottieUWP.Value
{
    /// <summary>
    /// <see cref="LottieUWP.Value.LottieValueCallback{T}"/> that provides a value offset from the original animation 
    ///  rather than an absolute value.
    /// </summary>
    public abstract class LottieRelativePointValueCallback : LottieValueCallback<Vector2>
    {
        public override Vector2 GetValue(float sf, float ef, Vector2 sv, Vector2 ev, float lkp, float ikp, float p)
        {
            var point = new Vector2(MiscUtils.Lerp(sv.X, ev.X, ikp), MiscUtils.Lerp(sv.Y, sv.Y, ikp));

            var offset = GetOffset(sf, ef, sv, ev, lkp, ikp, p);
            point.X += offset.X;
            point.Y += offset.Y;
            return point;
        }

        public abstract Vector2 GetOffset(float startFrame, float endFrame, Vector2 startValue, Vector2 endValue, float linearKeyframeProgress, float interpolatedKeyframeProgress, float overallProgress);
    }
}
