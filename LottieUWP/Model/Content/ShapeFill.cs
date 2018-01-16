using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    public class ShapeFill : IContentModel
    {
        private readonly bool _fillEnabled;
        private readonly AnimatableColorValue _color;
        private readonly AnimatableIntegerValue _opacity;

        public ShapeFill(string name, bool fillEnabled, PathFillType fillType, AnimatableColorValue color, AnimatableIntegerValue opacity)
        {
            Name = name;
            _fillEnabled = fillEnabled;
            FillType = fillType;
            _color = color;
            _opacity = opacity;
        }

        internal virtual string Name { get; }

        internal virtual AnimatableColorValue Color => _color;

        internal virtual AnimatableIntegerValue Opacity => _opacity;

        internal virtual PathFillType FillType { get; }

        public IContent ToContent(LottieDrawable drawable, BaseLayer layer)
        {
            return new FillContent(drawable, layer, this);
        }

        public override string ToString()
        {
            return "ShapeFill{" + "color=" + ", fillEnabled=" + _fillEnabled + '}';
        }
    }
}