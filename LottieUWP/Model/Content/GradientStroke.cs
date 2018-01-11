using System.Collections.Generic;
using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    internal class GradientStroke : IContentModel
    {
        private GradientStroke(string name, GradientType gradientType, AnimatableGradientColorValue gradientColor, AnimatableIntegerValue opacity, AnimatablePointValue startPoint, AnimatablePointValue endPoint, AnimatableFloatValue width, ShapeStroke.LineCapType capType, ShapeStroke.LineJoinType joinType, List<AnimatableFloatValue> lineDashPattern, AnimatableFloatValue dashOffset)
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

        internal static class Factory
        {
            internal static GradientStroke NewInstance(JsonReader reader, LottieComposition composition)
            {
                string name = null;
                AnimatableGradientColorValue color = null;
                AnimatableIntegerValue opacity = null;
                GradientType gradientType = GradientType.Radial;
                AnimatablePointValue startPoint = null;
                AnimatablePointValue endPoint = null;
                AnimatableFloatValue width = null;
                ShapeStroke.LineCapType capType = ShapeStroke.LineCapType.Unknown;
                ShapeStroke.LineJoinType joinType = ShapeStroke.LineJoinType.Round;
                AnimatableFloatValue offset = null;

                var lineDashPattern = new List<AnimatableFloatValue>();

                while (reader.HasNext())
                {
                    switch (reader.NextName())
                    {
                        case "nm":
                            name = reader.NextString();
                            break;
                        case "g":
                            int points = -1;
                            reader.BeginObject();
                            while (reader.HasNext())
                            {
                                switch (reader.NextName())
                                {
                                    case "p":
                                        points = reader.NextInt();
                                        break;
                                    case "k":
                                        color = AnimatableGradientColorValue.Factory
                                            .NewInstance(reader, composition, points);
                                        break;
                                    default:
                                        reader.SkipValue();
                                        break;
                                }
                            }

                            reader.EndObject();
                            break;
                        case "o":
                            opacity = AnimatableIntegerValue.Factory.NewInstance(reader, composition);
                            break;
                        case "t":
                            gradientType = reader.NextInt() == 1 ? GradientType.Linear : GradientType.Radial;
                            break;
                        case "s":
                            startPoint = AnimatablePointValue.Factory.NewInstance(reader, composition);
                            break;
                        case "e":
                            endPoint = AnimatablePointValue.Factory.NewInstance(reader, composition);
                            break;
                        case "w":
                            width = AnimatableFloatValue.Factory.NewInstance(reader, composition);
                            break;
                        case "lc":
                            capType = (ShapeStroke.LineCapType)(reader.NextInt() - 1);
                            break;
                        case "lj":
                            joinType = (ShapeStroke.LineJoinType)(reader.NextInt() - 1);
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

                                if (n.Equals("o"))
                                {
                                    offset = val;
                                }
                                else if (n.Equals("d") || n.Equals("g"))
                                {
                                    lineDashPattern.Add(val);
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

                return new GradientStroke(name, gradientType, color, opacity, startPoint, endPoint, width, capType, joinType, lineDashPattern, offset);
            }
        }
    }
}