using LottieUWP.Model.Animatable;
using LottieUWP.Model.Content;

namespace LottieUWP.Parser
{
    static class ShapePathParser
    {
        internal static ShapePath Parse(JsonReader reader, LottieComposition composition)
        {
            string name = null;
            int ind = 0;
            AnimatableShapeValue shape = null;
            bool hidden = false;

            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "nm":
                        name = reader.NextString();
                        break;
                    case "ind":
                        ind = reader.NextInt();
                        break;
                    case "ks":
                        shape = AnimatableValueParser.ParseShapeData(reader, composition);
                        break;
                    case "hd":
                        hidden = reader.NextBoolean();
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }

            return new ShapePath(name, ind, shape, hidden);
        }
    }
}
