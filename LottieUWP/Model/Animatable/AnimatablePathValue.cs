using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LottieUWP.Value;
using LottieUWP.Animation.Keyframe;

namespace LottieUWP.Model.Animatable
{
    public class AnimatablePathValue : IAnimatableValue<Vector2?, Vector2?>
    {
        private readonly List<Keyframe<Vector2?>> _keyframes;

        /// <summary>
        /// Create a default static animatable path.
        /// </summary>
        public AnimatablePathValue()
        {
            _keyframes = new List<Keyframe<Vector2?>> { new Keyframe<Vector2?>(new Vector2(0, 0)) };
        }

        public AnimatablePathValue(List<Keyframe<Vector2?>> keyframes)
        {
            _keyframes = keyframes;
        }

        public IBaseKeyframeAnimation<Vector2?, Vector2?> CreateAnimation()
        {
            if (_keyframes[0].Static)
            {
                return new PointKeyframeAnimation(_keyframes);
            }

            return new PathKeyframeAnimation(_keyframes.ToList());
        }
    }
}