using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    internal class Repeater : IContentModel
    {
        private Repeater(string name, AnimatableFloatValue copies, AnimatableFloatValue offset, AnimatableTransform transform)
        {
            Name = name;
            Copies = copies;
            Offset = offset;
            Transform = transform;
        }

        internal virtual string Name { get; }

        internal virtual AnimatableFloatValue Copies { get; }

        internal virtual AnimatableFloatValue Offset { get; }

        internal virtual AnimatableTransform Transform { get; }

        public IContent ToContent(LottieDrawable drawable, BaseLayer layer)
        {
            return new RepeaterContent(drawable, layer, this);
        }

        internal static class Factory
        {
            internal static Repeater NewInstance(JsonReader reader, LottieComposition composition)
            {
                string name = null;
                AnimatableFloatValue copies = null;
                AnimatableFloatValue offset = null;
                AnimatableTransform transform = null;

                while (reader.HasNext())
                {
                    switch (reader.NextName())
                    {
                        case "nm":
                            name = reader.NextString();
                            break;
                        case "c":
                            copies = AnimatableFloatValue.Factory.NewInstance(reader, composition, false);
                            break;
                        case "o":
                            offset = AnimatableFloatValue.Factory.NewInstance(reader, composition, false);
                            break;
                        case "tr":
                            transform = AnimatableTransform.Factory.NewInstance(reader, composition);
                            break;
                        default:
                            reader.SkipValue();
                            break;
                    }
                }

                return new Repeater(name, copies, offset, transform);
            }
        }
    }
}
