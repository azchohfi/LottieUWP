using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Foundation;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    internal abstract class BaseLayer : IDrawingContent
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
                    return new TextLayer(drawable, layerModel);
                case Layer.LayerType.Unknown:
                default:
                    // Do nothing
                    Debug.WriteLine("Unknown layer type " + layerModel.GetLayerType(), LottieLog.Tag);
                    return null;
            }
        }

        private class TraceSections
        {
            /** this is cached since it is in a hot code path */
            internal readonly string Draw;
            internal readonly string DrawLayer;
            internal readonly string DrawMask;
            internal readonly string DrawMatte;
            internal readonly string SaveLayer;
            internal readonly string RestoreLayer;
            internal readonly string ClearLayer;

            public TraceSections(String layerName)
            {
                Draw = $"{layerName}.Draw";
                DrawLayer = $"{layerName}.DrawLayer";
                DrawMask = $"{layerName}.DrawMask";
                DrawMatte = $"{layerName}.DrawMatte";
                SaveLayer = $"{layerName}.SaveLayer";
                RestoreLayer = $"{layerName}.RestoreLayer";
                ClearLayer = $"{layerName}.ClearLayer";
            }
        }

        private readonly Path _path = new Path();
        internal DenseMatrix Matrix = DenseMatrix.CreateIdentity(3);
        private readonly Paint _contentPaint = new Paint(Paint.AntiAliasFlag);
        private readonly Paint _maskPaint = new Paint(Paint.AntiAliasFlag);
        private readonly Paint _mattePaint = new Paint(Paint.AntiAliasFlag);
        private readonly Paint _clearPaint = new Paint();
        protected Rect Rect;
        private Rect _maskBoundsRect;
        private Rect _matteBoundsRect;
        private Rect _tempMaskBoundsRect;
        private readonly TraceSections _traceSections;
        internal DenseMatrix BoundsMatrix = DenseMatrix.CreateIdentity(3);
        internal readonly LottieDrawable LottieDrawable;
        internal Layer LayerModel;
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
            LayerModel = layerModel;
            _traceSections = new TraceSections(layerModel.Name);
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
            Transform.ValueChanged += OnValueChanged;
            Transform.AddAnimationsToLayer(this);

            if (layerModel.Masks != null && layerModel.Masks.Count > 0)
            {
                _mask = new MaskKeyframeAnimation(layerModel.Masks);
                foreach (var animation in _mask.MaskAnimations)
                {
                    AddAnimation(animation);
                    animation.ValueChanged += OnValueChanged;
                }
            }
            SetupInOutAnimations();
        }

        public virtual void OnValueChanged(object sender, EventArgs eventArgs)
        {
            InvalidateSelf();
        }

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
            if (LayerModel.InOutKeyframes.Count > 0)
            {
                var inOutAnimation = new FloatKeyframeAnimation(LayerModel.InOutKeyframes);
                inOutAnimation.SetIsDiscrete();
                inOutAnimation.ValueChanged += (sender, args) =>
                {
                    Visible = inOutAnimation.Value == 1f;
                };
                Visible = inOutAnimation.Value == 1f;
                AddAnimation(inOutAnimation);
            }
            else
            {
                Visible = true;
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
            LottieLog.BeginSection(_traceSections.Draw);
            if (!_visible)
            {
                LottieLog.EndSection(_traceSections.Draw);
                return;
            }
            BuildParentLayerListIfNeeded();
            Matrix.Reset();
            Matrix.Set(parentMatrix);
            for (var i = _parentLayers.Count - 1; i >= 0; i--)
            {
                Matrix = MatrixExt.PreConcat(Matrix, _parentLayers[i].Transform.Matrix);
            }
            var alpha = (byte)(parentAlpha / 255f * (float)Transform.Opacity.Value / 100f * 255);
            if (!HasMatteOnThisLayer() && !HasMasksOnThisLayer())
            {
                Matrix = MatrixExt.PreConcat(Matrix, Transform.Matrix);
                LottieLog.BeginSection(_traceSections.DrawLayer);
                DrawLayer(canvas, Matrix, alpha);
                LottieLog.EndSection(_traceSections.DrawLayer);
                LottieLog.EndSection(_traceSections.Draw);
                return;
            }

            RectExt.Set(ref Rect, 0, 0, 0, 0);
            GetBounds(out Rect, Matrix);
            IntersectBoundsWithMatte(Rect, Matrix);

            Matrix = MatrixExt.PreConcat(Matrix, Transform.Matrix);
            IntersectBoundsWithMask(Rect, Matrix);

            RectExt.Set(ref Rect, 0, 0, canvas.Width, canvas.Height);

            LottieLog.BeginSection(_traceSections.SaveLayer);
            canvas.SaveLayer(Rect, _contentPaint, BitmapCanvas.AllSaveFlag);
            LottieLog.EndSection(_traceSections.SaveLayer);

            // Clear the off screen buffer. This is necessary for some phones.
            ClearCanvas(canvas);
            LottieLog.BeginSection(_traceSections.DrawLayer);
            DrawLayer(canvas, Matrix, alpha);
            LottieLog.EndSection(_traceSections.DrawLayer);

            if (HasMasksOnThisLayer())
            {
                ApplyMasks(canvas, Matrix);
            }

            if (HasMatteOnThisLayer())
            {
                LottieLog.BeginSection(_traceSections.DrawMatte);
                LottieLog.BeginSection(_traceSections.SaveLayer);
                canvas.SaveLayer(Rect, _mattePaint, SaveFlags);
                LottieLog.EndSection(_traceSections.SaveLayer);
                ClearCanvas(canvas);

                _matteLayer.Draw(canvas, parentMatrix, alpha);
                LottieLog.BeginSection(_traceSections.RestoreLayer);
                canvas.Restore();
                LottieLog.EndSection(_traceSections.RestoreLayer);
                LottieLog.EndSection(_traceSections.DrawMatte);
            }

            LottieLog.BeginSection(_traceSections.RestoreLayer);
            canvas.Restore();
            LottieLog.EndSection(_traceSections.RestoreLayer);
            LottieLog.EndSection(_traceSections.Draw);
        }

        private void ClearCanvas(BitmapCanvas canvas)
        {
            LottieLog.BeginSection(_traceSections.ClearLayer);
            // If we don't pad the clear draw, some phones leave a 1px border of the graphics buffer.
            canvas.DrawRect(Rect.Left - 1, Rect.Top - 1, Rect.Right + 1, Rect.Bottom + 1, _clearPaint);
            LottieLog.EndSection(_traceSections.ClearLayer);
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
            if (LayerModel.GetMatteType() == Layer.MatteType.Invert)
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
            LottieLog.BeginSection(_traceSections.DrawMask);
            LottieLog.BeginSection(_traceSections.SaveLayer);
            canvas.SaveLayer(Rect, _maskPaint, SaveFlags);
            LottieLog.EndSection(_traceSections.SaveLayer);
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
            LottieLog.BeginSection(_traceSections.RestoreLayer);
            canvas.Restore();
            LottieLog.EndSection(_traceSections.RestoreLayer);
            LottieLog.EndSection(_traceSections.DrawMask);
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

        public string Name => LayerModel.Name;

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