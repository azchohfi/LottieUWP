using System.Numerics;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Content;

namespace LottieUWP.Parser
{
    static class CircleShapeParser
    {
        internal static CircleShape Parse(JsonReader reader, LottieComposition composition, int d)
        {
            string name = null;
            IAnimatableValue<Vector2?, Vector2?> position = null;
            AnimatablePointValue size = null;
            bool reversed = d == 3;
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
                    case "hd":
                        hidden = reader.NextBoolean();
                        break;
                    case "d":
                        // "d" is 2 for normal and 3 for reversed. 
                        reversed = reader.NextInt() == 3;
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }

            return new CircleShape(name, position, size, reversed, hidden);
        }
    }
}
