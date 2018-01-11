namespace LottieUWP.Model
{
    internal class Font
    {
        private Font(string family, string name, string style, float ascent)
        {
            Family = family;
            Name = name;
            Style = style;
            Ascent = ascent;
        }

        internal readonly string Family;

        internal readonly string Name;

        internal readonly string Style;

        internal readonly float Ascent;

        internal static class Factory
        {
            internal static Font NewInstance(JsonReader reader)
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
}
