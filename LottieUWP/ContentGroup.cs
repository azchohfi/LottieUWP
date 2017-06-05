using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    internal class ContentGroup : IDrawingContent, IPathContent, BaseKeyframeAnimation.IAnimationListener
    {
        private static readonly string Tag = typeof(ContentGroup).Name;
        private DenseMatrix _matrix = new DenseMatrix(3, 3);
        private readonly Path _path = new Path();
        private Rect _rect;

        private readonly List<IContent> _contents = new List<IContent>();
        private readonly LottieDrawable _lottieDrawable;
        private IList<IPathContent> _pathContents;
        private readonly TransformKeyframeAnimation _transformAnimation;

        internal ContentGroup(LottieDrawable lottieDrawable, BaseLayer layer, ShapeGroup shapeGroup)
        {
            Name = shapeGroup.Name;
            _lottieDrawable = lottieDrawable;
            IList<object> items = shapeGroup.Items;
            if (items.Count == 0)
            {
                return;
            }

            var animatableTransform = items[items.Count - 1] as AnimatableTransform;
            if (animatableTransform != null)
            {
                _transformAnimation = animatableTransform.CreateAnimation();
                //noinspection ConstantConditions
                _transformAnimation.AddAnimationsToLayer(layer);
                _transformAnimation.AddListener(this);
            }

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item is ShapeFill)
                {
                    _contents.Add(new FillContent(lottieDrawable, layer, (ShapeFill)item));
                }
                else if (item is GradientFill)
                {
                    _contents.Add(new GradientFillContent(lottieDrawable, layer, (GradientFill)item));
                }
                else if (item is ShapeStroke)
                {
                    _contents.Add(new StrokeContent(lottieDrawable, layer, (ShapeStroke)item));
                }
                else if (item is GradientStroke)
                {
                    _contents.Add(new GradientStrokeContent(lottieDrawable, layer, (GradientStroke)item));
                }
                else if (item is ShapeGroup)
                {
                    _contents.Add(new ContentGroup(lottieDrawable, layer, (ShapeGroup)item));
                }
                else if (item is RectangleShape)
                {
                    _contents.Add(new RectangleContent(lottieDrawable, layer, (RectangleShape)item));
                }
                else if (item is CircleShape)
                {
                    _contents.Add(new EllipseContent(lottieDrawable, layer, (CircleShape)item));
                }
                else if (item is ShapePath)
                {
                    _contents.Add(new ShapeContent(lottieDrawable, layer, (ShapePath)item));
                }
                else if (item is PolystarShape)
                {
                    _contents.Add(new PolystarContent(lottieDrawable, layer, (PolystarShape)item));
                }
                else if (item is ShapeTrimPath)
                {
                    _contents.Add(new TrimPathContent(layer, (ShapeTrimPath)item));
                }
                else //noinspection StatementWithEmptyBody
                {
                    if (item is MergePaths)
                    {
                        if (lottieDrawable.EnableMergePathsForKitKatAndAbove())
                        {
                            _contents.Add(new MergePathsContent((MergePaths)item));
                        }
                        else
                        {
                            Debug.WriteLine("Animation contains merge paths but they are disabled.", Tag);
                        }
                    }
                }
            }

            IList<IContent> contentsToRemove = new List<IContent>();
            MergePathsContent currentMergePathsContent = null;
            for (int i = _contents.Count - 1; i >= 0; i--)
            {
                IContent content = _contents[i];
                if(content is MergePathsContent)
                    currentMergePathsContent = (MergePathsContent) content;
                if (currentMergePathsContent != null && content != currentMergePathsContent)
                {
                    currentMergePathsContent.AddContentIfNeeded(content);
                    contentsToRemove.Add(content);
                }
            }

            for (int i = _contents.Count - 1; i >= 0; i--)
            {
                if (contentsToRemove.Contains(_contents[i]))
                {
                    _contents.RemoveAt(i);
                }
            }
        }

        public void OnValueChanged()
        {
            _lottieDrawable.InvalidateSelf();
        }

        public virtual string Name { get; }

        public virtual void AddColorFilter(string layerName, string contentName, ColorFilter colorFilter)
        {
            for (int i = 0; i < _contents.Count; i++)
            {
                var drawingContent = _contents[i] as IDrawingContent;
                if (drawingContent != null)
                {
                    if (contentName == null || contentName.Equals(drawingContent.Name))
                    {
                        drawingContent.AddColorFilter(layerName, null, colorFilter);
                    }
                    else
                    {
                        drawingContent.AddColorFilter(layerName, contentName, colorFilter);
                    }
                }
            }
        }

        public virtual void SetContents(IList<IContent> contentsBefore, IList<IContent> contentsAfter)
        {
            // Do nothing with contents after.
            var myContentsBefore = new List<IContent>(contentsBefore.Count + _contents.Count);
            myContentsBefore.AddRange(contentsBefore);

            for (int i = _contents.Count - 1; i >= 0; i--)
            {
                IContent content = _contents[i];
                content.SetContents(myContentsBefore, _contents.Take(i + 1).ToList());
                myContentsBefore.Add(content);
            }
        }

        internal virtual IList<IPathContent> PathList
        {
            get
            {
                if (_pathContents == null)
                {
                    _pathContents = new List<IPathContent>();
                    for (int i = 0; i < _contents.Count; i++)
                    {
                        var content = _contents[i] as IPathContent;
                        if (content != null)
                        {
                            _pathContents.Add(content);
                        }
                    }
                }
                return _pathContents;
            }
        }

        internal virtual DenseMatrix TransformationMatrix
        {
            get
            {
                if (_transformAnimation != null)
                {
                    return _transformAnimation.Matrix;
                }
                _matrix.Reset();
                return _matrix;
            }
        }

        public Path Path
        {
            get
            {
                // TODO: cache this somehow.
                _matrix.Reset();
                if (_transformAnimation != null)
                {
                    _matrix.Set(_transformAnimation.Matrix);
                }
                _path.Reset();
                for (int i = _contents.Count - 1; i >= 0; i--)
                {
                    var pathContent = _contents[i] as IPathContent;
                    if (pathContent != null)
                    {
                        _path.AddPath(pathContent.Path, _matrix);
                    }
                }
                return _path;
            }
        }

        public virtual void Draw(BitmapCanvas canvas, DenseMatrix parentMatrix, int parentAlpha)
        {
            _matrix.Set(parentMatrix);
            int alpha;
            if (_transformAnimation != null)
            {
                _matrix = MatrixExt.PreConcat(_matrix, _transformAnimation.Matrix);
                alpha = (int)(_transformAnimation.Opacity.Value / 100f * parentAlpha / 255f * 255);
            }
            else
            {
                alpha = parentAlpha;
            }

            for (int i = _contents.Count - 1; i >= 0; i--)
            {
                var drawingContent = _contents[i] as IDrawingContent;
                drawingContent?.Draw(canvas, _matrix, alpha);
            }
        }

        public virtual void GetBounds(out Rect outBounds, DenseMatrix parentMatrix)
        {
            _matrix.Set(parentMatrix);
            if (_transformAnimation != null)
            {
                _matrix = MatrixExt.PreConcat(_matrix, _transformAnimation.Matrix);
            }
            RectExt.Set(ref _rect, 0, 0, 0, 0);
            for (int i = _contents.Count - 1; i >= 0; i--)
            {
                var drawingContent = _contents[i] as IDrawingContent;
                if (drawingContent != null)
                {
                    drawingContent.GetBounds(out _rect, _matrix);
                    if (outBounds.IsEmpty)
                    {
                        RectExt.Set(ref outBounds, _rect);
                    }
                    else
                    {
                        RectExt.Set(ref outBounds, 
                            Math.Min(outBounds.Left, _rect.Left), 
                            Math.Min(outBounds.Top, _rect.Top), 
                            Math.Max(outBounds.Right, _rect.Right), 
                            Math.Max(outBounds.Bottom, _rect.Bottom));
                    }
                }
            }
        }
    }
}