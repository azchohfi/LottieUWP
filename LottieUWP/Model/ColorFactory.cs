using Windows.UI;
using LottieUWP.Model.Animatable;
using Newtonsoft.Json;

namespace LottieUWP.Model
{
    internal class ColorFactory : IAnimatableValueFactory<Color>
    {
        internal static readonly ColorFactory Instance = new ColorFactory();

        public Color ValueFromObject(JsonReader reader, float scale)
        {
            bool isArray = reader.Peek() == JsonToken.StartArray;
            if (isArray)
            {
                reader.BeginArray();
            }
            var r = reader.NextDouble();
            var g = reader.NextDouble();
            var b = reader.NextDouble();
            var a = reader.NextDouble();
            if (isArray)
            {
                reader.EndArray();
            }

            if (r <= 1 && g <= 1 && b <= 1 && a <= 1)
            {
                r *= 255;
                g *= 255;
                b *= 255;
                a *= 255;
            }
            return Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
        }
    }
}