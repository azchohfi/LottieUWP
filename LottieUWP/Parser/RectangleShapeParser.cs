using System.Numerics;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Content;

namespace LottieUWP.Parser
{
    public static class RectangleShapeParser
    {
        public static RectangleShape Parse(JsonReader reader, LottieComposition composition)
        {
            string name = null;
            IAnimatableValue<Vector2?, Vector2?> position = null;
            AnimatablePointValue size = null;
            AnimatableFloatValue roundedness = null;

            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "nm":
                        name = reader.NextString();
                        break;
                    case "p":
                        position =
                            AnimatablePathValue.CreateAnimatablePathOrSplitDimensionPath(reader, composition);
                        break;
                    case "s":
                        size = AnimatablePointValue.Factory.NewInstance(reader, composition);
                        break;
                    case "r":
                        roundedness = AnimatableFloatValue.Factory.NewInstance(reader, composition);
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }

            return new RectangleShape(name, position, size, roundedness);
        }
    }
}
