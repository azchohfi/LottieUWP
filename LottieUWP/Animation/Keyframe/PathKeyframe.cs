using LottieUWP.Value;
using System.Numerics;

namespace LottieUWP.Animation.Keyframe
{
    public class PathKeyframe : Keyframe<Vector2?>
    {
        public PathKeyframe(LottieComposition composition, Keyframe<Vector2?> keyframe)
            : base(composition, keyframe.StartValue, keyframe.EndValue, keyframe.Interpolator, keyframe.StartFrame, keyframe.EndFrame)
        {
            var equals = EndValue != null && StartValue != null && StartValue.Equals(EndValue.Value);
            if (EndValue != null && !equals)
            {
                Path = Utils.Utils.CreatePath(StartValue.Value, EndValue.Value, keyframe.PathCp1, keyframe.PathCp2);
            }
        }

        /// <summary>
        /// This will be null if the startValue and endValue are the same.
        /// </summary>
        internal Path Path { get; }
    }
}