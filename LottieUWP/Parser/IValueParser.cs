namespace LottieUWP.Parser
{
    internal interface IValueParser<out T>
    {
        T Parse(JsonReader reader, float scale);
    }
}
