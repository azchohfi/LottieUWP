using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    public class ShapePath : IContentModel
    {
        private readonly string _name;
        private readonly int _index;
        private readonly AnimatableShapeValue _shapePath;

        public ShapePath(string name, int index, AnimatableShapeValue shapePath)
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
    }
}