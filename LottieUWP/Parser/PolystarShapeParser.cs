using System.Numerics;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Content;

namespace LottieUWP.Parser
{
    public static class PolystarShapeParser
    {
        public static PolystarShape Parse(JsonReader reader, LottieComposition composition)
        {
            string name = null;
            PolystarShape.Type type = PolystarShape.Type.Polygon;
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
                        type = (PolystarShape.Type)reader.NextInt();
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
