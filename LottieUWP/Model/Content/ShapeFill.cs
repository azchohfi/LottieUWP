using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    public class ShapeFill : IContentModel
    {
        private readonly bool _fillEnabled;

        public ShapeFill(string name, bool fillEnabled, PathFillType fillType, AnimatableColorValue color, AnimatableIntegerValue opacity, bool hidden)
        {
            Name = name;
            _fillEnabled = fillEnabled;
            FillType = fillType;
            Color = color;
            Opacity = opacity;
            IsHidden = hidden;
        }

        internal string Name { get; }

        internal AnimatableColorValue Color { get; }

        internal AnimatableIntegerValue Opacity { get; }

        internal PathFillType FillType { get; }

        internal bool IsHidden { get; }

        public IContent ToContent(ILottieDrawable drawable, BaseLayer layer)
        {
            return new FillContent(drawable, layer, this);
        }

        public override string ToString()
        {
            return "ShapeFill{" + "color=" + ", fillEnabled=" + _fillEnabled + '}';
        }
    }
}