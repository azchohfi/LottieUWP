using LottieUWP.Model.Animatable;
using LottieUWP.Model.Content;

namespace LottieUWP.Parser
{
    static class ShapeTrimPathParser
    {
        internal static ShapeTrimPath Parse(JsonReader reader, LottieComposition composition)
        {
            string name = null;
            ShapeTrimPath.Type type = ShapeTrimPath.Type.Simultaneously;
            AnimatableFloatValue start = null;
            AnimatableFloatValue end = null;
            AnimatableFloatValue offset = null;
            bool hidden = false;

            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "s":
                        start = AnimatableValueParser.ParseFloat(reader, composition, false);
                        break;
                    case "e":
                        end = AnimatableValueParser.ParseFloat(reader, composition, false);
                        break;
                    case "o":
                        offset = AnimatableValueParser.ParseFloat(reader, composition, false);
                        break;
                    case "nm":
                        name = reader.NextString();
                        break;
                    case "m":
                        type = (ShapeTrimPath.Type)reader.NextInt();
                        break;
                    case "hd":
                        hidden = reader.NextBoolean();
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }

            return new ShapeTrimPath(name, type, start, end, offset, hidden);
        }
    }
}
