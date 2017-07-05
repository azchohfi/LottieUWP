using System.Collections.Generic;
using System.Diagnostics;
using Windows.Data.Json;

namespace LottieUWP
{
    internal class ShapeGroup : IContentModel
    {
        internal static IContentModel ShapeItemWithJson(JsonObject json, LottieComposition composition)
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
                case "rp":
                    return Repeater.Factory.NewInstance(json, composition);
                default:
                    Debug.WriteLine("Unknown shape type " + type, LottieLog.Tag);
                    break;
            }
            return null;
        }

        private readonly string _name;
        private readonly List<IContentModel> _items;

        internal ShapeGroup(string name, List<IContentModel> items)
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
                var items = new List<IContentModel>();

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

        internal virtual List<IContentModel> Items => _items;

        public IContent ToContent(LottieDrawable drawable, BaseLayer layer)
        {
            return new ContentGroup(drawable, layer, this);
        }

        public override string ToString()
        {
            return "ShapeGroup{" + "name='" + _name + "\' Shapes: " + "[" + string.Join(",", _items) + "]" + "}";
        }
    }
}