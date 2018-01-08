using System.Collections.Generic;
using Windows.Data.Json;
using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;
using Microsoft.Graphics.Canvas.Geometry;

namespace LottieUWP.Model.Content
{
    internal class ShapeStroke : IContentModel
    {
        internal enum LineCapType
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
        
        internal enum LineJoinType
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

        private ShapeStroke(string name, AnimatableFloatValue offset, List<AnimatableFloatValue> lineDashPattern, AnimatableColorValue color, AnimatableIntegerValue opacity, AnimatableFloatValue width, LineCapType capType, LineJoinType joinType)
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

        internal static class Factory
        {
            internal static ShapeStroke NewInstance(JsonObject json, LottieComposition composition)
            {
                var name = json.GetNamedString("nm");
                var lineDashPattern = new List<AnimatableFloatValue>();
                var color = AnimatableColorValue.Factory.NewInstance(json.GetNamedObject("c"), composition);
                var width = AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("w"), composition);
                var opacity = AnimatableIntegerValue.Factory.NewInstance(json.GetNamedObject("o"), composition);
                var capType = (LineCapType)(int)(json.GetNamedNumber("lc") - 1);
                var joinType = (LineJoinType)(int)(json.GetNamedNumber("lj") - 1);
                AnimatableFloatValue offset = null;

                if (json.ContainsKey("d"))
                {
                    var dashesJson = json.GetNamedArray("d");
                    for (uint i = 0; i < dashesJson.Count; i++)
                    {
                        var dashJson = dashesJson.GetObjectAt(i);
                        var n = dashJson.GetNamedString("n");
                        if (n.Equals("o"))
                        {
                            var value = dashJson.GetNamedObject("v");
                            offset = AnimatableFloatValue.Factory.NewInstance(value, composition);
                        }
                        else if (n.Equals("d") || n.Equals("g"))
                        {
                            var value = dashJson.GetNamedObject("v");
                            lineDashPattern.Add(AnimatableFloatValue.Factory.NewInstance(value, composition));
                        }
                    }
                    if (lineDashPattern.Count == 1)
                    {
                        // If there is only 1 value then it is assumed to be equal parts on and off.
                        lineDashPattern.Add(lineDashPattern[0]);
                    }
                }
                return new ShapeStroke(name, offset, lineDashPattern, color, opacity, width, capType, joinType);
            }
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