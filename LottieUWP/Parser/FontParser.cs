using LottieUWP.Model;

namespace LottieUWP.Parser
{
    public static class FontParser
    {
        public static Font Parse(JsonReader reader)
        {
            string family = null;
            string name = null;
            string style = null;
            float ascent = 0;

            reader.BeginObject();
            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "fFamily":
                        family = reader.NextString();
                        break;
                    case "fName":
                        name = reader.NextString();
                        break;
                    case "fStyle":
                        style = reader.NextString();
                        break;
                    case "ascent":
                        ascent = reader.NextDouble();
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }
            reader.EndObject();

            return new Font(family, name, style, ascent);
        }
    }
}
