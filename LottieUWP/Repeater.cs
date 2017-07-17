using Windows.Data.Json;

namespace LottieUWP
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
            internal static Repeater NewInstance(JsonObject json, LottieComposition composition)
            {
                var name = json.GetNamedString("nm", "");
                var copies = AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("c", null), composition, false);
                var offset = AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("o", null), composition, false);
                var transform = AnimatableTransform.Factory.NewInstance(json.GetNamedObject("tr", null), composition);

                return new Repeater(name, copies, offset, transform);
            }
        }
    }
}
