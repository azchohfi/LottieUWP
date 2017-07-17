using System.Collections.Generic;

namespace LottieUWP
{
    public interface IContent
    {
        string Name { get; }

        void SetContents(List<IContent> contentsBefore, List<IContent> contentsAfter);
    }
}