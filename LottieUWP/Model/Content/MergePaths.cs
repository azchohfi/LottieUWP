using System.Diagnostics;
using LottieUWP.Animation.Content;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    public class MergePaths : IContentModel
    {
        public enum MergePathsMode
        {
            Merge = 1,
            Add = 2,
            Subtract = 3,
            Intersect = 4,
            ExcludeIntersections = 5
        }

        public MergePaths(string name, MergePathsMode mode, bool hidden)
        {
            Name = name;
            Mode = mode;
            IsHidden = hidden;
        }

        public string Name { get; }

        internal MergePathsMode Mode { get; }

        internal bool IsHidden { get; }

        public IContent ToContent(ILottieDrawable drawable, BaseLayer layer)
        {
            if (!drawable.EnableMergePaths())
            {
                Debug.WriteLine("Animation contains merge paths but they are disabled.", LottieLog.Tag);
                return null;
            }
            return new MergePathsContent(this);
        }

        public override string ToString()
        {
            return "MergePaths{" + "mode=" + Mode + '}';
        }
    }
}