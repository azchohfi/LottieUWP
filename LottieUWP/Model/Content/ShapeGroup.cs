using System.Collections.Generic;
using LottieUWP.Animation.Content;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    public class ShapeGroup : IContentModel
    {
        public ShapeGroup(string name, List<IContentModel> items, bool hidden)
        {
            Name = name;
            Items = items;
            IsHidden = hidden;
        }

        public string Name { get; }

        public List<IContentModel> Items { get; }

        public bool IsHidden { get; }

        public IContent ToContent(ILottieDrawable drawable, BaseLayer layer)
        {
            return new ContentGroup(drawable, layer, this);
        }

        public override string ToString()
        {
            return "ShapeGroup{" + "name='" + Name + "\' Shapes: " + "[" + string.Join(",", Items) + "]" + "}";
        }
    }
}