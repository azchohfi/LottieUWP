using Windows.Data.Json;

namespace LottieUWP
{
    internal class ShapePath
    {
        private readonly string _name;
        private readonly int _index;
        private readonly AnimatableShapeValue _shapePath;

        private ShapePath(string name, int index, AnimatableShapeValue shapePath)
        {
            _name = name;
            _index = index;
            _shapePath = shapePath;
        }

        internal static class Factory
        {
            internal static ShapePath NewInstance(JsonObject json, LottieComposition composition)
            {
                var animatableShapeValue = AnimatableShapeValue.Factory.NewInstance(json.GetNamedObject("ks"), composition);
                return new ShapePath(json.GetNamedString("nm"), (int)json.GetNamedNumber("ind"), animatableShapeValue);
            }
        }

        public virtual string Name => _name;

        internal virtual AnimatableShapeValue GetShapePath()
        {
            return _shapePath;
        }

        public override string ToString()
        {
            return "ShapePath{" + "name=" + _name + ", index=" + _index + ", hasAnimation=" + _shapePath.HasAnimation() + '}';
        }
    }
}