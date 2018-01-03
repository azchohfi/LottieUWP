using Windows.Data.Json;

namespace LottieUWP
{
    public static class JsonArrayExt
    {
        internal static double GetNumberAt(this JsonArray jsonArray, int index, double fallback)
        {
            var jsonObject = jsonArray[index];
            if (jsonObject.ValueType == JsonValueType.Number)
                return jsonObject.GetNumber();
            return fallback;
        }
    }
}