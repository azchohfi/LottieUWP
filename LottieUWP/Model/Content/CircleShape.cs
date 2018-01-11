using System.Numerics;
using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    internal class CircleShape : IContentModel
    {
        private CircleShape(string name, IAnimatableValue<Vector2?, Vector2?> position, AnimatablePointValue size, bool isReversed)
        {
            Name = name;
            Position = position;
            Size = size;
            IsReversed = isReversed;
        }

        internal static class Factory
        {
            internal static CircleShape NewInstance(JsonReader reader, LottieComposition composition)
            {
                string name = null;
                IAnimatableValue<Vector2?, Vector2?> position = null;
                AnimatablePointValue size = null;
                bool reversed = false;

                while (reader.HasNext())
                {
                    switch (reader.NextName())
                    {
                        case "nm":
                            name = reader.NextString();
                            break;
                        case "p":
                            position = AnimatablePathValue.CreateAnimatablePathOrSplitDimensionPath(reader, composition);
                            break;
                        case "s":
                            size = AnimatablePointValue.Factory.NewInstance(reader, composition);
                            break;
                        case "d":
                            // "d" is 2 for normal and 3 for reversed. 
                            reversed = reader.NextInt() == 3;
                            break;
                        default:
                            reader.SkipValue();
                            break;
                    }
                }

                return new CircleShape(name, position, size, reversed);
            }
        }

        internal string Name { get; }

        public IAnimatableValue<Vector2?, Vector2?> Position { get; }

        public AnimatablePointValue Size { get; }

        public bool IsReversed { get; }

        public IContent ToContent(LottieDrawable drawable, BaseLayer layer)
        {
            return new EllipseContent(drawable, layer, this);
        }
    }
}