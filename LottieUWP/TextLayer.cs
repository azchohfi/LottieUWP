using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    internal class TextLayer : BaseLayer
    {
        private Rect _rectF;
        private readonly DenseMatrix _matrix2 = DenseMatrix.CreateIdentity(3);
        private readonly Paint _fillPaint = new Paint(Paint.AntiAliasFlag)
        {
            Style = Paint.PaintStyle.Fill
        };
        private readonly Paint _strokePaint = new Paint(Paint.AntiAliasFlag)
        {
            Style = Paint.PaintStyle.Stroke
        };

        private readonly IDictionary<FontCharacter, IList<ContentGroup>> _contentsForCharacter = new Dictionary<FontCharacter, IList<ContentGroup>>();
        private readonly IBaseKeyframeAnimation<DocumentData> _textAnimation;
        private readonly LottieDrawable _lottieDrawable;
        private readonly LottieComposition _composition;
        private readonly IBaseKeyframeAnimation<Color> _colorAnimation;
        private readonly IBaseKeyframeAnimation<Color> _strokeAnimation;
        private readonly IBaseKeyframeAnimation<float?> _strokeWidthAnimation;
        private readonly IBaseKeyframeAnimation<float?> _trackingAnimation;
        
        internal TextLayer(LottieDrawable lottieDrawable, Layer layerModel) : base(lottieDrawable, layerModel)
        {
            _lottieDrawable = lottieDrawable;
            _composition = layerModel.Composition;
            _textAnimation = layerModel.Text.CreateAnimation();
            _textAnimation.ValueChanged += OnValueChanged;
            AddAnimation(_textAnimation);

            var textProperties = layerModel.TextProperties;
            if (textProperties?._color != null)
            {
                _colorAnimation = textProperties._color.CreateAnimation();
                _colorAnimation.ValueChanged += OnValueChanged;
                AddAnimation(_colorAnimation);
            }

            if (textProperties?._stroke != null)
            {
                _strokeAnimation = textProperties._stroke.CreateAnimation();
                _strokeAnimation.ValueChanged += OnValueChanged;
                AddAnimation(_strokeAnimation);
            }
            
            if (textProperties?._strokeWidth != null)
            {
                _strokeWidthAnimation = textProperties._strokeWidth.CreateAnimation();
                _strokeWidthAnimation.ValueChanged += OnValueChanged;
                AddAnimation(_strokeWidthAnimation);
            }

            if (textProperties?._tracking != null)
            {
                _trackingAnimation = textProperties._tracking.CreateAnimation();
                _trackingAnimation.ValueChanged += OnValueChanged;
                AddAnimation(_trackingAnimation);
            }
        }

        public override void DrawLayer(BitmapCanvas canvas, DenseMatrix parentMatrix, byte parentAlpha)
        {
            canvas.Save();
            var documentData = _textAnimation.Value;
            if (!_composition.Fonts.TryGetValue(documentData.FontName, out var font))
            {
                // Something is wrong. 
                return;
            }
            var fontScale = documentData.Size / 100f;
            var parentScale = Utils.GetScale(parentMatrix);
            var text = documentData.Text;

            _fillPaint.Color = _colorAnimation?.Value ?? documentData.Color;
            _strokePaint.Color = _strokeAnimation?.Value ?? documentData.StrokeColor;
            if (_strokeWidthAnimation?.Value != null)
            {
                _strokePaint.StrokeWidth = _strokeWidthAnimation.Value.Value;
            }
            else
            {
                _strokePaint.StrokeWidth = documentData.StrokeWidth * _composition.DpScale * parentScale;
            }

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if(!_composition.Characters.TryGetValue(FontCharacter.HashFor(c, font.Family, font.Style), out var character))
                {
                    // Something is wrong. Potentially, they didn't export the text as a glyph. 
                    continue;
                }
                var contentGroups = GetContentsForCharacter(character);
                for (var j = 0; j < contentGroups.Count; j++)
                {
                    var path = contentGroups[j].Path;
                    path.ComputeBounds(out _rectF);
                    _matrix2.Set(parentMatrix);
                    MatrixExt.PreScale(_matrix2, fontScale, fontScale);
                    path.Transform(_matrix2);
                    if (documentData.StrokeOverFill)
                    {
                        DrawCharacter(canvas, path, _fillPaint);
                        DrawCharacter(canvas, path, _strokePaint);
                    }
                    else
                    {
                        DrawCharacter(canvas, path, _strokePaint);
                        DrawCharacter(canvas, path, _fillPaint);
                    }
                }
                var tx = (float)character.Width * fontScale * _composition.DpScale * parentScale;
                // Add tracking 
                var tracking = documentData.Tracking / 10f;
                if (_trackingAnimation?.Value != null)
                {
                    tracking += _trackingAnimation.Value.Value;
                }
                tx += tracking * parentScale;
                canvas.Translate(tx, 0);
            }
            canvas.Restore();
        }

        private void DrawCharacter(BitmapCanvas canvas, Path path, Paint paint)
        {
            if (paint.Color == Colors.Transparent)
            {
                return;
            }
            if (paint.Style == Paint.PaintStyle.Stroke && paint.StrokeWidth == 0)
            {
                return;
            }
            canvas.DrawPath(path, paint);
        }

        private IList<ContentGroup> GetContentsForCharacter(FontCharacter character)
        {
            if (_contentsForCharacter.ContainsKey(character))
            {
                return _contentsForCharacter[character];
            }
            var shapes = character.Shapes;
            var size = shapes.Count;
            IList<ContentGroup> contents = new List<ContentGroup>(size);
            for (var i = 0; i < size; i++)
            {
                var sg = shapes[i];
                contents.Add(new ContentGroup(_lottieDrawable, this, sg));
            }
            _contentsForCharacter[character] = contents;
            return contents;
        }
    }
}
