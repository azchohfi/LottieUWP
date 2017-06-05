using System;
using System.Collections.Generic;
using Windows.Foundation;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    internal class CompositionLayer : BaseLayer
    {
        private readonly IList<BaseLayer> _layers = new List<BaseLayer>();
        private Rect _originalClipRect;
        private Rect _newClipRect;

        private bool? _hasMatte;
        private bool? _hasMasks;

        internal CompositionLayer(LottieDrawable lottieDrawable, Layer layerModel, IList<Layer> layerModels, LottieComposition composition) : base(lottieDrawable, layerModel)
        {
            Dictionary<long, BaseLayer> layerMap = new Dictionary<long, BaseLayer>(composition.Layers.Count);

            BaseLayer mattedLayer = null;
            for (int i = layerModels.Count - 1; i >= 0; i--)
            {
                Layer lm = layerModels[i];
                BaseLayer layer = ForModel(lm, lottieDrawable, composition);
                if (layer == null)
                {
                    continue;
                }
                layerMap.Add(layer.LayerModel.Id, layer);
                if (mattedLayer != null)
                {
                    mattedLayer.MatteLayer = layer;
                    mattedLayer = null;
                }
                else
                {
                    _layers.Insert(0, layer);
                    switch (lm.GetMatteType())
                    {
                        case Layer.MatteType.Add:
                        case Layer.MatteType.Invert:
                            mattedLayer = layer;
                            break;
                    }
                }
            }

            foreach (var layer in layerMap)
            {
                BaseLayer layerView = layer.Value;
                if (layerMap.TryGetValue(layerView.LayerModel.ParentId, out BaseLayer parentLayer))
                {
                    layerView.ParentLayer = parentLayer;
                }
            }
        }

        public override void DrawLayer(BitmapCanvas canvas, DenseMatrix parentMatrix, int parentAlpha)
        {
            canvas.GetClipBounds(out _originalClipRect);
            RectExt.Set(ref _newClipRect, 0, 0, _layerModel.PreCompWidth, _layerModel.PreCompHeight);
            parentMatrix.MapRect(ref _newClipRect);

            for (int i = _layers.Count - 1; i >= 0; i--)
            {
                bool nonEmptyClip = true;
                if (!_newClipRect.IsEmpty)
                {
                    nonEmptyClip = canvas.ClipRect(_newClipRect);
                }
                if (nonEmptyClip)
                {
                    _layers[i].Draw(canvas, parentMatrix, parentAlpha);
                }
            }
            if (!_originalClipRect.IsEmpty)
            {
                canvas.ClipRect(_originalClipRect, Region.Op.Replace);
            }
        }

        public override void GetBounds(out Rect outBounds, DenseMatrix parentMatrix)
        {
            base.GetBounds(out outBounds, parentMatrix);
            RectExt.Set(ref Rect, 0, 0, 0, 0);
            for (int i = _layers.Count - 1; i >= 0; i--)
            {
                BaseLayer content = _layers[i];
                content.GetBounds(out Rect, BoundsMatrix);
                if (outBounds.IsEmpty)
                {
                    RectExt.Set(ref outBounds, Rect);
                }
                else
                {
                    RectExt.Set(ref outBounds, Math.Min(outBounds.Left, Rect.Left), Math.Min(outBounds.Top, Rect.Top), Math.Max(outBounds.Right, Rect.Right), Math.Max(outBounds.Bottom, Rect.Bottom));
                }
            }
        }

        public override float Progress
        {
            set
            {
                base.Progress = value;
                value -= _layerModel.StartProgress;
                for (int i = _layers.Count - 1; i >= 0; i--)
                {
                    _layers[i].Progress = value;
                }
            }
        }

        internal virtual bool HasMasks()
        {
            if (_hasMasks == null)
            {
                for (int i = _layers.Count - 1; i >= 0; i--)
                {
                    BaseLayer layer = _layers[i];
                    if (layer is ShapeLayer)
                    {
                        if (layer.HasMasksOnThisLayer())
                        {
                            _hasMasks = true;
                            return true;
                        }
                    }
                }
                _hasMasks = false;
            }
            return _hasMasks.Value;
        }

        internal virtual bool HasMatte()
        {
            if (_hasMatte == null)
            {
                if (HasMatteOnThisLayer())
                {
                    _hasMatte = true;
                    return true;
                }

                for (int i = _layers.Count - 1; i >= 0; i--)
                {
                    if (_layers[i].HasMatteOnThisLayer())
                    {
                        _hasMatte = true;
                        return true;
                    }
                }
                _hasMatte = false;
            }
            return _hasMatte.Value;
        }

        public override void AddColorFilter(string layerName, string contentName, ColorFilter colorFilter)
        {
            for (int i = 0; i < _layers.Count; ++i)
            {
                BaseLayer layer = _layers[i];
                string name = layer.LayerModel.Name;
                if (string.IsNullOrEmpty(layerName))
                {
                    layer.AddColorFilter(null, null, colorFilter);
                }
                else if (name.Equals(layerName))
                {
                    layer.AddColorFilter(layerName, contentName, colorFilter);
                }
            }
        }
    }
}