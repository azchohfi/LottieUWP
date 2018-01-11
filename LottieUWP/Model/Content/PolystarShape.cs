using System.Numerics;
using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    internal class PolystarShape : IContentModel
    {
        internal enum Type
        {
            Star = 1,
            Polygon = 2
        }

        private readonly Type _type;

        private PolystarShape(string name, Type type, AnimatableFloatValue points, IAnimatableValue<Vector2?, Vector2?> position, AnimatableFloatValue rotation, AnimatableFloatValue innerRadius, AnimatableFloatValue outerRadius, AnimatableFloatValue innerRoundedness, AnimatableFloatValue outerRoundedness)
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

        internal virtual IAnimatableValue<Vector2?, Vector2?> Position { get; }

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
            internal static PolystarShape NewInstance(JsonReader reader, LottieComposition composition)
            {
                string name = null;
                Type type = Type.Polygon;
                AnimatableFloatValue points = null;
                IAnimatableValue<Vector2?, Vector2?> position = null;
                AnimatableFloatValue rotation = null;
                AnimatableFloatValue outerRadius = null;
                AnimatableFloatValue outerRoundedness = null;
                AnimatableFloatValue innerRadius = null;
                AnimatableFloatValue innerRoundedness = null;

                while (reader.HasNext())
                {
                    switch (reader.NextName())
                    {
                        case "nm":
                            name = reader.NextString();
                            break;
                        case "sy":
                            type = (Type)reader.NextInt();
                            break;
                        case "pt":
                            points = AnimatableFloatValue.Factory.NewInstance(reader, composition, false);
                            break;
                        case "p":
                            position = AnimatablePathValue.CreateAnimatablePathOrSplitDimensionPath(reader, composition);
                            break;
                        case "r":
                            rotation = AnimatableFloatValue.Factory.NewInstance(reader, composition, false);
                            break;
                        case "or":
                            outerRadius = AnimatableFloatValue.Factory.NewInstance(reader, composition);
                            break;
                        case "os":
                            outerRoundedness = AnimatableFloatValue.Factory.NewInstance(reader, composition, false);
                            break;
                        case "ir":
                            innerRadius = AnimatableFloatValue.Factory.NewInstance(reader, composition);
                            break;
                        case "is":
                            innerRoundedness = AnimatableFloatValue.Factory.NewInstance(reader, composition, false);
                            break;
                        default:
                            reader.SkipValue();
                            break;
                    }
                }

                return new PolystarShape(name, type, points, position, rotation, innerRadius, outerRadius, innerRoundedness, outerRoundedness);
            }
        }
    }
}