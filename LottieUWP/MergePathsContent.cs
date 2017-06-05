using System.Collections.Generic;

namespace LottieUWP
{
    internal class MergePathsContent : IPathContent
    {
        private readonly Path _firstPath = new Path();
        private readonly Path _remainderPath = new Path();
        private readonly Path _path = new Path();

        private readonly IList<IPathContent> _pathContents = new List<IPathContent>();
        private readonly MergePaths _mergePaths;

        internal MergePathsContent(MergePaths mergePaths)
        {
            Name = mergePaths.Name;
            _mergePaths = mergePaths;
        }

        internal virtual void AddContentIfNeeded(IContent content)
        {
            var pathContent = content as IPathContent;
            if (pathContent != null)
            {
                _pathContents.Add(pathContent);
            }
        }

        public void SetContents(IList<IContent> contentsBefore, IList<IContent> contentsAfter)
        {
            for (int i = 0; i < _pathContents.Count; i++)
            {
                _pathContents[i].SetContents(contentsBefore, contentsAfter);
            }
        }

        public virtual Path Path
        {
            get
            {
                _path.Reset();

                switch (_mergePaths.Mode.InnerEnumValue)
                {
                    case MergePaths.MergePathsMode.InnerEnum.Merge:
                        AddPaths();
                        break;
                    case MergePaths.MergePathsMode.InnerEnum.Add:
                        OpFirstPathWithRest(Op.Union);
                        break;
                    case MergePaths.MergePathsMode.InnerEnum.Subtract:
                        OpFirstPathWithRest(Op.ReverseDifference);
                        break;
                    case MergePaths.MergePathsMode.InnerEnum.Intersect:
                        OpFirstPathWithRest(Op.Intersect);
                        break;
                    case MergePaths.MergePathsMode.InnerEnum.ExcludeIntersections:
                        OpFirstPathWithRest(Op.Xor);
                        break;
                }

                return _path;
            }
        }

        public string Name { get; }

        private void AddPaths()
        {
            for (int i = 0; i < _pathContents.Count; i++)
            {
                _path.AddPath(_pathContents[i].Path);
            }
        }

        private void OpFirstPathWithRest(Op op)
        {
            _remainderPath.Reset();
            _firstPath.Reset();

            for (int i = _pathContents.Count - 1; i >= 1; i--)
            {
                IPathContent content = _pathContents[i];

                var contentGroup = content as ContentGroup;
                if (contentGroup != null)
                {
                    IList<IPathContent> pathList = contentGroup.PathList;
                    for (int j = pathList.Count - 1; j >= 0; j--)
                    {
                        Path path = pathList[j].Path;
                        path.Transform(contentGroup.TransformationMatrix);
                        _remainderPath.AddPath(path);
                    }
                }
                else
                {
                    _remainderPath.AddPath(content.Path);
                }
            }

            IPathContent lastContent = _pathContents[0];
            var group = lastContent as ContentGroup;
            if (group != null)
            {
                IList<IPathContent> pathList = group.PathList;
                for (int j = 0; j < pathList.Count; j++)
                {
                    Path path = pathList[j].Path;
                    path.Transform(group.TransformationMatrix);
                    _firstPath.AddPath(path);
                }
            }
            else
            {
                _firstPath.Set(lastContent.Path);
            }

            _path.Op(_firstPath, _remainderPath, op);
        }
    }
}