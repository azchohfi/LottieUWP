using Windows.Data.Json;

namespace LottieUWP
{
    internal class PointFFactory : IAnimatableValueFactory<PointF>
    {
        internal static readonly PointFFactory Instance = new PointFFactory();

        private PointFFactory()
        {
        }

        public PointF ValueFromObject(IJsonValue @object, float scale)
        {
            if (@object.ValueType == JsonValueType.Array)
            {
                return JsonUtils.PointFromJsonArray(@object.GetArray(), scale);
            }
            if (@object.ValueType == JsonValueType.Object)
            {
                return JsonUtils.PointFromJsonObject(@object.GetObject(), scale);
            }

            throw new System.ArgumentException("Unable to parse point from " + @object);
        }
    }
}