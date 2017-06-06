using System.Collections.Generic;

namespace LottieUWP
{
    internal class ShapeContent : IPathContent, BaseKeyframeAnimation.IAnimationListener
    {
        private readonly Path _path = new Path();

        private readonly LottieDrawable _lottieDrawable;
        private readonly IBaseKeyframeAnimation<Path> _shapeAnimation;

        private bool _isPathValid;
        private TrimPathContent _trimPath;

        internal ShapeContent(LottieDrawable lottieDrawable, BaseLayer layer, ShapePath shape)
        {
            Name = shape.Name;
            _lottieDrawable = lottieDrawable;
            _shapeAnimation = shape.GetShapePath().CreateAnimation();
            layer.AddAnimation(_shapeAnimation);
            _shapeAnimation.AddUpdateListener(this);
        }

        public void OnValueChanged()
        {
            Invalidate();
        }

        private void Invalidate()
        {
            _isPathValid = false;
            _lottieDrawable.InvalidateSelf();
        }

        public void SetContents(IList<IContent> contentsBefore, IList<IContent> contentsAfter)
        {
            for (var i = 0; i < contentsBefore.Count; i++)
            {
                if (contentsBefore[i] is TrimPathContent trimPathContent && trimPathContent.Type == ShapeTrimPath.Type.Simultaneously)
                {
                    // Trim path individually will be handled by the stroke where paths are combined.
                    _trimPath = trimPathContent;
                    _trimPath.AddListener(this);
                }
            }
        }

        public Path Path
        {
            get
            {
                if (_isPathValid)
                {
                    return _path;
                }

                _path.Reset();

                _path.Set(_shapeAnimation.Value);
                _path.FillType = PathFillType.EvenOdd;

                Utils.ApplyTrimPathIfNeeded(_path, _trimPath);

                _isPathValid = true;
                return _path;
            }
        }

        public string Name { get; }
    }
}