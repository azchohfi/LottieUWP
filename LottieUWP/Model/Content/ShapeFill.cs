using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    internal class ShapeFill : IContentModel
    {
        private readonly bool _fillEnabled;
        private readonly AnimatableColorValue _color;
        private readonly AnimatableIntegerValue _opacity;

        private ShapeFill(string name, bool fillEnabled, PathFillType fillType, AnimatableColorValue color, AnimatableIntegerValue opacity)
        {
            Name = name;
            _fillEnabled = fillEnabled;
            FillType = fillType;
            _color = color;
            _opacity = opacity;
        }

        internal static class Factory
        {
            internal static ShapeFill NewInstance(JsonReader reader, LottieComposition composition)
            {
                AnimatableColorValue color = null;
                bool fillEnabled = false;
                AnimatableIntegerValue opacity = null;
                string name = null;
                int fillTypeInt = 1;

                while (reader.HasNext())
                {
                    switch (reader.NextName())
                    {
                        case "nm":
                            name = reader.NextString();
                            break;
                        case "c":
                            color = AnimatableColorValue.Factory.NewInstance(reader, composition);
                            break;
                        case "o":
                            opacity = AnimatableIntegerValue.Factory.NewInstance(reader, composition);
                            break;
                        case "fillEnabled":
                            fillEnabled = reader.NextBoolean();
                            break;
                        case "r":
                            fillTypeInt = reader.NextInt();
                            break;
                        default:
                            reader.SkipValue();
                            break;
                    }
                }

                var fillType = fillTypeInt == 1 ? PathFillType.Winding : PathFillType.EvenOdd;
                return new ShapeFill(name, fillEnabled, fillType, color, opacity);
            }
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