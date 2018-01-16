using System.Collections.Generic;
using LottieUWP.Animation.Content;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    public class ShapeGroup : IContentModel
    {
        private readonly string _name;
        private readonly List<IContentModel> _items;

        public ShapeGroup(string name, List<IContentModel> items)
        {
            _name = name;
            _items = items;
        }

        public virtual string Name => _name;

        public virtual List<IContentModel> Items => _items;

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