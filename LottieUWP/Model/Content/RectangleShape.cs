using System.Numerics;
using Windows.Data.Json;
using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    internal class RectangleShape : IContentModel
    {
        private readonly IAnimatableValue<Vector2?> _position;
        private readonly AnimatablePointValue _size;
        private readonly AnimatableFloatValue _cornerRadius;

        private RectangleShape(string name, IAnimatableValue<Vector2?> position, AnimatablePointValue size, AnimatableFloatValue cornerRadius)
        {
            Name = name;
            _position = position;
            _size = size;
            _cornerRadius = cornerRadius;
        }

        internal static class Factory
        {
            internal static RectangleShape NewInstance(JsonObject json, LottieComposition composition)
            {
                return new RectangleShape(json.GetNamedString("nm"), AnimatablePathValue.CreateAnimatablePathOrSplitDimensionPath(json.GetNamedObject("p"), composition), AnimatablePointValue.Factory.NewInstance(json.GetNamedObject("s"), composition), AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("r"), composition));
            }
        }

        internal virtual string Name { get; }

        internal virtual AnimatableFloatValue CornerRadius => _cornerRadius;

        internal virtual AnimatablePointValue Size => _size;

        internal virtual IAnimatableValue<Vector2?> Position => _position;

        public IContent ToContent(LottieDrawable drawable, BaseLayer layer)
        {
            return new RectangleContent(drawable, layer, this);
        }

        public override string ToString()
        {
            return "RectangleShape{" + "cornerRadius=" + _cornerRadius.InitialValue + ", position=" + _position + ", size=" + _size + '}';
        }
    }
}