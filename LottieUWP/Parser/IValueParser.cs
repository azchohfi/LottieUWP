namespace LottieUWP.Parser
{
    public interface IValueParser<out T>
    {
        T Parse(JsonReader reader, float scale);
    }
}
