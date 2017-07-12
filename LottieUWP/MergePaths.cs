using System.Diagnostics;
using Windows.Data.Json;

namespace LottieUWP
{
    internal class MergePaths : IContentModel
    {
        internal enum MergePathsMode
        {
            Merge = 1,
            Add = 2,
            Subtract = 3,
            Intersect = 4,
            ExcludeIntersections = 5
        }

        private readonly MergePathsMode _mode;

        private MergePaths(string name, MergePathsMode mode)
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

        internal static class Factory
        {
            internal static MergePaths NewInstance(JsonObject json)
            {
                return new MergePaths(json.GetNamedString("nm"), (MergePathsMode)(int)json.GetNamedNumber("mm", 1));
            }
        }
    }
}