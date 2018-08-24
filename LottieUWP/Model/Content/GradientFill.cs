using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    public class GradientFill : IContentModel
    {
        public GradientFill(string name, GradientType gradientType, PathFillType fillType,
            AnimatableGradientColorValue gradientColor, AnimatableIntegerValue opacity, AnimatablePointValue startPoint,
            AnimatablePointValue endPoint, AnimatableFloatValue highlightLength, AnimatableFloatValue highlightAngle)
        {
            GradientType = gradientType;
            FillType = fillType;
            GradientColor = gradientColor;
            Opacity = opacity;
            StartPoint = startPoint;
            EndPoint = endPoint;
            Name = name;
            HighlightLength = highlightLength;
            HighlightAngle = highlightAngle;
        }

        internal string Name { get; }

        internal GradientType GradientType { get; }

        internal PathFillType FillType { get; }

        internal AnimatableGradientColorValue GradientColor { get; }

        internal AnimatableIntegerValue Opacity { get; }

        internal AnimatablePointValue StartPoint { get; }

        internal AnimatablePointValue EndPoint { get; }

        internal AnimatableFloatValue HighlightLength { get; }

        internal AnimatableFloatValue HighlightAngle { get; }

        public IContent ToContent(LottieDrawable drawable, BaseLayer layer)
        {
            return new GradientFillContent(drawable, layer, this);
        }
    }
}