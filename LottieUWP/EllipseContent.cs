using System.Collections.Generic;

namespace LottieUWP
{
    internal class EllipseContent : IPathContent, BaseKeyframeAnimation.IAnimationListener
    {
        private const float EllipseControlPointPercentage = 0.55228f;

        private readonly Path _path = new Path();

        private readonly LottieDrawable _lottieDrawable;
        private readonly IBaseKeyframeAnimation<PointF> _sizeAnimation;
        private readonly IBaseKeyframeAnimation<PointF> _positionAnimation;

        private TrimPathContent _trimPath;
        private bool _isPathValid;

        internal EllipseContent(LottieDrawable lottieDrawable, BaseLayer layer, CircleShape circleShape)
        {
            Name = circleShape.Name;
            _lottieDrawable = lottieDrawable;
            _sizeAnimation = circleShape.Size.CreateAnimation();
            _positionAnimation = circleShape.Position.CreateAnimation();

            layer.AddAnimation(_sizeAnimation);
            layer.AddAnimation(_positionAnimation);

            _sizeAnimation.AddUpdateListener(this);
            _positionAnimation.AddUpdateListener(this);
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
                    _trimPath = trimPathContent;
                    _trimPath.AddListener(this);
                }
            }
        }

        public string Name { get; }

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
                // TODO: handle bounds

                var cpW = halfWidth * EllipseControlPointPercentage;
                var cpH = halfHeight * EllipseControlPointPercentage;

                _path.Reset();
                _path.MoveTo(0, -halfHeight);
                _path.CubicTo(0 + cpW, -halfHeight, halfWidth, 0 - cpH, halfWidth, 0);
                _path.CubicTo(halfWidth, 0 + cpH, 0 + cpW, halfHeight, 0, halfHeight);
                _path.CubicTo(0 - cpW, halfHeight, -halfWidth, 0 + cpH, -halfWidth, 0);
                _path.CubicTo(-halfWidth, 0 - cpH, 0 - cpW, -halfHeight, 0, -halfHeight);

                var position = _positionAnimation.Value;
                _path.Offset(position.X, position.Y);

                _path.Close();

                Utils.ApplyTrimPathIfNeeded(_path, _trimPath);

                _isPathValid = true;
                return _path;
            }
        }
    }
}