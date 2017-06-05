using Windows.Data.Json;
using Windows.UI;

namespace LottieUWP
{
    internal class ColorFactory : IAnimatableValueFactory<Color>
    {
        internal static readonly ColorFactory Instance = new ColorFactory();

        public Color ValueFromObject(IJsonValue @object, float scale)
        {
            JsonArray colorArray = @object.GetArray();
            if (colorArray.Count == 4)
            {
                bool shouldUse255 = true;
                for (int i = 0; i < colorArray.Count; i++)
                {
                    double colorChannel = colorArray[i].GetNumber();
                    if (colorChannel > 1f)
                    {
                        shouldUse255 = false;
                    }
                }

                float multiplier = shouldUse255 ? 255f : 1f;
                return Color.FromArgb((byte)(colorArray[3].GetNumber() * multiplier), (byte)(colorArray[0].GetNumber() * multiplier), (byte)(colorArray[1].GetNumber() * multiplier), (byte)(colorArray[2].GetNumber() * multiplier));
            }
            return Colors.Black;
        }
    }
}