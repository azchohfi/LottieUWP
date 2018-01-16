using LottieUWP.Value;
using Newtonsoft.Json;

namespace LottieUWP.Parser
{
    public class ScaleXyParser : IValueParser<ScaleXy>
    {
        public static readonly ScaleXyParser Instance = new ScaleXyParser();

        public ScaleXy Parse(JsonReader reader, float scale)
        {
            bool isArray = reader.Peek() == JsonToken.StartArray;
            if (isArray)
            {
                reader.BeginArray();
            }
            float sx = reader.NextDouble();
            float sy = reader.NextDouble();
            while (reader.HasNext())
            {
                reader.SkipValue();
            }
            if (isArray)
            {
                reader.EndArray();
            }
            return new ScaleXy(sx / 100f * scale, sy / 100f * scale);
        }
    }
}
