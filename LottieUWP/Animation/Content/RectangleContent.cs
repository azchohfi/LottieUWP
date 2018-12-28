using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using LottieUWP.Animation.Keyframe;
using LottieUWP.Model;
using LottieUWP.Model.Content;
using LottieUWP.Model.Layer;
using LottieUWP.Utils;
using LottieUWP.Value;

namespace LottieUWP.Animation.Content
{
    internal class RectangleContent : IPathContent, IKeyPathElementContent
    {
        private readonly Path _path = new Path();
        private Rect _rect;

        private readonly bool _hidden;
        private readonly ILottieDrawable _lottieDrawable;
        private readonly IBaseKeyframeAnimation<Vector2?, Vector2?> _positionAnimation;
        private readonly IBaseKeyframeAnimation<Vector2?, Vector2?> _sizeAnimation;
        private readonly IBaseKeyframeAnimation<float?, float?> _cornerRadiusAnimation;

        private TrimPathContent _trimPath;
        private bool _isPathValid;

        internal RectangleContent(ILottieDrawable lottieDrawable, BaseLayer layer, RectangleShape rectShape)
        {
            Name = rectShape.Name;
            _hidden = rectShape.IsHidden;
            _lottieDrawable = lottieDrawable;
            _positionAnimation = rectShape.Position.CreateAnimation();
            _sizeAnimation = rectShape.Size.CreateAnimation();
            _cornerRadiusAnimation = rectShape.CornerRadius.CreateAnimation();

            layer.AddAnimation(_positionAnimation);
            layer.AddAnimation(_sizeAnimation);
            layer.AddAnimation(_cornerRadiusAnimation);

            _positionAnimation.ValueChanged += OnValueChanged;
            _sizeAnimation.ValueChanged += OnValueChanged;
            _cornerRadiusAnimation.ValueChanged += OnValueChanged;
        }

        public string Name { get; }

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

                var size = _sizeAnimation.Value;
                var halfWidth = size.Value.X / 2f;
                var halfHeight = size.Value.Y / 2f;
                var radius = _cornerRadiusAnimation?.Value ?? 0f;
                var maxRadius = Math.Min(halfWidth, halfHeight);
                if (radius > maxRadius)
                {
                    radius = maxRadius;
                }

                // Draw the rectangle top right to bottom left.
                var position = _positionAnimation.Value.Value;

                _path.MoveTo(position.X + halfWidth, position.Y - halfHeight + radius);

                _path.LineTo(position.X + halfWidth, position.Y + halfHeight - radius);

                if (radius > 0)
                {
                    RectExt.Set(ref _rect, position.X + halfWidth - 2 * radius, position.Y + halfHeight - 2 * radius, position.X + halfWidth, position.Y + halfHeight);
                    _path.ArcTo(position.X + halfWidth, position.Y + halfHeight - radius, _rect, 0, 90);
                }

                _path.LineTo(position.X - halfWidth + radius, position.Y + halfHeight);
                
                if (radius > 0)
                {
                    RectExt.Set(ref _rect, position.X - halfWidth, position.Y + halfHeight - 2 * radius, position.X - halfWidth + 2 * radius, position.Y + halfHeight);
                    _path.ArcTo(position.X - halfWidth + radius, position.Y + halfHeight, _rect, 90, 90);
                }
                
                _path.LineTo(position.X - halfWidth, position.Y - halfHeight + radius);
                
                if (radius > 0)
                {
                    RectExt.Set(ref _rect, position.X - halfWidth, position.Y - halfHeight, position.X - halfWidth + 2 * radius, position.Y - halfHeight + 2 * radius);
                    _path.ArcTo(position.X - halfWidth, position.Y - halfHeight + radius, _rect, 180, 90);
                }
                
                _path.LineTo(position.X + halfWidth - radius, position.Y - halfHeight);
                
                if (radius > 0)
                {
                    RectExt.Set(ref _rect, position.X + halfWidth - 2 * radius, position.Y - halfHeight, position.X + halfWidth, position.Y - halfHeight + 2 * radius);
                    _path.ArcTo(position.X + halfWidth - radius, position.Y - halfHeight, _rect, 270, 90);
                }
                _path.Close();

                Utils.Utils.ApplyTrimPathIfNeeded(_path, _trimPath);

                _isPathValid = true;
                return _path;
            }
        }

        public void ResolveKeyPath(KeyPath keyPath, int depth, List<KeyPath> accumulator, KeyPath currentPartialKeyPath)
        {
            MiscUtils.ResolveKeyPath(keyPath, depth, accumulator, currentPartialKeyPath, this);
        }

        public void AddValueCallback<T>(LottieProperty property, ILottieValueCallback<T> callback)
        {

        }
    }
}