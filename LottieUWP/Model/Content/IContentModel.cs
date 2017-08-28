using LottieUWP.Animation.Content;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    internal interface IContentModel
    {
        IContent ToContent(LottieDrawable drawable, BaseLayer layer);
    }
}
