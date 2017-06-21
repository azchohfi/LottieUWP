using System;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    internal class TransformKeyframeAnimation
    {
        private DenseMatrix _matrix = DenseMatrix.CreateIdentity(3);

        private readonly IBaseKeyframeAnimation<PointF> _anchorPoint;
        private readonly IBaseKeyframeAnimation<PointF> _position;
        private readonly IBaseKeyframeAnimation<ScaleXy> _scale;
        private readonly IBaseKeyframeAnimation<float?> _rotation;
        private readonly IBaseKeyframeAnimation<int?> _opacity;

        internal TransformKeyframeAnimation(AnimatableTransform animatableTransform)
        {
            _anchorPoint = animatableTransform.AnchorPoint.CreateAnimation();
            _position = animatableTransform.Position.CreateAnimation();
            _scale = animatableTransform.Scale.CreateAnimation();
            _rotation = animatableTransform.Rotation.CreateAnimation();
            _opacity = animatableTransform.Opacity.CreateAnimation();
        }

        internal virtual void AddAnimationsToLayer(BaseLayer layer)
        {
            layer.AddAnimation(_anchorPoint);
            layer.AddAnimation(_position);
            layer.AddAnimation(_scale);
            layer.AddAnimation(_rotation);
            layer.AddAnimation(_opacity);
        }

        internal event EventHandler ValueChanged
        {
            add
            {
                _anchorPoint.ValueChanged += value;
                _position.ValueChanged += value;
                _scale.ValueChanged += value;
                _rotation.ValueChanged += value;
                _opacity.ValueChanged += value;
            }
            remove
            {
                _anchorPoint.ValueChanged -= value;
                _position.ValueChanged -= value;
                _scale.ValueChanged -= value;
                _rotation.ValueChanged -= value;
                _opacity.ValueChanged -= value;
            }
        }

        internal virtual IBaseKeyframeAnimation<int?> Opacity => _opacity;

        internal virtual DenseMatrix Matrix
        {
            get
            {
                _matrix.Reset();
                var position = _position.Value;
                if (position != null && (position.X != 0 || position.Y != 0))
                {
                    _matrix = MatrixExt.PreTranslate(_matrix, position.X, position.Y);
                }

                if (_rotation.Value.HasValue && _rotation.Value.Value != 0f)
                {
                    _matrix = MatrixExt.PreRotate(_matrix, _rotation.Value.Value);
                }

                var scaleTransform = _scale.Value;
                if (scaleTransform != null && (scaleTransform.ScaleX != 1f || scaleTransform.ScaleY != 1f))
                {
                    _matrix = MatrixExt.PreScale(_matrix, scaleTransform.ScaleX, scaleTransform.ScaleY);
                }

                var anchorPoint = _anchorPoint.Value;
                if (anchorPoint != null && (anchorPoint.X != 0 || anchorPoint.Y != 0))
                {
                    _matrix = MatrixExt.PreTranslate(_matrix, -anchorPoint.X, -anchorPoint.Y);
                }
                return _matrix;
            }
        }
    }
}