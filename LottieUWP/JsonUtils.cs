using System.Numerics;
using Windows.Data.Json;

namespace LottieUWP
{
    internal static class JsonUtils
    {
        internal static Vector2 PointFromJsonObject(JsonObject values, float scale)
        {
            return new Vector2(ValueFromObject(values["x"]) * scale, ValueFromObject(values["y"]) * scale);
        }

        internal static Vector2 PointFromJsonArray(JsonArray values, float scale)
        {
            if (values.Count < 2)
            {
                throw new System.ArgumentException("Unable to parse point for " + values);
            }
            return new Vector2((float)values.GetNumberAt(0, 1) * scale, (float)values.GetNumberAt(1, 1) * scale);
        }

        internal static float ValueFromObject(IJsonValue @object)
        {
            if (@object.ValueType == JsonValueType.Number)
            {
                return (float)@object.GetNumber();
            }
            if (@object.ValueType == JsonValueType.Array)
            {
                return (float)@object.GetArray()[0].GetNumber();
            }
            return 0;
        }
    }
}