using System.Numerics;
using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    public class PolystarShape : IContentModel
    {
        public enum Type
        {
            Star = 1,
            Polygon = 2
        }

        private readonly Type _type;

        public PolystarShape(string name, Type type, AnimatableFloatValue points, IAnimatableValue<Vector2?, Vector2?> position, AnimatableFloatValue rotation, AnimatableFloatValue innerRadius, AnimatableFloatValue outerRadius, AnimatableFloatValue innerRoundedness, AnimatableFloatValue outerRoundedness, bool hidden)
        {
            Name = name;
            _type = type;
            Points = points;
            Position = position;
            Rotation = rotation;
            InnerRadius = innerRadius;
            OuterRadius = outerRadius;
            InnerRoundedness = innerRoundedness;
            OuterRoundedness = outerRoundedness;
            IsHidden = hidden;
        }

        internal string Name { get; }

        internal new Type GetType()
        {
            return _type;
        }

        internal AnimatableFloatValue Points { get; }

        internal IAnimatableValue<Vector2?, Vector2?> Position { get; }

        internal AnimatableFloatValue Rotation { get; }

        internal AnimatableFloatValue InnerRadius { get; }

        internal AnimatableFloatValue OuterRadius { get; }

        internal AnimatableFloatValue InnerRoundedness { get; }

        internal AnimatableFloatValue OuterRoundedness { get; }

        internal bool IsHidden { get; }

        public IContent ToContent(ILottieDrawable drawable, BaseLayer layer)
        {
            return new PolystarContent(drawable, layer, this);
        }
    }
}