using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    internal class TransformKeyframeAnimation
    {
        private DenseMatrix _matrix = new DenseMatrix(3, 3);

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

        internal virtual void AddListener(BaseKeyframeAnimation.IAnimationListener listener)
        {
            _anchorPoint.AddUpdateListener(listener);
            _position.AddUpdateListener(listener);
            _scale.AddUpdateListener(listener);
            _rotation.AddUpdateListener(listener);
            _opacity.AddUpdateListener(listener);
        }

        internal virtual IBaseKeyframeAnimation<int?> Opacity => _opacity;

        internal virtual DenseMatrix Matrix
        {
            get
            {
                _matrix.Reset();
                PointF position = _position.Value;
                if (position != null && (position.X != 0 || position.Y != 0))
                {
                    _matrix = MatrixExt.PreTranslate(_matrix, position.X, position.Y);
                }

                float rotation = _rotation.Value.Value;
                if (rotation != 0f)
                {
                    _matrix = MatrixExt.PreRotate(_matrix, rotation);
                }

                ScaleXy scaleTransform = _scale.Value;
                if (scaleTransform.ScaleX != 1f || scaleTransform.ScaleY != 1f)
                {
                    _matrix = MatrixExt.PreScale(_matrix, scaleTransform.ScaleX, scaleTransform.ScaleY);
                }

                PointF anchorPoint = _anchorPoint.Value;
                if (anchorPoint.X != 0 || anchorPoint.Y != 0)
                {
                    _matrix = MatrixExt.PreTranslate(_matrix, -anchorPoint.X, -anchorPoint.Y);
                }
                return _matrix;
            }
        }
    }
}