using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    public abstract class BaseStrokeContent : IDrawingContent
    {
        private class TraceSections
        {
            internal readonly string Draw;
            internal readonly string DrawTrimPath;
            internal readonly string DrawPath;

            public TraceSections(String layerName)
            {
                Draw = $"{layerName}.Draw";
                DrawTrimPath = $"{layerName}.DrawTrimmedPath";
                DrawPath = $"{layerName}.DrawPath";
            }
        }

        private readonly PathMeasure _pm = new PathMeasure();
        private readonly Path _path = new Path();
        private readonly Path _trimPathPath = new Path();
        private Rect _rect;
        private readonly LottieDrawable _lottieDrawable;
        private readonly IList<PathGroup> _pathGroups = new List<PathGroup>();
        private readonly double[] _dashPatternValues;
        private readonly TraceSections _traceSections;
        internal readonly Paint Paint = new Paint(Paint.AntiAliasFlag);

        private readonly IBaseKeyframeAnimation<float?> _widthAnimation;
        private readonly IBaseKeyframeAnimation<int?> _opacityAnimation;
        private readonly IList<IBaseKeyframeAnimation<float?>> _dashPatternAnimations;
        private readonly IBaseKeyframeAnimation<float?> _dashPatternOffsetAnimation;

        internal BaseStrokeContent(LottieDrawable lottieDrawable, BaseLayer layer, PenLineCap cap, PenLineJoin join, AnimatableIntegerValue opacity, AnimatableFloatValue width, IList<AnimatableFloatValue> dashPattern, AnimatableFloatValue offset)
        {
            _lottieDrawable = lottieDrawable;
            _traceSections = new TraceSections(layer.Name);

            Paint.Style = Paint.PaintStyle.Stroke;
            Paint.StrokeCap = cap;
            Paint.StrokeJoin = join;

            _opacityAnimation = opacity.CreateAnimation();
            _widthAnimation = width.CreateAnimation();

            if (offset == null)
            {
                _dashPatternOffsetAnimation = null;
            }
            else
            {
                _dashPatternOffsetAnimation = offset.CreateAnimation();
            }
            _dashPatternAnimations = new List<IBaseKeyframeAnimation<float?>>(dashPattern.Count);
            _dashPatternValues = new double[dashPattern.Count];

            for (var i = 0; i < dashPattern.Count; i++)
            {
                _dashPatternAnimations.Add(dashPattern[i].CreateAnimation());
            }

            layer.AddAnimation(_opacityAnimation);
            layer.AddAnimation(_widthAnimation);
            for (var i = 0; i < _dashPatternAnimations.Count; i++)
            {
                layer.AddAnimation(_dashPatternAnimations[i]);
            }
            if (_dashPatternOffsetAnimation != null)
            {
                layer.AddAnimation(_dashPatternOffsetAnimation);
            }

            _opacityAnimation.ValueChanged += OnValueChanged;
            _widthAnimation.ValueChanged += OnValueChanged;

            for (var i = 0; i < dashPattern.Count; i++)
            {
                _dashPatternAnimations[i].ValueChanged += OnValueChanged;
            }
            if (_dashPatternOffsetAnimation != null)
            {
                _dashPatternOffsetAnimation.ValueChanged += OnValueChanged;
            }
        }

        public virtual void OnValueChanged(object sender, EventArgs eventArgs)
        {
            _lottieDrawable.InvalidateSelf();
        }

        public abstract string Name { get; }

        public void SetContents(IList<IContent> contentsBefore, IList<IContent> contentsAfter)
        {
            TrimPathContent trimPathContentBefore = null;
            for (var i = contentsBefore.Count - 1; i >= 0; i--)
            {
                var content = contentsBefore[i];
                if (content is TrimPathContent trimPathContent && trimPathContent.Type == ShapeTrimPath.Type.Individually)
                {
                    trimPathContentBefore = trimPathContent;
                }
            }
            if (trimPathContentBefore != null)
            {
                trimPathContentBefore.ValueChanged += OnValueChanged;
            }

            PathGroup currentPathGroup = null;
            for (var i = contentsAfter.Count - 1; i >= 0; i--)
            {
                var content = contentsAfter[i];
                if (content is TrimPathContent trimPathContent && trimPathContent.Type == ShapeTrimPath.Type.Individually)
                {
                    if (currentPathGroup != null)
                    {
                        _pathGroups.Add(currentPathGroup);
                    }
                    currentPathGroup = new PathGroup(trimPathContent);
                    trimPathContent.ValueChanged += OnValueChanged;
                }
                else if (content is IPathContent)
                {
                    if (currentPathGroup == null)
                    {
                        currentPathGroup = new PathGroup(trimPathContentBefore);
                    }
                    currentPathGroup.Paths.Add((IPathContent)content);
                }
            }
            if (currentPathGroup != null)
            {
                _pathGroups.Add(currentPathGroup);
            }
        }

        public virtual void Draw(BitmapCanvas canvas, DenseMatrix parentMatrix, byte parentAlpha)
        {
            LottieLog.BeginSection(_traceSections.Draw);
            var alpha = (byte)(parentAlpha / 255f * _opacityAnimation.Value / 100f * 255);
            Paint.Alpha = alpha;
            Paint.StrokeWidth = _widthAnimation.Value.Value * Utils.GetScale(parentMatrix);
            if (Paint.StrokeWidth <= 0)
            {
                // Android draws a hairline stroke for 0, After Effects doesn't.
                LottieLog.EndSection(_traceSections.Draw);
                return;
            }
            ApplyDashPatternIfNeeded(parentMatrix);

            for (var i = 0; i < _pathGroups.Count; i++)
            {
                var pathGroup = _pathGroups[i];

                if (pathGroup.TrimPath != null)
                {
                    ApplyTrimPath(canvas, pathGroup, parentMatrix);
                }
                else
                {
                    LottieLog.BeginSection(_traceSections.DrawPath);
                    _path.Reset();
                    for (var j = pathGroup.Paths.Count - 1; j >= 0; j--)
                    {
                        _path.AddPath(pathGroup.Paths[j].Path, parentMatrix);
                    }
                    canvas.DrawPath(_path, Paint);
                    LottieLog.EndSection(_traceSections.DrawPath);
                }
            }
            LottieLog.EndSection(_traceSections.Draw);
        }

        private void ApplyTrimPath(BitmapCanvas canvas, PathGroup pathGroup, DenseMatrix parentMatrix)
        {
            LottieLog.BeginSection(_traceSections.DrawTrimPath);
            if (pathGroup.TrimPath == null)
            {
                LottieLog.EndSection(_traceSections.DrawTrimPath);
                return;
            }
            _path.Reset();
            for (var j = pathGroup.Paths.Count - 1; j >= 0; j--)
            {
                _path.AddPath(pathGroup.Paths[j].Path, parentMatrix);
            }
            _pm.SetPath(_path);
            var totalLength = _pm.Length;
            while (_pm.NextContour())
            {
                totalLength += _pm.Length;
            }
            var offsetLength = totalLength * pathGroup.TrimPath.Offset.Value.Value / 360f;
            var startLength = totalLength * pathGroup.TrimPath.Start.Value.Value / 100f + offsetLength;
            var endLength = totalLength * pathGroup.TrimPath.End.Value.Value / 100f + offsetLength;

            float currentLength = 0;
            for (var j = pathGroup.Paths.Count - 1; j >= 0; j--)
            {
                _trimPathPath.Set(pathGroup.Paths[j].Path);
                _trimPathPath.Transform(parentMatrix);
                _pm.SetPath(_trimPathPath);
                var length = _pm.Length;
                if (endLength > totalLength && endLength - totalLength < currentLength + length && currentLength < endLength - totalLength)
                {
                    // Draw the segment when the end is greater than the length which wraps around to the
                    // beginning.
                    float startValue;
                    if (startLength > totalLength)
                    {
                        startValue = (startLength - totalLength) / length;
                    }
                    else
                    {
                        startValue = 0;
                    }
                    var endValue = Math.Min((endLength - totalLength) / length, 1);
                    Utils.ApplyTrimPathIfNeeded(_trimPathPath, startValue, endValue, 0);
                    canvas.DrawPath(_trimPathPath, Paint);
                }
                else
                {
                    if (currentLength + length < startLength || currentLength > endLength)
                    {
                        // Do nothing
                    }
                    else if (currentLength + length <= endLength && startLength < currentLength)
                    {
                        canvas.DrawPath(_trimPathPath, Paint);
                    }
                    else
                    {
                        float startValue;
                        if (startLength < currentLength)
                        {
                            startValue = 0;
                        }
                        else
                        {
                            startValue = (startLength - currentLength) / length;
                        }
                        float endValue;
                        if (endLength > currentLength + length)
                        {
                            endValue = 1f;
                        }
                        else
                        {
                            endValue = (endLength - currentLength) / length;
                        }
                        Utils.ApplyTrimPathIfNeeded(_trimPathPath, startValue, endValue, 0);
                        canvas.DrawPath(_trimPathPath, Paint);
                    }
                }
                currentLength += length;
            }
            LottieLog.EndSection(_traceSections.DrawTrimPath);
        }

        public void GetBounds(out Rect outBounds, DenseMatrix parentMatrix)
        {
            _path.Reset();
            for (var i = 0; i < _pathGroups.Count; i++)
            {
                var pathGroup = _pathGroups[i];
                for (var j = 0; j < pathGroup.Paths.Count; j++)
                {
                    _path.AddPath(pathGroup.Paths[j].Path, parentMatrix);
                }
            }
            _path.ComputeBounds(out _rect);

            var width = _widthAnimation.Value.Value;
            RectExt.Set(ref _rect, _rect.Left - width / 2f, _rect.Top - width / 2f, _rect.Right + width / 2f, _rect.Bottom + width / 2f);
            RectExt.Set(ref outBounds, _rect);
            // Add padding to account for rounding errors.
            RectExt.Set(ref outBounds, outBounds.Left - 1, outBounds.Top - 1, outBounds.Right + 1, outBounds.Bottom + 1);
        }

        public abstract void AddColorFilter(string layerName, string contentName, ColorFilter colorFilter);

        private void ApplyDashPatternIfNeeded(DenseMatrix parentMatrix)
        {
            if (_dashPatternAnimations.Count == 0)
            {
                return;
            }

            var scale = Utils.GetScale(parentMatrix);
            for (var i = 0; i < _dashPatternAnimations.Count; i++)
            {
                _dashPatternValues[i] = _dashPatternAnimations[i].Value.Value;
                // If the value of the dash pattern or gap is too small, the number of individual sections
                // approaches infinity as the value approaches 0.
                // To mitigate this, we essentially put a minimum value on the dash pattern size of 1px
                // and a minimum gap size of 0.01.
                if (i % 2 == 0)
                {
                    if (_dashPatternValues[i] < 1f)
                    {
                        _dashPatternValues[i] = 1f;
                    }
                }
                else
                {
                    if (_dashPatternValues[i] < 0.1f)
                    {
                        _dashPatternValues[i] = 0.1f;
                    }
                }
                _dashPatternValues[i] *= scale;
            }
            var offset = _dashPatternOffsetAnimation?.Value ?? 0f;
            Paint.PathEffect = new DashPathEffect(_dashPatternValues, offset);
        }

        /// <summary>
        /// Data class to help drawing trim paths individually.
        /// </summary>
        private sealed class PathGroup
        {
            internal readonly IList<IPathContent> Paths = new List<IPathContent>();
            internal readonly TrimPathContent TrimPath;

            internal PathGroup(TrimPathContent trimPath)
            {
                TrimPath = trimPath;
            }
        }
    }
}