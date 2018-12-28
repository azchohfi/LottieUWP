using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    public class Repeater : IContentModel
    {
        public Repeater(string name, AnimatableFloatValue copies, AnimatableFloatValue offset, AnimatableTransform transform, bool hidden)
        {
            Name = name;
            Copies = copies;
            Offset = offset;
            Transform = transform;
            IsHidden = hidden;
        }

        internal string Name { get; }

        internal AnimatableFloatValue Copies { get; }

        internal AnimatableFloatValue Offset { get; }

        internal AnimatableTransform Transform { get; }

        internal bool IsHidden { get; }

        public IContent ToContent(ILottieDrawable drawable, BaseLayer layer)
        {
            return new RepeaterContent(drawable, layer, this);
        }
    }
}
