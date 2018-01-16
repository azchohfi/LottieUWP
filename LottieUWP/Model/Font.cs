namespace LottieUWP.Model
{
    public class Font
    {
        public Font(string family, string name, string style, float ascent)
        {
            Family = family;
            Name = name;
            Style = style;
            Ascent = ascent;
        }

        public string Family { get; }

        public string Name { get; }

        public string Style { get; }

        internal readonly float Ascent;
    }
}
