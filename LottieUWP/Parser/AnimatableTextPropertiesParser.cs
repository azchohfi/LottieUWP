using LottieUWP.Model.Animatable;

namespace LottieUWP.Parser
{
    public static class AnimatableTextPropertiesParser
    {
        public static AnimatableTextProperties Parse(JsonReader reader, LottieComposition composition)
        {
            AnimatableTextProperties anim = null;

            reader.BeginObject();
            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "a":
                        anim = ParseAnimatableTextProperties(reader, composition);
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }
            reader.EndObject();
            if (anim == null)
            {
                // Not sure if this is possible. 
                return new AnimatableTextProperties(null, null, null, null);
            }
            return anim;
        }

        private static AnimatableTextProperties ParseAnimatableTextProperties(JsonReader reader, LottieComposition composition)
        {
            AnimatableColorValue color = null;
            AnimatableColorValue stroke = null;
            AnimatableFloatValue strokeWidth = null;
            AnimatableFloatValue tracking = null;

            reader.BeginObject();
            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "fc":
                        color = AnimatableValueParser.ParseColor(reader, composition);
                        break;
                    case "sc":
                        stroke = AnimatableValueParser.ParseColor(reader, composition);
                        break;
                    case "sw":
                        strokeWidth = AnimatableValueParser.ParseFloat(reader, composition);
                        break;
                    case "t":
                        tracking = AnimatableValueParser.ParseFloat(reader, composition);
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }
            reader.EndObject();

            return new AnimatableTextProperties(color, stroke, strokeWidth, tracking);
        }
    }
}
