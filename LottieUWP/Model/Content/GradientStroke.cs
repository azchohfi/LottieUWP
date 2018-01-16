using System.Collections.Generic;
using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    public class GradientStroke : IContentModel
    {
        public GradientStroke(string name, GradientType gradientType, AnimatableGradientColorValue gradientColor, AnimatableIntegerValue opacity, AnimatablePointValue startPoint, AnimatablePointValue endPoint, AnimatableFloatValue width, ShapeStroke.LineCapType capType, ShapeStroke.LineJoinType joinType, List<AnimatableFloatValue> lineDashPattern, AnimatableFloatValue dashOffset)
        {
            Name = name;
            GradientType = gradientType;
            GradientColor = gradientColor;
            Opacity = opacity;
            StartPoint = startPoint;
            EndPoint = endPoint;
            Width = width;
            CapType = capType;
            JoinType = joinType;
            LineDashPattern = lineDashPattern;
            DashOffset = dashOffset;
        }

        internal virtual string Name { get; }

        internal virtual GradientType GradientType { get; }

        internal virtual AnimatableGradientColorValue GradientColor { get; }

        internal virtual AnimatableIntegerValue Opacity { get; }

        internal virtual AnimatablePointValue StartPoint { get; }

        internal virtual AnimatablePointValue EndPoint { get; }

        internal virtual AnimatableFloatValue Width { get; }

        internal virtual ShapeStroke.LineCapType CapType { get; }

        internal virtual ShapeStroke.LineJoinType JoinType { get; }

        internal virtual List<AnimatableFloatValue> LineDashPattern { get; }

        internal virtual AnimatableFloatValue DashOffset { get; }

        public IContent ToContent(LottieDrawable drawable, BaseLayer layer)
        {
            return new GradientStrokeContent(drawable, layer, this);
        }
    }
}