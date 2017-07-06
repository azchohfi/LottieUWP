using System;
using System.Collections.Generic;
using Windows.Foundation;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    internal class CompositionLayer : BaseLayer
    {
        private readonly IBaseKeyframeAnimation<float?> _timeRemapping;
        private readonly IList<BaseLayer> _layers = new List<BaseLayer>();
        private Rect _originalClipRect;
        private Rect _newClipRect;

        private bool? _hasMatte;
        private bool? _hasMasks;

        internal CompositionLayer(LottieDrawable lottieDrawable, Layer layerModel, IList<Layer> layerModels, LottieComposition composition) : base(lottieDrawable, layerModel)
        {
            var timeRemapping = layerModel.TimeRemapping;
            if (timeRemapping != null)
            {
                _timeRemapping = timeRemapping.CreateAnimation();
                AddAnimation(_timeRemapping);
                _timeRemapping.ValueChanged += OnValueChanged;
            }
            else
            {
                _timeRemapping = null;
            }

            var layerMap = new Dictionary<long, BaseLayer>(composition.Layers.Count);

            BaseLayer mattedLayer = null;
            for (var i = layerModels.Count - 1; i >= 0; i--)
            {
                var lm = layerModels[i];
                var layer = ForModel(lm, lottieDrawable, composition);
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
                var layerView = layer.Value;
                if (layerMap.TryGetValue(layerView.LayerModel.ParentId, out BaseLayer parentLayer))
                {
                    layerView.ParentLayer = parentLayer;
                }
            }
        }

        public override void DrawLayer(BitmapCanvas canvas, DenseMatrix parentMatrix, byte parentAlpha)
        {
            LottieLog.BeginSection("CompositionLayer.Draw");
            canvas.GetClipBounds(out _originalClipRect);
            RectExt.Set(ref _newClipRect, 0, 0, LayerModel.PreCompWidth, LayerModel.PreCompHeight);
            parentMatrix.MapRect(ref _newClipRect);

            for (var i = _layers.Count - 1; i >= 0; i--)
            {
                var nonEmptyClip = true;
                if (!_newClipRect.IsEmpty)
                {
                    nonEmptyClip = canvas.ClipRect(_newClipRect);
                }
                if (nonEmptyClip)
                {
                    BaseLayer layer = _layers[i];
                    layer.Draw(canvas, parentMatrix, parentAlpha);
                }
            }
            if (!_originalClipRect.IsEmpty)
            {
                canvas.ClipRect(_originalClipRect, Region.Op.Replace);
            }
            LottieLog.EndSection("CompositionLayer.Draw");
        }

        public override void GetBounds(out Rect outBounds, DenseMatrix parentMatrix)
        {
            base.GetBounds(out outBounds, parentMatrix);
            RectExt.Set(ref Rect, 0, 0, 0, 0);
            for (var i = _layers.Count - 1; i >= 0; i--)
            {
                var content = _layers[i];
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

                if (_timeRemapping?.Value != null)
                {
                    var duration = LottieDrawable.Composition.Duration;
                    var remappedTime = (long)(_timeRemapping.Value.Value * 1000);
                    value = remappedTime / (float)duration;
                }
                if (LayerModel.TimeStretch != 0)
                {
                    value /= LayerModel.TimeStretch;
                }

                value -= LayerModel.StartProgress;
                for (var i = _layers.Count - 1; i >= 0; i--)
                {
                    _layers[i].Progress = value;
                }
            }
        }

        internal virtual bool HasMasks()
        {
            if (_hasMasks == null)
            {
                for (var i = _layers.Count - 1; i >= 0; i--)
                {
                    var layer = _layers[i];
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

                for (var i = _layers.Count - 1; i >= 0; i--)
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
            for (var i = 0; i < _layers.Count; ++i)
            {
                var layer = _layers[i];
                var name = layer.LayerModel.Name;
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