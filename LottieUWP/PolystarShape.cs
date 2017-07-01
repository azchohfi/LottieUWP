using Windows.Data.Json;

namespace LottieUWP
{
    internal class PolystarShape : IContentModel
    {
        internal enum Type
        {
            Star = 1,
            Polygon = 2
        }

        private readonly Type _type;

        private PolystarShape(string name, Type type, AnimatableFloatValue points, IAnimatableValue<PointF> position, AnimatableFloatValue rotation, AnimatableFloatValue innerRadius, AnimatableFloatValue outerRadius, AnimatableFloatValue innerRoundedness, AnimatableFloatValue outerRoundedness)
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
        }

        internal virtual string Name { get; }

        internal new virtual Type GetType()
        {
            return _type;
        }

        internal virtual AnimatableFloatValue Points { get; }

        internal virtual IAnimatableValue<PointF> Position { get; }

        internal virtual AnimatableFloatValue Rotation { get; }

        internal virtual AnimatableFloatValue InnerRadius { get; }

        internal virtual AnimatableFloatValue OuterRadius { get; }

        internal virtual AnimatableFloatValue InnerRoundedness { get; }

        internal virtual AnimatableFloatValue OuterRoundedness { get; }

        public IContent ToContent(LottieDrawable drawable, BaseLayer layer)
        {
            return new PolystarContent(drawable, layer, this);
        }

        internal static class Factory
        {
            internal static PolystarShape NewInstance(JsonObject json, LottieComposition composition)
            {
                var name = json.GetNamedString("nm");
                var type = (Type)(int)json.GetNamedNumber("sy");
                var points = AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("pt"), composition, false);
                var position = AnimatablePathValue.CreateAnimatablePathOrSplitDimensionPath(json.GetNamedObject("p"), composition);
                var rotation = AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("r"), composition, false);
                var outerRadius = AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("or"), composition);
                var outerRoundedness = AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("os"), composition, false);
                AnimatableFloatValue innerRadius;
                AnimatableFloatValue innerRoundedness;

                if (type == Type.Star)
                {
                    innerRadius = AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("ir"), composition);
                    innerRoundedness = AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("is"), composition, false);
                }
                else
                {
                    innerRadius = null;
                    innerRoundedness = null;
                }
                return new PolystarShape(name, type, points, position, rotation, innerRadius, outerRadius, innerRoundedness, outerRoundedness);
            }
        }
    }
}