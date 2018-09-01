using System.Numerics;
using LottieUWP.Animation.Content;
using LottieUWP.Animation.Keyframe;
using LottieUWP.Model.Content;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Animatable
{
    public class AnimatableTransform : IModifierContent, IContentModel
    {
        public AnimatableTransform()
            : this(
                new AnimatablePathValue(),
                new AnimatablePathValue(),
                new AnimatableScaleValue(),
                new AnimatableFloatValue(),
                new AnimatableIntegerValue(),
                new AnimatableFloatValue(),
                new AnimatableFloatValue()
            )
        {
        }

        public AnimatableTransform(AnimatablePathValue anchorPoint, IAnimatableValue<Vector2?, Vector2?> position, AnimatableScaleValue scale, AnimatableFloatValue rotation, AnimatableIntegerValue opacity, AnimatableFloatValue startOpacity, AnimatableFloatValue endOpacity)
        {
            AnchorPoint = anchorPoint;
            Position = position;
            Scale = scale;
            Rotation = rotation;
            Opacity = opacity;
            StartOpacity = startOpacity;
            EndOpacity = endOpacity;
        }

        internal AnimatablePathValue AnchorPoint { get; }

        internal IAnimatableValue<Vector2?, Vector2?> Position { get; }

        internal AnimatableScaleValue Scale { get; }

        internal AnimatableFloatValue Rotation { get; }

        internal AnimatableIntegerValue Opacity { get; }

        // Used for repeaters 
        internal AnimatableFloatValue StartOpacity { get; }
        internal AnimatableFloatValue EndOpacity { get; }

        public TransformKeyframeAnimation CreateAnimation()
        {
            return new TransformKeyframeAnimation(this);
        }

        public IContent ToContent(ILottieDrawable drawable, BaseLayer layer)
        {
            return null;
        }
    }
}