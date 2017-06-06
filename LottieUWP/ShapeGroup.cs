using System.Collections.Generic;
using System.Diagnostics;
using Windows.Data.Json;

namespace LottieUWP
{
    internal class ShapeGroup
    {
        internal static object ShapeItemWithJson(JsonObject json, LottieComposition composition)
        {
            var type = json.GetNamedString("ty");

            switch (type)
            {
                case "gr":
                    return Factory.NewInstance(json, composition);
                case "st":
                    return ShapeStroke.Factory.NewInstance(json, composition);
                case "gs":
                    return GradientStroke.Factory.NewInstance(json, composition);
                case "fl":
                    return ShapeFill.Factory.NewInstance(json, composition);
                case "gf":
                    return GradientFill.Factory.NewInstance(json, composition);
                case "tr":
                    return AnimatableTransform.Factory.NewInstance(json, composition);
                case "sh":
                    return ShapePath.Factory.NewInstance(json, composition);
                case "el":
                    return CircleShape.Factory.NewInstance(json, composition);
                case "rc":
                    return RectangleShape.Factory.NewInstance(json, composition);
                case "tm":
                    return ShapeTrimPath.Factory.NewInstance(json, composition);
                case "sr":
                    return PolystarShape.Factory.NewInstance(json, composition);
                case "mm":
                    return MergePaths.Factory.NewInstance(json);
                default:
                    Debug.WriteLine("Unknown shape type " + type, "LOTTIE");
                    break;
            }
            return null;
        }

        private readonly string _name;
        private readonly IList<object> _items;

        internal ShapeGroup(string name, IList<object> items)
        {
            _name = name;
            _items = items;
        }

        internal static class Factory
        {
            internal static ShapeGroup NewInstance(JsonObject json, LottieComposition composition)
            {
                var jsonItems = json.GetNamedArray("it");
                var name = json.GetNamedString("nm");
                IList<object> items = new List<object>();

                for (uint i = 0; i < jsonItems.Count; i++)
                {
                    var newItem = ShapeItemWithJson(jsonItems.GetObjectAt(i), composition);
                    if (newItem != null)
                    {
                        items.Add(newItem);
                    }
                }
                return new ShapeGroup(name, items);
            }
        }

        public virtual string Name => _name;

        internal virtual IList<object> Items => _items;

        public override string ToString()
        {
            return "ShapeGroup{" + "name='" + _name + "\' Shapes: " + "[" + string.Join(",", _items) + "]" + "}";
        }
    }
}