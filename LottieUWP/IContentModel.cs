namespace LottieUWP
{
    internal interface IContentModel
    {
        IContent ToContent(LottieDrawable drawable, BaseLayer layer);
    }
}
