using System;
using System.Collections.Generic;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Content;

namespace LottieUWP.Parser
{
    static class ShapeStrokeParser
    {
        internal static ShapeStroke Parse(JsonReader reader, LottieComposition composition)
        {
            string name = null;
            AnimatableColorValue color = null;
            AnimatableFloatValue width = null;
            AnimatableIntegerValue opacity = null;
            ShapeStroke.LineCapType capType = ShapeStroke.LineCapType.Unknown;
            ShapeStroke.LineJoinType joinType = ShapeStroke.LineJoinType.Round;
            AnimatableFloatValue offset = null;
            float miterLimit = 0f;
            bool hidden = false;

            List<AnimatableFloatValue> lineDashPattern = new List<AnimatableFloatValue>();

            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "nm":
                        name = reader.NextString();
                        break;
                    case "c":
                        color = AnimatableValueParser.ParseColor(reader, composition);
                        break;
                    case "w":
                        width = AnimatableValueParser.ParseFloat(reader, composition);
                        break;
                    case "o":
                        opacity = AnimatableValueParser.ParseInteger(reader, composition);
                        break;
                    case "lc":
                        capType = (ShapeStroke.LineCapType)(reader.NextInt() - 1);
                        break;
                    case "lj":
                        joinType = (ShapeStroke.LineJoinType)(reader.NextInt() - 1);
                        break;
                    case "ml":
                        miterLimit = reader.NextDouble();
                        break;
                    case "hd":
                        hidden = reader.NextBoolean();
                        break;
                    case "d":
                        reader.BeginArray();
                        while (reader.HasNext())
                        {
                            String n = null;
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
                                        val = AnimatableValueParser.ParseFloat(reader, composition);
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

            return new ShapeStroke(name, offset, lineDashPattern, color, opacity, width, capType, joinType, miterLimit, hidden);
        }
    }
}
