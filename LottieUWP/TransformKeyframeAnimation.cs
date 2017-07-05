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

        // Used for repeaters 
        private readonly IBaseKeyframeAnimation<float?> _startOpacity;
        private readonly IBaseKeyframeAnimation<float?> _endOpacity;

        internal TransformKeyframeAnimation(AnimatableTransform animatableTransform)
        {
            _anchorPoint = animatableTransform.AnchorPoint.CreateAnimation();
            _position = animatableTransform.Position.CreateAnimation();
            _scale = animatableTransform.Scale.CreateAnimation();
            _rotation = animatableTransform.Rotation.CreateAnimation();
            _opacity = animatableTransform.Opacity.CreateAnimation();
            _startOpacity = animatableTransform.StartOpacity?.CreateAnimation();
            _endOpacity = animatableTransform.EndOpacity?.CreateAnimation();
        }

        internal virtual void AddAnimationsToLayer(BaseLayer layer)
        {
            layer.AddAnimation(_anchorPoint);
            layer.AddAnimation(_position);
            layer.AddAnimation(_scale);
            layer.AddAnimation(_rotation);
            layer.AddAnimation(_opacity);
            if (_startOpacity != null)
            {
                layer.AddAnimation(_startOpacity);
            }
            if (_endOpacity != null)
            {
                layer.AddAnimation(_endOpacity);
            }
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
                if (_startOpacity != null)
                {
                    _startOpacity.ValueChanged += value;
                }
                if (_endOpacity != null)
                {
                    _endOpacity.ValueChanged += value;
                }
            }
            remove
            {
                _anchorPoint.ValueChanged -= value;
                _position.ValueChanged -= value;
                _scale.ValueChanged -= value;
                _rotation.ValueChanged -= value;
                _opacity.ValueChanged -= value;
                if (_startOpacity != null)
                {
                    _startOpacity.ValueChanged -= value;
                }
                if (_endOpacity != null)
                {
                    _endOpacity.ValueChanged -= value;
                }
            }
        }

        internal virtual IBaseKeyframeAnimation<int?> Opacity => _opacity;

        internal virtual IBaseKeyframeAnimation<float?> StartOpacity => _startOpacity;

        internal virtual IBaseKeyframeAnimation<float?> EndOpacity => _endOpacity;

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

        /** 
        * TODO: see if we can use this for the main get_Matrix method. 
        */
        internal DenseMatrix GetMatrixForRepeater(float amount)
        {
            var position = _position.Value;
            var anchorPoint = _anchorPoint.Value;
            var scale = _scale.Value;
            var rotation = _rotation.Value.Value;

            _matrix.Reset();
            _matrix = MatrixExt.PreTranslate(_matrix, position.X * amount, position.Y * amount);
            _matrix = MatrixExt.PreScale(_matrix,
                (float)Math.Pow(scale.ScaleX, amount),
                (float)Math.Pow(scale.ScaleY, amount));
            _matrix = MatrixExt.PreRotate(_matrix, rotation * amount, anchorPoint.X, anchorPoint.Y);

            return _matrix;
        }
    }
}