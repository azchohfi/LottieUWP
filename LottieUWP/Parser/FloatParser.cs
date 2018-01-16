namespace LottieUWP.Parser
{
    public class FloatParser : IValueParser<float?>
    {
        public static readonly FloatParser Instance = new FloatParser();

        public float? Parse(JsonReader reader, float scale)
        {
            return JsonUtils.ValueFromObject(reader) * scale;
        }
    }
}
