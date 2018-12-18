using System.Collections.Generic;
using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    public class GradientStroke : IContentModel
    {
        public GradientStroke(string name, GradientType gradientType, AnimatableGradientColorValue gradientColor, AnimatableIntegerValue opacity, AnimatablePointValue startPoint, AnimatablePointValue endPoint, AnimatableFloatValue width, ShapeStroke.LineCapType capType, ShapeStroke.LineJoinType joinType, float miterLimit, List<AnimatableFloatValue> lineDashPattern, AnimatableFloatValue dashOffset, bool hidden)
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
            MiterLimit = miterLimit;
            LineDashPattern = lineDashPattern;
            DashOffset = dashOffset;
            IsHidden = hidden;
        }

        internal string Name { get; }

        internal GradientType GradientType { get; }

        internal AnimatableGradientColorValue GradientColor { get; }

        internal AnimatableIntegerValue Opacity { get; }

        internal AnimatablePointValue StartPoint { get; }

        internal AnimatablePointValue EndPoint { get; }

        internal AnimatableFloatValue Width { get; }

        internal ShapeStroke.LineCapType CapType { get; }

        internal ShapeStroke.LineJoinType JoinType { get; }

        internal float MiterLimit { get; }

        internal bool IsHidden { get; }

        internal List<AnimatableFloatValue> LineDashPattern { get; }

        internal AnimatableFloatValue DashOffset { get; }

        public IContent ToContent(ILottieDrawable drawable, BaseLayer layer)
        {
            return new GradientStrokeContent(drawable, layer, this);
        }
    }
}