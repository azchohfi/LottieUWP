using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using LottieUWP.Animation.Content;
using LottieUWP.Animation.Keyframe;

namespace LottieUWP.Model.Layer
{
    internal class TextLayer : BaseLayer
    {
        //private Rect _rectF;
        private readonly Paint _fillPaint = new Paint(Paint.AntiAliasFlag)
        {
            Style = Paint.PaintStyle.Fill
        };
        private readonly Paint _strokePaint = new Paint(Paint.AntiAliasFlag)
        {
            Style = Paint.PaintStyle.Stroke
        };

        private readonly Dictionary<FontCharacter, List<ContentGroup>> _contentsForCharacter = new Dictionary<FontCharacter, List<ContentGroup>>();
        private readonly TextKeyframeAnimation _textAnimation;
        private readonly LottieDrawable _lottieDrawable;
        private readonly LottieComposition _composition;
        private readonly IBaseKeyframeAnimation<Color, Color> _colorAnimation;
        private readonly IBaseKeyframeAnimation<Color, Color> _strokeAnimation;
        private readonly IBaseKeyframeAnimation<float?, float?> _strokeWidthAnimation;
        private readonly IBaseKeyframeAnimation<float?, float?> _trackingAnimation;

        internal TextLayer(LottieDrawable lottieDrawable, Layer layerModel) : base(lottieDrawable, layerModel)
        {
            _lottieDrawable = lottieDrawable;
            _composition = layerModel.Composition;
            _textAnimation = (TextKeyframeAnimation)layerModel.Text.CreateAnimation();
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

        public override void DrawLayer(BitmapCanvas canvas, Matrix3X3 parentMatrix, byte parentAlpha)
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
                var parentScale = Utils.Utils.GetScale(parentMatrix);
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

        private void DrawTextGlyphs(DocumentData documentData, Matrix3X3 parentMatrix, Font font, BitmapCanvas canvas)
        {
            var fontScale = (float)documentData.Size / 100;
            var parentScale = Utils.Utils.GetScale(parentMatrix);
            var text = documentData.Text;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                var characterHash = FontCharacter.HashFor(c, font.Family, font.Style);
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

        private void DrawTextWithFont(DocumentData documentData, Font font, Matrix3X3 parentMatrix, BitmapCanvas canvas)
        {
            var parentScale = Utils.Utils.GetScale(parentMatrix);
            var typeface = _lottieDrawable.GetTypeface(font.Family, font.Style);
            if (typeface == null)
            {
                return;
            }
            var text = documentData.Text;
            var textDelegate = _lottieDrawable.TextDelegate;
            if (textDelegate != null)
            {
                text = textDelegate.GetTextInternal(text);
            }
            _fillPaint.Typeface = typeface;
            _fillPaint.TextSize = documentData.Size * _composition.DpScale;
            _strokePaint.Typeface = _fillPaint.Typeface;
            _strokePaint.TextSize = _fillPaint.TextSize;
            for (var i = 0; i < text.Length; i++)
            {
                var character = text[i];
                var size = DrawCharacterFromFont(character, documentData, canvas);

                // Add tracking
                var tracking = documentData.Tracking / 10f;
                if (_trackingAnimation?.Value != null)
                {
                    tracking += _trackingAnimation.Value.Value;
                }
                var tx = (float)size.Width + tracking * parentScale;
                canvas.Translate(tx, 0);
            }
        }

        private void DrawCharacterAsGlyph(FontCharacter character, Matrix3X3 parentMatrix, float fontScale, DocumentData documentData, BitmapCanvas canvas)
        {
            var contentGroups = GetContentsForCharacter(character);
            for (var j = 0; j < contentGroups.Count; j++)
            {
                var path = contentGroups[j].Path;
                //path.ComputeBounds(out _rectF);
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

        private Rect DrawCharacterFromFont(char c, DocumentData documentData, BitmapCanvas canvas)
        {
            if (documentData.StrokeOverFill)
            {
                DrawCharacter(c, _fillPaint, canvas);
                return DrawCharacter(c, _strokePaint, canvas);
            }

            DrawCharacter(c, _strokePaint, canvas);
            return DrawCharacter(c, _fillPaint, canvas);
        }

        private Rect DrawCharacter(char character, Paint paint, BitmapCanvas canvas)
        {
            return canvas.DrawText(character, paint);
        }

        private List<ContentGroup> GetContentsForCharacter(FontCharacter character)
        {
            if (_contentsForCharacter.ContainsKey(character))
            {
                return _contentsForCharacter[character];
            }
            var shapes = character.Shapes;
            var size = shapes.Count;
            var contents = new List<ContentGroup>(size);
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
