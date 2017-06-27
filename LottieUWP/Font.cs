using Windows.Data.Json;

namespace LottieUWP
{
    internal class Font
    {
        private Font(string family, string name, string style, double ascent)
        {
            Family = family;
            Name = name;
            Style = style;
            Ascent = ascent;
        }

        internal readonly string Family;

        internal readonly string Name;

        internal readonly string Style;

        internal readonly double Ascent;

        internal static class Factory
        {
            internal static Font NewInstance(JsonObject json)
            {
                string family = json.GetNamedString("fFamily", "");
                string name = json.GetNamedString("fName", "");
                string style = json.GetNamedString("fStyle", "");
                double ascent = json.GetNamedNumber("ascent", 0);
                return new Font(family, name, style, ascent);
            }
        }
    }
}
