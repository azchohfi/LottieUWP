using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    public class ShapePath : IContentModel
    {
        private readonly int _index;
        private readonly AnimatableShapeValue _shapePath;

        public ShapePath(string name, int index, AnimatableShapeValue shapePath, bool hidden)
        {
            Name = name;
            _index = index;
            _shapePath = shapePath;
            IdHidden = hidden;
        }

        public string Name { get; }

        internal AnimatableShapeValue GetShapePath()
        {
            return _shapePath;
        }

        public IContent ToContent(ILottieDrawable drawable, BaseLayer layer)
        {
            return new ShapeContent(drawable, layer, this);
        }

        public bool IdHidden { get; }

        public override string ToString()
        {
            return "ShapePath{" + "name=" + Name + ", index=" + _index + '}';
        }
    }
}