using Windows.Data.Json;

namespace LottieUWP
{
    internal class ShapeFill
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

        internal class Factory
        {
            internal static ShapeFill NewInstance(JsonObject json, LottieComposition composition)
            {
                AnimatableColorValue color = null;
                bool fillEnabled;
                AnimatableIntegerValue opacity = null;
                string name = json.GetNamedString("nm");

                var jsonColor = json.GetNamedObject("c", null);
                if (jsonColor != null)
                {
                    color = AnimatableColorValue.Factory.NewInstance(jsonColor, composition);
                }

                var jsonOpacity = json.GetNamedObject("o", null);
                if (jsonOpacity != null)
                {
                    opacity = AnimatableIntegerValue.Factory.NewInstance(jsonOpacity, composition);
                }
                fillEnabled = json.GetNamedBoolean("fillEnabled", false);

                int fillTypeInt = (int)json.GetNamedNumber("r", 1);
                PathFillType fillType = fillTypeInt == 1 ? PathFillType.Winding : PathFillType.EvenOdd;

                return new ShapeFill(name, fillEnabled, fillType, color, opacity);
            }
        }

        internal virtual string Name { get; }

        internal virtual AnimatableColorValue Color => _color;

        internal virtual AnimatableIntegerValue Opacity => _opacity;

        internal virtual PathFillType FillType { get; }

        public override string ToString()
        {
            return "ShapeFill{" + "color=" + (_color == null ? "null" : string.Format("{0:X}", _color.InitialValue)) + ", fillEnabled=" + _fillEnabled + ", opacity=" + (_opacity == null ? "null" : _opacity.InitialValue.Value.ToString()) + '}';
        }
    }
}