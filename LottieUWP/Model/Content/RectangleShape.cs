using System.Numerics;
using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    public class RectangleShape : IContentModel
    {
        public RectangleShape(string name, IAnimatableValue<Vector2?, Vector2?> position, AnimatablePointValue size, AnimatableFloatValue cornerRadius, bool hidden)
        {
            Name = name;
            Position = position;
            Size = size;
            CornerRadius = cornerRadius;
            IsHidden = hidden;
        }

        internal string Name { get; }

        internal AnimatableFloatValue CornerRadius { get; }

        internal AnimatablePointValue Size { get; }

        internal IAnimatableValue<Vector2?, Vector2?> Position { get; }

        internal bool IsHidden { get; }

        public IContent ToContent(ILottieDrawable drawable, BaseLayer layer)
        {
            return new RectangleContent(drawable, layer, this);
        }

        public override string ToString()
        {
            return "RectangleShape{position=" + Position + ", size=" + Size + '}';
        }
    }
}