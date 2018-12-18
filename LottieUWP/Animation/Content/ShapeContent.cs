using System;
using System.Collections.Generic;
using LottieUWP.Animation.Keyframe;
using LottieUWP.Model.Content;
using LottieUWP.Model.Layer;

namespace LottieUWP.Animation.Content
{
    internal class ShapeContent : IPathContent
    {
        private readonly Path _path = new Path();

        private readonly bool _hidden;
        private readonly ILottieDrawable _lottieDrawable;
        private readonly IBaseKeyframeAnimation<ShapeData, Path> _shapeAnimation;

        private bool _isPathValid;
        private TrimPathContent _trimPath;

        internal ShapeContent(ILottieDrawable lottieDrawable, BaseLayer layer, ShapePath shape)
        {
            Name = shape.Name;
            _hidden = shape.IdHidden;
            _lottieDrawable = lottieDrawable;
            _shapeAnimation = shape.GetShapePath().CreateAnimation();
            layer.AddAnimation(_shapeAnimation);
            _shapeAnimation.ValueChanged += OnValueChanged;
        }

        private void OnValueChanged(object sender, EventArgs eventArgs)
        {
            Invalidate();
        }

        private void Invalidate()
        {
            _isPathValid = false;
            _lottieDrawable.InvalidateSelf();
        }

        public void SetContents(List<IContent> contentsBefore, List<IContent> contentsAfter)
        {
            for (var i = 0; i < contentsBefore.Count; i++)
            {
                if (contentsBefore[i] is TrimPathContent trimPathContent && trimPathContent.Type == ShapeTrimPath.Type.Simultaneously)
                {
                    // Trim path individually will be handled by the stroke where paths are combined.
                    _trimPath = trimPathContent;
                    _trimPath.ValueChanged += OnValueChanged;
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

                if (_hidden)
                {
                    _isPathValid = true;
                    return _path;
                }

                _path.Set(_shapeAnimation.Value);
                _path.FillType = PathFillType.EvenOdd;

                Utils.Utils.ApplyTrimPathIfNeeded(_path, _trimPath);

                _isPathValid = true;
                return _path;
            }
        }

        public string Name { get; }
    }
}