using Windows.Data.Json;

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
            internal static Font NewInstance(JsonObject json)
            {
                var family = json.GetNamedString("fFamily", "");
                var name = json.GetNamedString("fName", "");
                var style = json.GetNamedString("fStyle", "");
                var ascent = (float)json.GetNamedNumber("ascent", 0);
                return new Font(family, name, style, ascent);
            }
        }
    }
}
