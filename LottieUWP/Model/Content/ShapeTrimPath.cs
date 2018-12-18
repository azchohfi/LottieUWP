using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    public class ShapeTrimPath : IContentModel
    {
        public enum Type
        {
            Simultaneously = 1,
            Individually = 2
        }

        private readonly Type _type;

        public ShapeTrimPath(string name, Type type, AnimatableFloatValue start, AnimatableFloatValue end, AnimatableFloatValue offset, bool hidden)
        {
            Name = name;
            _type = type;
            Start = start;
            End = end;
            Offset = offset;
            IsHidden = hidden;
        }

        internal string Name { get; }

        internal new Type GetType()
        {
            return _type;
        }

        internal AnimatableFloatValue End { get; }

        internal AnimatableFloatValue Start { get; }

        internal AnimatableFloatValue Offset { get; }

        internal bool IsHidden { get; }

        public IContent ToContent(ILottieDrawable drawable, BaseLayer layer)
        {
            return new TrimPathContent(layer, this);
        }

        public override string ToString()
        {
            return "Trim Path: {start: " + Start + ", end: " + End + ", offset: " + Offset + "}";
        }
    }
}