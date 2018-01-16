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

        private readonly MergePathsMode _mode;

        public MergePaths(string name, MergePathsMode mode)
        {
            Name = name;
            _mode = mode;
        }

        public virtual string Name { get; }

        internal virtual MergePathsMode Mode => _mode;

        public IContent ToContent(LottieDrawable drawable, BaseLayer layer)
        {
            if (!drawable.EnableMergePathsForKitKatAndAbove())
            {
                Debug.WriteLine("Animation contains merge paths but they are disabled.", LottieLog.Tag);
                return null;
            }
            return new MergePathsContent(this);
        }

        public override string ToString()
        {
            return "MergePaths{" + "mode=" + _mode + '}';
        }
    }
}