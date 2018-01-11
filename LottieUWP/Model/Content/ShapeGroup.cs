using System.Collections.Generic;
using System.Diagnostics;
using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    internal class ShapeGroup : IContentModel
    {
        internal static IContentModel ShapeItemWithJson(JsonReader reader, LottieComposition composition)
        {
            string type = null;

            reader.BeginObject();
            while (reader.HasNext())
            {
                if (reader.NextString().Equals("ty"))
                {
                    type = reader.NextString();
                    break;
                }

                reader.SkipValue();
            }

            IContentModel model = null;
            switch (type)
            {
                case "gr":
                    model = Factory.NewInstance(reader, composition);
                    break;
                case "st":
                    model = ShapeStroke.Factory.NewInstance(reader, composition);
                    break;
                case "gs":
                    model = GradientStroke.Factory.NewInstance(reader, composition);
                    break;
                case "fl":
                    model = ShapeFill.Factory.NewInstance(reader, composition);
                    break;
                case "gf":
                    model = GradientFill.Factory.NewInstance(reader, composition);
                    break;
                case "tr":
                    model = AnimatableTransform.Factory.NewInstance(reader, composition);
                    break;
                case "sh":
                    model = ShapePath.Factory.NewInstance(reader, composition);
                    break;
                case "el":
                    model = CircleShape.Factory.NewInstance(reader, composition);
                    break;
                case "rc":
                    model = RectangleShape.Factory.NewInstance(reader, composition);
                    break;
                case "tm":
                    model = ShapeTrimPath.Factory.NewInstance(reader, composition);
                    break;
                case "sr":
                    model = PolystarShape.Factory.NewInstance(reader, composition);
                    break;
                case "mm":
                    model = MergePaths.Factory.NewInstance(reader);
                    break;
                case "rp":
                    model = Repeater.Factory.NewInstance(reader, composition);
                    break;
                default:
                    Debug.WriteLine("Unknown shape type " + type, LottieLog.Tag);
                    break;
            }

            while (reader.HasNext())
            {
                reader.SkipValue();
            }
            reader.EndObject();

            return model;
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
            internal static ShapeGroup NewInstance(JsonReader reader, LottieComposition composition)
            {
                string name = null;
                var items = new List<IContentModel>();

                while (reader.HasNext())
                {
                    switch (reader.NextName())
                    {
                        case "nm":
                            name = reader.NextString();
                            break;
                        case "it":
                            reader.BeginArray();
                            while (reader.HasNext())
                            {
                                var newItem = ShapeItemWithJson(reader, composition);
                                if (newItem != null)
                                {
                                    items.Add(newItem);
                                }
                            }

                            reader.EndArray();
                            break;
                        default:
                            reader.SkipValue();
                            break;
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