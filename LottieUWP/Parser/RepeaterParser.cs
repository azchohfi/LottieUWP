using LottieUWP.Model.Animatable;
using LottieUWP.Model.Content;

namespace LottieUWP.Parser
{
    public static class RepeaterParser
    {
        public static Repeater Parse(JsonReader reader, LottieComposition composition)
        {
            string name = null;
            AnimatableFloatValue copies = null;
            AnimatableFloatValue offset = null;
            AnimatableTransform transform = null;

            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "nm":
                        name = reader.NextString();
                        break;
                    case "c":
                        copies = AnimatableFloatValue.Factory.NewInstance(reader, composition, false);
                        break;
                    case "o":
                        offset = AnimatableFloatValue.Factory.NewInstance(reader, composition, false);
                        break;
                    case "tr":
                        transform = AnimatableTransform.Factory.NewInstance(reader, composition);
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }

            return new Repeater(name, copies, offset, transform);
        }
    }
}
