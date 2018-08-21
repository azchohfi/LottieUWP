using System.Numerics;
using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    public class RectangleShape : IContentModel
    {
        private readonly IAnimatableValue<Vector2?, Vector2?> _position;
        private readonly AnimatablePointValue _size;
        private readonly AnimatableFloatValue _cornerRadius;

        public RectangleShape(string name, IAnimatableValue<Vector2?, Vector2?> position, AnimatablePointValue size, AnimatableFloatValue cornerRadius)
        {
            Name = name;
            _position = position;
            _size = size;
            _cornerRadius = cornerRadius;
        }

        internal string Name { get; }

        internal AnimatableFloatValue CornerRadius => _cornerRadius;

        internal AnimatablePointValue Size => _size;

        internal IAnimatableValue<Vector2?, Vector2?> Position => _position;

        public IContent ToContent(LottieDrawable drawable, BaseLayer layer)
        {
            return new RectangleContent(drawable, layer, this);
        }

        public override string ToString()
        {
            return "RectangleShape{position=" + _position + ", size=" + _size + '}';
        }
    }
}