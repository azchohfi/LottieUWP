using LottieUWP.Model.Animatable;
using LottieUWP.Model.Content;

namespace LottieUWP.Parser
{
    static class ShapeFillParser
    {
        internal static ShapeFill Parse(JsonReader reader, LottieComposition composition)
        {
            AnimatableColorValue color = null;
            bool fillEnabled = false;
            AnimatableIntegerValue opacity = null;
            string name = null;
            int fillTypeInt = 1;
            bool hidden = false;

            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "nm":
                        name = reader.NextString();
                        break;
                    case "c":
                        color = AnimatableValueParser.ParseColor(reader, composition);
                        break;
                    case "o":
                        opacity = AnimatableValueParser.ParseInteger(reader, composition);
                        break;
                    case "fillEnabled":
                        fillEnabled = reader.NextBoolean();
                        break;
                    case "r":
                        fillTypeInt = reader.NextInt();
                        break;
                    case "hd":
                        hidden = reader.NextBoolean();
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }

            var fillType = fillTypeInt == 1 ? PathFillType.Winding : PathFillType.EvenOdd;
            return new ShapeFill(name, fillEnabled, fillType, color, opacity, hidden);
        }
    }
}
