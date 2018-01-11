using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    internal class ShapeTrimPath : IContentModel
    {
        internal enum Type
        {
            Simultaneously = 1,
            Individually = 2
        }

        private readonly Type _type;
        private readonly AnimatableFloatValue _start;
        private readonly AnimatableFloatValue _end;
        private readonly AnimatableFloatValue _offset;

        private ShapeTrimPath(string name, Type type, AnimatableFloatValue start, AnimatableFloatValue end, AnimatableFloatValue offset)
        {
            Name = name;
            _type = type;
            _start = start;
            _end = end;
            _offset = offset;
        }

        internal virtual string Name { get; }

        internal new virtual Type GetType()
        {
            return _type;
        }

        internal virtual AnimatableFloatValue End => _end;

        internal virtual AnimatableFloatValue Start => _start;

        internal virtual AnimatableFloatValue Offset => _offset;

        public IContent ToContent(LottieDrawable drawable, BaseLayer layer)
        {
            return new TrimPathContent(layer, this);
        }

        public override string ToString()
        {
            return "Trim Path: {start: " + _start + ", end: " + _end + ", offset: " + _offset + "}";
        }

        internal static class Factory
        {
            internal static ShapeTrimPath NewInstance(JsonReader reader, LottieComposition composition)
            {
                string name = null;
                Type type = Type.Simultaneously;
                AnimatableFloatValue start = null;
                AnimatableFloatValue end = null;
                AnimatableFloatValue offset = null;

                while (reader.HasNext())
                {
                    switch (reader.NextName())
                    {
                        case "s":
                            start = AnimatableFloatValue.Factory.NewInstance(reader, composition, false);
                            break;
                        case "e":
                            end = AnimatableFloatValue.Factory.NewInstance(reader, composition, false);
                            break;
                        case "o":
                            offset = AnimatableFloatValue.Factory.NewInstance(reader, composition, false);
                            break;
                        case "nm":
                            name = reader.NextString();
                            break;
                        case "m":
                            type = (Type)reader.NextInt();
                            break;
                        default:
                            reader.SkipValue();
                            break;
                    }
                }

                return new ShapeTrimPath(name, type, start, end, offset);
            }
        }
    }
}