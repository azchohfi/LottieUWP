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

        public ShapeStroke(string name, AnimatableFloatValue offset, List<AnimatableFloatValue> lineDashPattern, AnimatableColorValue color, AnimatableIntegerValue opacity, AnimatableFloatValue width, LineCapType capType, LineJoinType joinType, float miterLimit, bool hidden)
        {
            Name = name;
            DashOffset = offset;
            LineDashPattern = lineDashPattern;
            Color = color;
            Opacity = opacity;
            Width = width;
            CapType = capType;
            JoinType = joinType;
            MiterLimit = miterLimit;
            IsHidden = hidden;
        }

        public IContent ToContent(ILottieDrawable drawable, BaseLayer layer)
        {
            return new StrokeContent(drawable, layer, this);
        }

        internal string Name { get; }

        internal AnimatableColorValue Color { get; }

        internal AnimatableIntegerValue Opacity { get; }

        internal AnimatableFloatValue Width { get; }

        internal List<AnimatableFloatValue> LineDashPattern { get; }

        internal AnimatableFloatValue DashOffset { get; }

        internal LineCapType CapType { get; }

        internal LineJoinType JoinType { get; }

        internal float MiterLimit { get; }

        internal bool IsHidden { get; }
    }
}