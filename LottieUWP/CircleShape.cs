using Windows.Data.Json;

namespace LottieUWP
{
    internal class CircleShape
    {
        private CircleShape(string name, IAnimatableValue<PointF> position, AnimatablePointValue size)
        {
            Name = name;
            Position = position;
            Size = size;
        }

        internal class Factory
        {
            internal static CircleShape NewInstance(JsonObject json, LottieComposition composition)
            {
                return new CircleShape(json.GetNamedString("nm"), AnimatablePathValue.CreateAnimatablePathOrSplitDimensionPath(json.GetNamedObject("p"), composition), AnimatablePointValue.Factory.NewInstance(json.GetNamedObject("s"), composition));
            }
        }

        internal virtual string Name { get; }

        public virtual IAnimatableValue<PointF> Position { get; }

        public virtual AnimatablePointValue Size { get; }
    }
}