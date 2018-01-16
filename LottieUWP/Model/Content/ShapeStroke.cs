using System.Collections.Generic;
using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;
using Microsoft.Graphics.Canvas.Geometry;

namespace LottieUWP.Model.Content
{
    public class ShapeStroke : IContentModel
    {
        public enum LineCapType
        {
            Butt,
            Round,
            Unknown
        }

        internal static CanvasCapStyle LineCapTypeToPaintCap(LineCapType lineCapType)
        {
            switch (lineCapType)
            {
                case LineCapType.Butt:
                    return CanvasCapStyle.Flat;
                case LineCapType.Round:
                    return CanvasCapStyle.Round;
                case LineCapType.Unknown:
                default:
                    return CanvasCapStyle.Square;
            }
        }
        
        public enum LineJoinType
        {
            Miter,
            Round,
            Bevel
        }

        internal static CanvasLineJoin LineJoinTypeToPaintLineJoin(LineJoinType lineJoinType)
        {
            switch (lineJoinType)
            {
                case LineJoinType.Bevel:
                    return CanvasLineJoin.Bevel;
                case LineJoinType.Miter:
                    return CanvasLineJoin.Miter;
                case LineJoinType.Round:
                default:
                    return CanvasLineJoin.Round;
            }
        }

        public ShapeStroke(string name, AnimatableFloatValue offset, List<AnimatableFloatValue> lineDashPattern, AnimatableColorValue color, AnimatableIntegerValue opacity, AnimatableFloatValue width, LineCapType capType, LineJoinType joinType)
        {
            Name = name;
            DashOffset = offset;
            LineDashPattern = lineDashPattern;
            Color = color;
            Opacity = opacity;
            Width = width;
            CapType = capType;
            JoinType = joinType;
        }

        public IContent ToContent(LottieDrawable drawable, BaseLayer layer)
        {
            return new StrokeContent(drawable, layer, this);
        }

        internal virtual string Name { get; }

        internal virtual AnimatableColorValue Color { get; }

        internal virtual AnimatableIntegerValue Opacity { get; }

        internal virtual AnimatableFloatValue Width { get; }

        internal virtual List<AnimatableFloatValue> LineDashPattern { get; }

        internal virtual AnimatableFloatValue DashOffset { get; }

        internal virtual LineCapType CapType { get; }

        internal virtual LineJoinType JoinType { get; }
    }
}