using System.Numerics;
using Windows.Data.Json;
using LottieUWP.Utils;

namespace LottieUWP.Model
{
    internal class PointFFactory : IAnimatableValueFactory<Vector2?>
    {
        internal static readonly PointFFactory Instance = new PointFFactory();

        private PointFFactory()
        {
        }

        public Vector2? ValueFromObject(IJsonValue @object, float scale)
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