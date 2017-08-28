using System;
using Windows.Data.Json;
using LottieUWP.Animation.Content;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Content
{
    internal class GradientFill : IContentModel
    {
        private GradientFill(string name, GradientType gradientType, PathFillType fillType, AnimatableGradientColorValue gradientColor, AnimatableIntegerValue opacity, AnimatablePointValue startPoint, AnimatablePointValue endPoint, AnimatableFloatValue highlightLength, AnimatableFloatValue highlightAngle)
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
            internal static GradientFill NewInstance(JsonObject json, LottieComposition composition)
            {
                var name = json.GetNamedString("nm");

                var jsonColor = json.GetNamedObject("g", null);
                if (jsonColor != null && jsonColor.ContainsKey("k"))
                {
                    // This is a hack because the "p" value which contains the number of color points is outside
                    // of "k" which contains the useful data.
                    var points = (int) jsonColor.GetNamedNumber("p");
                    jsonColor = jsonColor.GetNamedObject("k");
                    try
                    {
                        jsonColor["p"] = JsonValue.CreateNumberValue(points);
                    }
                    catch (Exception)
                    {
                        // Do nothing. This shouldn't fail.
                    }
                }
                AnimatableGradientColorValue color = null;
                if (jsonColor != null)
                {
                    color = AnimatableGradientColorValue.Factory.NewInstance(jsonColor, composition);
                }

                var jsonOpacity = json.GetNamedObject("o", null);
                AnimatableIntegerValue opacity = null;
                if (jsonOpacity != null)
                {
                    opacity = AnimatableIntegerValue.Factory.NewInstance(jsonOpacity, composition);
                }

                var fillTypeInt = (int) json.GetNamedNumber("r", 1);
                var fillType = fillTypeInt == 1 ? PathFillType.Winding : PathFillType.EvenOdd;

                var gradientTypeInt = (int) json.GetNamedNumber("t", 1);
                var gradientType = gradientTypeInt == 1 ? GradientType.Linear : GradientType.Radial;

                var jsonStartPoint = json.GetNamedObject("s", null);
                AnimatablePointValue startPoint = null;
                if (jsonStartPoint != null)
                {
                    startPoint = AnimatablePointValue.Factory.NewInstance(jsonStartPoint, composition);
                }

                var jsonEndPoint = json.GetNamedObject("e", null);
                AnimatablePointValue endPoint = null;
                if (jsonEndPoint != null)
                {
                    endPoint = AnimatablePointValue.Factory.NewInstance(jsonEndPoint, composition);
                }

                return new GradientFill(name, gradientType, fillType, color, opacity, startPoint, endPoint, null, null);
            }
        }
    }
}