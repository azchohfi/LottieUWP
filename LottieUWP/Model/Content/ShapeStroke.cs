using System.Collections.Generic;
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
            internal static ShapeStroke NewInstance(JsonReader reader, LottieComposition composition)
            {
                string name = null;
                AnimatableColorValue color = null;
                AnimatableFloatValue width = null;
                AnimatableIntegerValue opacity = null;
                LineCapType capType = LineCapType.Unknown;
                LineJoinType joinType = LineJoinType.Round;
                AnimatableFloatValue offset = null;

                List<AnimatableFloatValue> lineDashPattern = new List<AnimatableFloatValue>();

                while (reader.HasNext())
                {
                    switch (reader.NextName())
                    {
                        case "nm":
                            name = reader.NextString();
                            break;
                        case "c":
                            color = AnimatableColorValue.Factory.NewInstance(reader, composition);
                            break;
                        case "w":
                            width = AnimatableFloatValue.Factory.NewInstance(reader, composition);
                            break;
                        case "o":
                            opacity = AnimatableIntegerValue.Factory.NewInstance(reader, composition);
                            break;
                        case "lc":
                            capType = (LineCapType)(reader.NextInt() - 1);
                            break;
                        case "lj":
                            joinType = (LineJoinType)(reader.NextInt() - 1);
                            break;
                        case "d":
                            reader.BeginArray();
                            while (reader.HasNext())
                            {
                                string n = null;
                                AnimatableFloatValue val = null;

                                reader.BeginObject();
                                while (reader.HasNext())
                                {
                                    switch (reader.NextName())
                                    {
                                        case "n":
                                            n = reader.NextString();
                                            break;
                                        case "v":
                                            val = AnimatableFloatValue.Factory.NewInstance(reader, composition);
                                            break;
                                        default:
                                            reader.SkipValue();
                                            break;
                                    }
                                }
                                reader.EndObject();

                                switch (n)
                                {
                                    case "o":
                                        offset = val;
                                        break;
                                    case "d":
                                    case "g":
                                        lineDashPattern.Add(val);
                                        break;
                                }
                            }
                            reader.EndArray();

                            if (lineDashPattern.Count == 1)
                            {
                                // If there is only 1 value then it is assumed to be equal parts on and off. 
                                lineDashPattern.Add(lineDashPattern[0]);
                            }
                            break;
                        default:
                            reader.SkipValue();
                            break;
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