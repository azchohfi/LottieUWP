using System;

namespace LottieUWP.Parser
{
    public class IntegerParser : IValueParser<int?>
    {
        public static readonly IntegerParser Instance = new IntegerParser();
        public int? Parse(JsonReader reader, float scale)
        {
            return (int)Math.Round(JsonUtils.ValueFromObject(reader) * scale);
        }
    }
}
