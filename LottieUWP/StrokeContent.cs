using Windows.UI;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    internal class StrokeContent : BaseStrokeContent
    {
        private readonly KeyframeAnimation<Color> _colorAnimation;

        internal StrokeContent(LottieDrawable lottieDrawable, BaseLayer layer, ShapeStroke stroke) : base(lottieDrawable, layer, stroke.CapType.ToPaintCap(), stroke.JoinType.ToPaintJoin(), stroke.Opacity, stroke.Width, stroke.LineDashPattern, stroke.DashOffset)
        {
            Name = stroke.Name;
            _colorAnimation = (KeyframeAnimation<Color>)stroke.Color.CreateAnimation();
            _colorAnimation.AddUpdateListener(this);
            layer.AddAnimation(_colorAnimation);
        }

        public override void AddColorFilter(string layerName, string contentName, ColorFilter colorFilter)
        {
            Paint.ColorFilter = colorFilter;
        }

        public override void Draw(BitmapCanvas canvas, DenseMatrix parentMatrix, int parentAlpha)
        {
            Paint.Color = _colorAnimation.Value;
            base.Draw(canvas, parentMatrix, parentAlpha);
        }

        public override string Name { get; }
    }
}