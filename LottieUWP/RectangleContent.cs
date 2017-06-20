using System;
using System.Collections.Generic;

namespace LottieUWP
{
    internal class RectangleContent : IPathContent, BaseKeyframeAnimation.IAnimationListener
    {
        private readonly Path _path = new Path();

        private readonly LottieDrawable _lottieDrawable;
        private readonly IBaseKeyframeAnimation<PointF> _positionAnimation;
        private readonly IBaseKeyframeAnimation<PointF> _sizeAnimation;
        private readonly IBaseKeyframeAnimation<float?> _cornerRadiusAnimation;

        private TrimPathContent _trimPath;
        private bool _isPathValid;

        internal RectangleContent(LottieDrawable lottieDrawable, BaseLayer layer, RectangleShape rectShape)
        {
            Name = rectShape.Name;
            _lottieDrawable = lottieDrawable;
            _positionAnimation = rectShape.Position.CreateAnimation();
            _sizeAnimation = rectShape.Size.CreateAnimation();
            _cornerRadiusAnimation = rectShape.CornerRadius.CreateAnimation();

            layer.AddAnimation(_positionAnimation);
            layer.AddAnimation(_sizeAnimation);
            layer.AddAnimation(_cornerRadiusAnimation);

            _positionAnimation.AddUpdateListener(this);
            _sizeAnimation.AddUpdateListener(this);
            _cornerRadiusAnimation.AddUpdateListener(this);
        }

        public string Name { get; }

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

                var size = _sizeAnimation.Value;
                var halfWidth = size.X / 2f;
                var halfHeight = size.Y / 2f;
                var radius = _cornerRadiusAnimation?.Value ?? 0f;
                var maxRadius = Math.Min(halfWidth, halfHeight);
                if (radius > maxRadius)
                {
                    radius = maxRadius;
                }

                // Draw the rectangle top right to bottom left.
                var position = _positionAnimation.Value;

                _path.MoveTo(position.X + halfWidth, position.Y - halfHeight + radius);

                _path.LineTo(position.X + halfWidth, position.Y + halfHeight - radius);

                if (radius > 0)
                {
                    _path.ArcTo(position.X + halfWidth - radius, position.Y + halfHeight - radius, 90);
                }

                _path.LineTo(position.X - halfWidth + radius, position.Y + halfHeight);

                if (radius > 0)
                {
                    _path.ArcTo(position.X - halfWidth, position.Y + halfHeight - radius, 90);
                }

                _path.LineTo(position.X - halfWidth, position.Y - halfHeight + radius);

                if (radius > 0)
                {
                    _path.ArcTo(position.X - halfWidth + radius, position.Y - halfHeight, 90);
                }

                _path.LineTo(position.X + halfWidth - radius, position.Y - halfHeight);

                if (radius > 0)
                {
                    _path.ArcTo(position.X + halfWidth, position.Y - halfHeight + radius, 90);
                }
                _path.Close();

                Utils.ApplyTrimPathIfNeeded(_path, _trimPath);

                _isPathValid = true;
                return _path;
            }
        }
    }
}