using LottieUWP.Model.Animatable;
using LottieUWP.Model.Content;

namespace LottieUWP.Parser
{
    public static class ShapePathParser
    {
        public static ShapePath Parse(JsonReader reader, LottieComposition composition)
        {
            string name = null;
            int ind = 0;
            AnimatableShapeValue shape = null;

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
                        shape = AnimatableShapeValue.Factory.NewInstance(reader, composition);
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }

            return new ShapePath(name, ind, shape);
        }
    }
}
