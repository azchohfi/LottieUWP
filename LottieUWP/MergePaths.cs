using System.Collections.Generic;
using System.Diagnostics;
using Windows.Data.Json;

namespace LottieUWP
{
    internal class MergePaths : IContentModel
    {
        internal sealed class MergePathsMode
        {
            public static readonly MergePathsMode Merge = new MergePathsMode("Merge", InnerEnum.Merge);
            public static readonly MergePathsMode Add = new MergePathsMode("Add", InnerEnum.Add);
            public static readonly MergePathsMode Subtract = new MergePathsMode("Subtract", InnerEnum.Subtract);
            public static readonly MergePathsMode Intersect = new MergePathsMode("Intersect", InnerEnum.Intersect);
            public static readonly MergePathsMode ExcludeIntersections = new MergePathsMode("ExcludeIntersections", InnerEnum.ExcludeIntersections);

            private static readonly IList<MergePathsMode> ValueList = new List<MergePathsMode>();

            static MergePathsMode()
            {
                ValueList.Add(Merge);
                ValueList.Add(Add);
                ValueList.Add(Subtract);
                ValueList.Add(Intersect);
                ValueList.Add(ExcludeIntersections);
            }

            public enum InnerEnum
            {
                Merge,
                Add,
                Subtract,
                Intersect,
                ExcludeIntersections
            }

            public readonly InnerEnum InnerEnumValue;
            private readonly string _nameValue;
            private readonly int _ordinalValue;
            private static int _nextOrdinal;

            private MergePathsMode(string name, InnerEnum innerEnum)
            {
                _nameValue = name;
                _ordinalValue = _nextOrdinal++;
                InnerEnumValue = innerEnum;
            }

            internal static MergePathsMode ForId(int id)
            {
                switch (id)
                {
                    case 1:
                        return Merge;
                    case 2:
                        return Add;
                    case 3:
                        return Subtract;
                    case 4:
                        return Intersect;
                    case 5:
                        return ExcludeIntersections;
                    default:
                        return Merge;
                }
            }

            public static IList<MergePathsMode> Values()
            {
                return ValueList;
            }

            public int Ordinal()
            {
                return _ordinalValue;
            }

            public override string ToString()
            {
                return _nameValue;
            }

            public static MergePathsMode ValueOf(string name)
            {
                foreach (var enumInstance in ValueList)
                {
                    if (enumInstance._nameValue == name)
                    {
                        return enumInstance;
                    }
                }
                throw new System.ArgumentException(name);
            }
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
                return new MergePaths(json.GetNamedString("nm"), MergePathsMode.ForId((int)json.GetNamedNumber("mm", 1)));
            }
        }
    }
}