using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    internal class ShapePath : IContentModel
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

        public virtual string Name => _name;

        internal virtual AnimatableShapeValue GetShapePath()
        {
            return _shapePath;
        }

        public IContent ToContent(LottieDrawable drawable, BaseLayer layer)
        {
            return new ShapeContent(drawable, layer, this);
        }

        public override string ToString()
        {
            return "ShapePath{" + "name=" + _name + ", index=" + _index + '}';
        }

        internal static class Factory
        {
            internal static ShapePath NewInstance(JsonReader reader, LottieComposition composition)
            {
                string name = null;
                int ind = 0;
                AnimatableShapeValue shape = null;

                while (reader.HasNext())
                {
                    switch (reader.NextName())
                    {
                        case "nm":
                            name = reader.NextString();
                            break;
                        case "ind":
                            ind = reader.NextInt();
                            break;
                        case "ks":
                            shape = AnimatableShapeValue.Factory.NewInstance(reader, composition);
                            break;
                        default:
                            reader.SkipValue();
                            break;
                    }
                }

                return new ShapePath(name, ind, shape);
            }
        }
    }
}