using Windows.UI;
using LottieUWP.Animation.Keyframe;
using LottieUWP.Model.Content;
using LottieUWP.Model.Layer;

namespace LottieUWP.Animation.Content
{
    internal class StrokeContent : BaseStrokeContent
    {
        private readonly KeyframeAnimation<Color> _colorAnimation;

        internal StrokeContent(LottieDrawable lottieDrawable, BaseLayer layer, ShapeStroke stroke) : base(lottieDrawable, layer, ShapeStroke.LineCapTypeToPaintCap(stroke.CapType), ShapeStroke.LineJoinTypeToPaintLineJoin(stroke.JoinType), stroke.Opacity, stroke.Width, stroke.LineDashPattern, stroke.DashOffset)
        {
            Name = stroke.Name;
            _colorAnimation = (KeyframeAnimation<Color>)stroke.Color.CreateAnimation();
            _colorAnimation.ValueChanged += OnValueChanged;
            layer.AddAnimation(_colorAnimation);
        }

        public override void AddColorFilter(string layerName, string contentName, ColorFilter colorFilter)
        {
            Paint.ColorFilter = colorFilter;
        }

        public override void Draw(BitmapCanvas canvas, Matrix3X3 parentMatrix, byte parentAlpha)
        {
            Paint.Color = _colorAnimation.Value;
            base.Draw(canvas, parentMatrix, parentAlpha);
        }

        public override string Name { get; }
    }
}