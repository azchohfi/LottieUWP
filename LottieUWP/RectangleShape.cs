using Windows.Data.Json;

namespace LottieUWP
{
    internal class RectangleShape
    {
        private readonly IAnimatableValue<PointF> _position;
        private readonly AnimatablePointValue _size;
        private readonly AnimatableFloatValue _cornerRadius;

        private RectangleShape(string name, IAnimatableValue<PointF> position, AnimatablePointValue size, AnimatableFloatValue cornerRadius)
        {
            Name = name;
            _position = position;
            _size = size;
            _cornerRadius = cornerRadius;
        }

        internal class Factory
        {
            internal static RectangleShape NewInstance(JsonObject json, LottieComposition composition)
            {
                return new RectangleShape(json.GetNamedString("nm"), AnimatablePathValue.CreateAnimatablePathOrSplitDimensionPath(json.GetNamedObject("p"), composition), AnimatablePointValue.Factory.NewInstance(json.GetNamedObject("s"), composition), AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("r"), composition));
            }
        }

        internal virtual string Name { get; }

        internal virtual AnimatableFloatValue CornerRadius => _cornerRadius;

        internal virtual AnimatablePointValue Size => _size;

        internal virtual IAnimatableValue<PointF> Position => _position;

        public override string ToString()
        {
            return "RectangleShape{" + "cornerRadius=" + _cornerRadius.InitialValue + ", position=" + _position + ", size=" + _size + '}';
        }
    }
}