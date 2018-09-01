using LottieUWP.Animation.Content;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    public interface IContentModel
    {
        IContent ToContent(ILottieDrawable drawable, BaseLayer layer);
    }
}
