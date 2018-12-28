using LottieUWP.Model.Animatable;
using LottieUWP.Model.Content;

namespace LottieUWP.Parser
{
    static class GradientFillParser
    {
        internal static GradientFill Parse(JsonReader reader, LottieComposition composition)
        {
            string name = null;
            AnimatableGradientColorValue color = null;
            AnimatableIntegerValue opacity = null;
            GradientType gradientType = GradientType.Linear;
            AnimatablePointValue startPoint = null;
            AnimatablePointValue endPoint = null;
            PathFillType fillType = PathFillType.EvenOdd;
            bool hidden = false;

            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "nm":
                        name = reader.NextString();
                        break;
                    case "g":
                        int points = -1;
                        reader.BeginObject();
                        while (reader.HasNext())
                        {
                            switch (reader.NextName())
                            {
                                case "p":
                                    points = reader.NextInt();
                                    break;
                                case "k":
                                    color = AnimatableValueParser.ParseGradientColor(reader, composition, points);
                                    break;
                                default:
                                    reader.SkipValue();
                                    break;
                            }
                        }
                        reader.EndObject();
                        break;
                    case "o":
                        opacity = AnimatableValueParser.ParseInteger(reader, composition);
                        break;
                    case "t":
                        gradientType = reader.NextInt() == 1 ? GradientType.Linear : GradientType.Radial;
                        break;
                    case "s":
                        startPoint = AnimatableValueParser.ParsePoint(reader, composition);
                        break;
                    case "e":
                        endPoint = AnimatableValueParser.ParsePoint(reader, composition);
                        break;
                    case "r":
                        fillType = reader.NextInt() == 1 ? PathFillType.Winding : PathFillType.EvenOdd;
                        break;
                    case "hd":
                        hidden = reader.NextBoolean();
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }

            return new GradientFill(
            name, gradientType, fillType, color, opacity, startPoint, endPoint, null, null, hidden);
        }
    }
}
