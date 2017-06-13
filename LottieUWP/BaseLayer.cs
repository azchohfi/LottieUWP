using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    internal abstract class BaseLayer : IDrawingContent, BaseKeyframeAnimation.IAnimationListener
    {
        private static readonly int SaveFlags = BitmapCanvas.ClipSaveFlag | BitmapCanvas.ClipToLayerSaveFlag | BitmapCanvas.MatrixSaveFlag;

        internal static BaseLayer ForModel(Layer layerModel, LottieDrawable drawable, LottieComposition composition)
        {
            switch (layerModel.GetLayerType())
            {
                case Layer.LayerType.Shape:
                    return new ShapeLayer(drawable, layerModel);
                case Layer.LayerType.PreComp:
                    return new CompositionLayer(drawable, layerModel, composition.GetPrecomps(layerModel.RefId), composition);
                case Layer.LayerType.Solid:
                    return new SolidLayer(drawable, layerModel);
                case Layer.LayerType.Image:
                    return new ImageLayer(drawable, layerModel, composition.DpScale);
                case Layer.LayerType.Null:
                    return new NullLayer(drawable, layerModel);
                case Layer.LayerType.Text:
                case Layer.LayerType.Unknown:
                default:
                    // Do nothing
                    Debug.WriteLine("Unknown layer type " + layerModel.GetLayerType(), "LOTTIE");
                    return null;
            }
        }

        private readonly Path _path = new Path();
        private DenseMatrix _matrix = new DenseMatrix(3, 3);
        private readonly Paint _contentPaint = new Paint(Paint.AntiAliasFlag);
        private readonly Paint _maskPaint = new Paint(Paint.AntiAliasFlag);
        private readonly Paint _mattePaint = new Paint(Paint.AntiAliasFlag);
        private readonly Paint _clearPaint = new Paint();
        protected Rect Rect;
        private Rect _maskBoundsRect;
        private Rect _matteBoundsRect;
        private Rect _tempMaskBoundsRect;
        internal DenseMatrix BoundsMatrix = new DenseMatrix(3, 3);
        internal readonly LottieDrawable LottieDrawable;
        internal Layer _layerModel { get; set; }
        private readonly MaskKeyframeAnimation _mask;
        private BaseLayer _matteLayer;
        private BaseLayer _parentLayer;
        private IList<BaseLayer> _parentLayers;

        private readonly IList<IBaseKeyframeAnimation> _animations = new List<IBaseKeyframeAnimation>();
        internal readonly TransformKeyframeAnimation Transform;
        private bool _visible = true;

        internal BaseLayer(LottieDrawable lottieDrawable, Layer layerModel)
        {
            LottieDrawable = lottieDrawable;
            _layerModel = layerModel;
            _clearPaint.Xfermode = new PorterDuffXfermode(PorterDuff.Mode.Clear);
            _maskPaint.Xfermode = new PorterDuffXfermode(PorterDuff.Mode.DstIn);
            if (layerModel.GetMatteType() == Layer.MatteType.Invert)
            {
                _mattePaint.Xfermode = new PorterDuffXfermode(PorterDuff.Mode.DstOut);
            }
            else
            {
                _mattePaint.Xfermode = new PorterDuffXfermode(PorterDuff.Mode.DstIn);
            }

            Transform = layerModel.Transform.CreateAnimation();
            Transform.AddListener(this);
            Transform.AddAnimationsToLayer(this);

            if (layerModel.Masks != null && layerModel.Masks.Count > 0)
            {
                _mask = new MaskKeyframeAnimation(layerModel.Masks);
                foreach (var animation in _mask.MaskAnimations)
                {
                    AddAnimation(animation);
                    animation.AddUpdateListener(this);
                }
            }
            SetupInOutAnimations();
        }

        public virtual void OnValueChanged()
        {
            InvalidateSelf();
        }

        internal virtual Layer LayerModel => _layerModel;

        internal virtual BaseLayer MatteLayer
        {
            set => _matteLayer = value;
        }

        internal virtual bool HasMatteOnThisLayer()
        {
            return _matteLayer != null;
        }

        internal virtual BaseLayer ParentLayer
        {
            set => _parentLayer = value;
        }

        private void SetupInOutAnimations()
        {
            if (_layerModel.InOutKeyframes.Count > 0)
            {
                var inOutAnimation = new FloatKeyframeAnimation(_layerModel.InOutKeyframes);
                inOutAnimation.SetIsDiscrete();
                inOutAnimation.AddUpdateListener(new AnimationListenerAnonymousInnerClass(this, inOutAnimation));
                Visible = inOutAnimation.Value == 1f;
                AddAnimation(inOutAnimation);
            }
            else
            {
                Visible = true;
            }
        }

        private class AnimationListenerAnonymousInnerClass : BaseKeyframeAnimation.IAnimationListener
        {
            private readonly BaseLayer _outerInstance;

            private readonly FloatKeyframeAnimation _inOutAnimation;

            public AnimationListenerAnonymousInnerClass(BaseLayer outerInstance, FloatKeyframeAnimation inOutAnimation)
            {
                _outerInstance = outerInstance;
                _inOutAnimation = inOutAnimation;
            }

            public virtual void OnValueChanged()
            {
                _outerInstance.Visible = _inOutAnimation.Value == 1f;
            }
        }

        private void InvalidateSelf()
        {
            LottieDrawable.InvalidateSelf();
        }

        internal void AddAnimation(IBaseKeyframeAnimation newAnimation)
        {
            if (!(newAnimation is IStaticKeyFrameAnimation))
            {
                _animations.Add(newAnimation);
            }
        }

        public virtual void GetBounds(out Rect outBounds, DenseMatrix parentMatrix)
        {
            BoundsMatrix.Set(parentMatrix);
            BoundsMatrix = MatrixExt.PreConcat(BoundsMatrix, Transform.Matrix);
        }

        public void Draw(BitmapCanvas canvas, DenseMatrix parentMatrix, byte parentAlpha)
        {
            if (!_visible)
            {
                return;
            }
            BuildParentLayerListIfNeeded();
            _matrix.Reset();
            _matrix.Set(parentMatrix);
            for (var i = _parentLayers.Count - 1; i >= 0; i--)
            {
                _matrix = MatrixExt.PreConcat(_matrix, _parentLayers[i].Transform.Matrix);
            }
            var alpha = (byte)(parentAlpha / 255f * (float)Transform.Opacity.Value / 100f * 255);
            if (!HasMatteOnThisLayer() && !HasMasksOnThisLayer())
            {
                _matrix = MatrixExt.PreConcat(_matrix, Transform.Matrix);
                DrawLayer(canvas, _matrix, alpha);
                return;
            }

            RectExt.Set(ref Rect, 0, 0, 0, 0);
            GetBounds(out Rect, _matrix);
            IntersectBoundsWithMatte(Rect, _matrix);

            _matrix = MatrixExt.PreConcat(_matrix, Transform.Matrix);
            IntersectBoundsWithMask(Rect, _matrix);

            RectExt.Set(ref Rect, 0, 0, canvas.Width, canvas.Height);

            canvas.SaveLayer(Rect, _contentPaint, BitmapCanvas.AllSaveFlag);
            // Clear the off screen buffer. This is necessary for some phones.
            ClearCanvas(canvas);
            DrawLayer(canvas, _matrix, alpha);

            if (HasMasksOnThisLayer())
            {
                ApplyMasks(canvas, _matrix);
            }

            if (HasMatteOnThisLayer())
            {
                canvas.SaveLayer(Rect, _mattePaint, SaveFlags);
                ClearCanvas(canvas);
                
                _matteLayer.Draw(canvas, parentMatrix, alpha);
                canvas.Restore();
            }

            canvas.Restore();
        }

        private void ClearCanvas(BitmapCanvas canvas)
        {
            // If we don't pad the clear draw, some phones leave a 1px border of the graphics buffer.
            canvas.DrawRect(Rect.Left - 1, Rect.Top - 1, Rect.Right + 1, Rect.Bottom + 1, _clearPaint);
        }

        private void IntersectBoundsWithMask(Rect rect, DenseMatrix matrix)
        {
            RectExt.Set(ref _maskBoundsRect, 0, 0, 0, 0);
            if (!HasMasksOnThisLayer())
            {
                return;
            }
            
            var size = _mask.Masks.Count;
            for (var i = 0; i < size; i++)
            {
                var mask = _mask.Masks[i];
                var maskAnimation = _mask.MaskAnimations[i];
                var maskPath = maskAnimation.Value;
                _path.Set(maskPath);
                _path.Transform(matrix);

                switch (mask.GetMaskMode())
                {
                    case Mask.MaskMode.MaskModeSubtract:
                        // If there is a subtract mask, the mask could potentially be the size of the entire
                        // canvas so we can't use the mask bounds.
                        return;
                    case Mask.MaskMode.MaskModeAdd:
                    default:
                        _path.ComputeBounds(out _tempMaskBoundsRect);
                        // As we iterate through the masks, we want to calculate the union region of the masks.
                        // We initialize the rect with the first mask. If we don't call set() on the first call,
                        // the rect will always extend to (0,0).
                        if (i == 0)
                        {
                            RectExt.Set(ref _maskBoundsRect, _tempMaskBoundsRect);
                        }
                        else
                        {
                            RectExt.Set(ref _maskBoundsRect, Math.Min(_maskBoundsRect.Left, _tempMaskBoundsRect.Left), Math.Min(_maskBoundsRect.Top, _tempMaskBoundsRect.Top), Math.Max(_maskBoundsRect.Right, _tempMaskBoundsRect.Right), Math.Max(_maskBoundsRect.Bottom, _tempMaskBoundsRect.Bottom));
                        }
                        break;
                }
            }

            RectExt.Set(ref rect, Math.Max(rect.Left, _maskBoundsRect.Left), Math.Max(rect.Top, _maskBoundsRect.Top), Math.Min(rect.Right, _maskBoundsRect.Right), Math.Min(rect.Bottom, _maskBoundsRect.Bottom));
        }

        private void IntersectBoundsWithMatte(Rect rect, DenseMatrix matrix)
        {
            if (!HasMatteOnThisLayer())
            {
                return;
            }
            if (_layerModel.GetMatteType() == Layer.MatteType.Invert)
            {
                // We can't trim the bounds if the mask is inverted since it extends all the way to the
                // composition bounds.
                return;
            }
            _matteLayer.GetBounds(out _matteBoundsRect, matrix);
            RectExt.Set(ref rect, Math.Max(rect.Left, _matteBoundsRect.Left), Math.Max(rect.Top, _matteBoundsRect.Top), Math.Min(rect.Right, _matteBoundsRect.Right), Math.Min(rect.Bottom, _matteBoundsRect.Bottom));
        }

        public abstract void DrawLayer(BitmapCanvas canvas, DenseMatrix parentMatrix, byte parentAlpha);

        private void ApplyMasks(BitmapCanvas canvas, DenseMatrix matrix)
        {
            canvas.SaveLayer(Rect, _maskPaint, SaveFlags);
            ClearCanvas(canvas);

            var size = _mask.Masks.Count;
            for (var i = 0; i < size; i++)
            {
                var mask = _mask.Masks[i];
                var maskAnimation = _mask.MaskAnimations[i];
                var maskPath = maskAnimation.Value;
                _path.Set(maskPath);
                _path.Transform(matrix);

                switch (mask.GetMaskMode())
                {
                    case Mask.MaskMode.MaskModeSubtract:
                        _path.FillType = PathFillType.InverseWinding;
                        break;
                    case Mask.MaskMode.MaskModeAdd:
                    default:
                        _path.FillType = PathFillType.Winding;
                        break;
                }
                canvas.DrawPath(_path, _contentPaint);
            }
            canvas.Restore();
        }

        internal virtual bool HasMasksOnThisLayer()
        {
            return _mask != null && _mask.MaskAnimations.Count > 0;
        }

        private bool Visible
        {
            set
            {
                if (value != _visible)
                {
                    _visible = value;
                    InvalidateSelf();
                }
            }
        }

        public virtual float Progress
        {
            set
            {
                if (_matteLayer != null)
                {
                    _matteLayer.Progress = value;
                }
                for (var i = 0; i < _animations.Count; i++)
                {
                    _animations[i].Progress = value;
                }
            }
        }

        private void BuildParentLayerListIfNeeded()
        {
            if (_parentLayers != null)
            {
                return;
            }
            if (_parentLayer == null)
            {
                _parentLayers = new List<BaseLayer>();
                return;
            }

            _parentLayers = new List<BaseLayer>();
            var layer = _parentLayer;
            while (layer != null)
            {
                _parentLayers.Add(layer);
                layer = layer._parentLayer;
            }
        }

        public string Name => _layerModel.Name;

        public void SetContents(IList<IContent> contentsBefore, IList<IContent> contentsAfter)
        {
            // Do nothing
        }

        public virtual void AddColorFilter(string layerName, string contentName, ColorFilter colorFilter)
        {
            // Do nothing
        }
    }
}