using Windows.Data.Json;

namespace LottieUWP
{
    internal class JsonUtils
    {
        private JsonUtils()
        {
        }

        internal static PointF PointFromJsonObject(JsonObject values, float scale)
        {
            return new PointF(ValueFromObject(values["x"]) * scale, ValueFromObject(values["y"]) * scale);
        }

        internal static PointF PointFromJsonArray(JsonArray values, float scale)
        {
            if (values.Count < 2)
            {
                throw new System.ArgumentException("Unable to parse point for " + values);
            }
            return new PointF((float)values.GetNumberAt(0, 1) * scale, (float)values.GetNumberAt(1, 1) * scale);
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