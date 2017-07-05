using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    internal class TextLayer : BaseLayer
    {
        private Rect _rectF;
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
            if (!_lottieDrawable.UseTextGlyphs())
            {
                canvas.SetMatrix(parentMatrix);
            }
            var documentData = _textAnimation.Value;
            if (!_composition.Fonts.TryGetValue(documentData.FontName, out var font))
            {
                // Something is wrong. 
                return;
            }

            _fillPaint.Color = _colorAnimation?.Value ?? documentData.Color;
            _strokePaint.Color = _strokeAnimation?.Value ?? documentData.StrokeColor;
            if (_strokeWidthAnimation?.Value != null)
            {
                _strokePaint.StrokeWidth = _strokeWidthAnimation.Value.Value;
            }
            else
            {
                var parentScale = Utils.GetScale(parentMatrix);
                _strokePaint.StrokeWidth = documentData.StrokeWidth * _composition.DpScale * parentScale;
            }

            if (_lottieDrawable.UseTextGlyphs())
            {
                DrawTextGlyphs(documentData, parentMatrix, font, canvas);
            }
            else
            {
                DrawTextWithFont(documentData, font, parentMatrix, canvas);
            }

            canvas.Restore();
        }

        private void DrawTextGlyphs(DocumentData documentData, DenseMatrix parentMatrix, Font font, BitmapCanvas canvas)
        {
            float fontScale = (float)documentData.Size / 100;
            var parentScale = Utils.GetScale(parentMatrix);
            var text = documentData.Text;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                int characterHash = FontCharacter.HashFor(c, font.Family, font.Style);
                if (!_composition.Characters.TryGetValue(characterHash, out var character))
                {
                    // Something is wrong. Potentially, they didn't export the text as a glyph. 
                    continue;
                }
                DrawCharacterAsGlyph(character, parentMatrix, fontScale, documentData, canvas);
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
        }

        private void DrawTextWithFont(DocumentData documentData, Font font, DenseMatrix parentMatrix, BitmapCanvas canvas)
        {
            float parentScale = Utils.GetScale(parentMatrix);
            Typeface typeface = _lottieDrawable.GetTypeface(font.Family, font.Style);
            if (typeface == null)
            {
                return;
            }
            var text = documentData.Text;
            TextDelegate textDelegate = _lottieDrawable.TextDelegate;
            if (textDelegate != null)
            {
                text = textDelegate.GetTextInternal(text);
            }
            _fillPaint.Typeface = typeface;
            _fillPaint.TextSize = documentData.Size * _composition.DpScale;
            _strokePaint.Typeface = _fillPaint.Typeface;
            _strokePaint.TextSize = _fillPaint.TextSize;
            for (int i = 0; i < text.Length; i++)
            {
                char character = text[i];
                DrawCharacterFromFont(character, documentData, canvas);
                float charWidth = _fillPaint.MeasureText(character);
                // Add tracking
                float tracking = documentData.Tracking / 10f;
                if (_trackingAnimation?.Value != null)
                {
                    tracking += _trackingAnimation.Value.Value;
                }
                float tx = charWidth + tracking * parentScale;
                canvas.Translate(tx, 0);
            }
        }

        private void DrawCharacterAsGlyph(FontCharacter character, DenseMatrix parentMatrix, float fontScale, DocumentData documentData, BitmapCanvas canvas)
        {
            var contentGroups = GetContentsForCharacter(character);
            for (var j = 0; j < contentGroups.Count; j++)
            {
                var path = contentGroups[j].Path;
                path.ComputeBounds(out _rectF);
                Matrix.Set(parentMatrix);
                Matrix = MatrixExt.PreScale(Matrix, fontScale, fontScale);
                path.Transform(Matrix);
                if (documentData.StrokeOverFill)
                {
                    DrawGlyph(path, _fillPaint, canvas);
                    DrawGlyph(path, _strokePaint, canvas);
                }
                else
                {
                    DrawGlyph(path, _strokePaint, canvas);
                    DrawGlyph(path, _fillPaint, canvas);
                }
            }
        }

        private void DrawGlyph(Path path, Paint paint, BitmapCanvas canvas)
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

        private void DrawCharacterFromFont(char c, DocumentData documentData, BitmapCanvas canvas)
        {
            if (documentData.StrokeOverFill)
            {
                DrawCharacter(c, _fillPaint, canvas);
                DrawCharacter(c, _strokePaint, canvas);
            }
            else
            {
                DrawCharacter(c, _strokePaint, canvas);
                DrawCharacter(c, _fillPaint, canvas);
            }
        }

        private void DrawCharacter(char character, Paint paint, BitmapCanvas canvas)
        {
            canvas.DrawText(character, paint);
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
