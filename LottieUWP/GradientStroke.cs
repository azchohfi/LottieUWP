using System.Collections.Generic;
using Windows.Data.Json;

namespace LottieUWP
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
            internal static GradientStroke NewInstance(JsonObject json, LottieComposition composition)
            {
                var name = json.GetNamedString("nm");
                var jsonColor = json.GetNamedObject("g", null);
                if (jsonColor != null && jsonColor.ContainsKey("k"))
                {
                    jsonColor = jsonColor.GetNamedObject("k");
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

                var gradientTypeInt = (int)json.GetNamedNumber("t", 1);
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
                var width = AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("w"), composition);


                var capType = (ShapeStroke.LineCapType)(int)(json.GetNamedNumber("lc") - 1);
                var joinType = (ShapeStroke.LineJoinType)(int)(json.GetNamedNumber("lj") - 1);

                AnimatableFloatValue offset = null;
                var lineDashPattern = new List<AnimatableFloatValue>();
                if (json.ContainsKey("d"))
                {
                    var dashesJson = json.GetNamedArray("d");
                    for (var i = 0; i < dashesJson.Count; i++)
                    {
                        var dashJson = dashesJson[i].GetObject();
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

                return new GradientStroke(name, gradientType, color, opacity, startPoint, endPoint, width, capType, joinType, lineDashPattern, offset);
            }
        }
    }
}