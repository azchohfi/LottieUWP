using System.Numerics;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Content;

namespace LottieUWP.Parser
{
    static class RectangleShapeParser
    {
        internal static RectangleShape Parse(JsonReader reader, LottieComposition composition)
        {
            string name = null;
            IAnimatableValue<Vector2?, Vector2?> position = null;
            AnimatablePointValue size = null;
            AnimatableFloatValue roundedness = null;
            bool hidden = false;

            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "nm":
                        name = reader.NextString();
                        break;
                    case "p":
                        position = AnimatablePathValueParser.ParseSplitPath(reader, composition);
                        break;
                    case "s":
                        size = AnimatableValueParser.ParsePoint(reader, composition);
                        break;
                    case "r":
                        roundedness = AnimatableValueParser.ParseFloat(reader, composition);
                        break;
                    case "hd":
                        hidden = reader.NextBoolean();
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }

            return new RectangleShape(name, position, size, roundedness, hidden);
        }
    }
}
