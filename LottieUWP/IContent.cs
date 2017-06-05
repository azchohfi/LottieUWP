using System.Collections.Generic;

namespace LottieUWP
{
    public interface IContent
    {
        string Name { get; }

        void SetContents(IList<IContent> contentsBefore, IList<IContent> contentsAfter);
    }
}