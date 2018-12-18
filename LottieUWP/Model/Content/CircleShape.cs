using System.Numerics;
using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    public class CircleShape : IContentModel
    {
        public CircleShape(string name, IAnimatableValue<Vector2?, Vector2?> position, AnimatablePointValue size, bool isReversed, bool hidden)
        {
            Name = name;
            Position = position;
            Size = size;
            IsReversed = isReversed;
            IsHidden = hidden;
        }

        internal string Name { get; }

        public IAnimatableValue<Vector2?, Vector2?> Position { get; }

        public AnimatablePointValue Size { get; }

        public bool IsReversed { get; }

        public bool IsHidden { get; }

        public IContent ToContent(ILottieDrawable drawable, BaseLayer layer)
        {
            return new EllipseContent(drawable, layer, this);
        }
    }
}