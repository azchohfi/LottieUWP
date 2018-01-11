using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    internal class GradientFill : IContentModel
    {
        private GradientFill(string name, GradientType gradientType, PathFillType fillType,
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

        internal virtual string Name { get; }

        internal virtual GradientType GradientType { get; }

        internal virtual PathFillType FillType { get; }

        internal virtual AnimatableGradientColorValue GradientColor { get; }

        internal virtual AnimatableIntegerValue Opacity { get; }

        internal virtual AnimatablePointValue StartPoint { get; }

        internal virtual AnimatablePointValue EndPoint { get; }

        internal virtual AnimatableFloatValue HighlightLength { get; }

        internal virtual AnimatableFloatValue HighlightAngle { get; }

        public IContent ToContent(LottieDrawable drawable, BaseLayer layer)
        {
            return new GradientFillContent(drawable, layer, this);
        }

        internal static class Factory
        {
            internal static GradientFill NewInstance(JsonReader reader, LottieComposition composition)
            {
                string name = null;
                AnimatableGradientColorValue color = null;
                AnimatableIntegerValue opacity = null;
                GradientType gradientType = GradientType.Linear;
                AnimatablePointValue startPoint = null;
                AnimatablePointValue endPoint = null;
                PathFillType fillType = PathFillType.EvenOdd;

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
                                        color = AnimatableGradientColorValue.Factory.NewInstance(reader, composition,
                                            points);
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
                        case "r":
                            fillType = reader.NextInt() == 1 ? PathFillType.Winding : PathFillType.EvenOdd;
                            break;
                        default:
                            reader.SkipValue();
                            break;
                    }
                }

                return new GradientFill(name, gradientType, fillType, color, opacity, startPoint, endPoint, null,
                        null);
            }
        }
    }
}