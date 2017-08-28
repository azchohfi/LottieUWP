using System.Collections.Generic;

namespace LottieUWP.Animation.Content
{
    public interface IContent
    {
        string Name { get; }

        void SetContents(List<IContent> contentsBefore, List<IContent> contentsAfter);
    }
}