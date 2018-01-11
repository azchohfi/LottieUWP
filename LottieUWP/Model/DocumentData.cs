using System;
using Windows.UI;
using LottieUWP.Utils;

namespace LottieUWP.Model
{
    internal class DocumentData
    {
        internal readonly string Text;
        internal readonly string FontName;
        internal readonly double Size;
        internal readonly int Justification;
        internal readonly int Tracking;
        internal readonly double LineHeight;
        internal readonly double BaselineShift;
        internal readonly Color Color;
        internal readonly Color StrokeColor;
        internal readonly int StrokeWidth;
        internal readonly bool StrokeOverFill;

        internal DocumentData(string text, string fontName, double size, int justification, int tracking, double lineHeight, double baselineShift, Color color, Color strokeColor, int strokeWidth, bool strokeOverFill)
        {
            Text = text;
            FontName = fontName;
            Size = size;
            Justification = justification;
            Tracking = tracking;
            LineHeight = lineHeight;
            BaselineShift = baselineShift;
            Color = color;
            StrokeColor = strokeColor;
            StrokeWidth = strokeWidth;
            StrokeOverFill = strokeOverFill;
        }

        internal static class Factory
        {
            internal static DocumentData NewInstance(JsonReader reader)
            {
                string text = null;
                string fontName = null;
                double size = 0;
                int justification = 0;
                int tracking = 0;
                double lineHeight = 0;
                double baselineShift = 0;
                Color fillColor;
                Color strokeColor;
                int strokeWidth = 0;
                bool strokeOverFill = true;

                reader.BeginObject();
                while (reader.HasNext())
                {
                    switch (reader.NextName())
                    {
                        case "t":
                            text = reader.NextString();
                            break;
                        case "f":
                            fontName = reader.NextString();
                            break;
                        case "s":
                            size = reader.NextDouble();
                            break;
                        case "j":
                            justification = reader.NextInt();
                            break;
                        case "tr":
                            tracking = reader.NextInt();
                            break;
                        case "lh":
                            lineHeight = reader.NextDouble();
                            break;
                        case "ls":
                            baselineShift = reader.NextDouble();
                            break;
                        case "fc":
                            fillColor = JsonUtils.JsonToColor(reader);
                            break;
                        case "sc":
                            strokeColor = JsonUtils.JsonToColor(reader);
                            break;
                        case "sw":
                            strokeWidth = reader.NextInt();
                            break;
                        case "of":
                            strokeOverFill = reader.NextBoolean();
                            break;
                        default:
                            reader.SkipValue();
                            break;
                    }
                }
                reader.EndObject();

                return new DocumentData(text, fontName, size, justification, tracking, lineHeight, baselineShift, fillColor, strokeColor, strokeWidth, strokeOverFill);
            }
        }

        public override int GetHashCode()
        {
            int result;
            long temp;
            result = Text.GetHashCode();
            result = 31 * result + FontName.GetHashCode();
            result = (int)(31 * result + Size);
            result = 31 * result + Justification;
            result = 31 * result + Tracking;
            temp = BitConverter.DoubleToInt64Bits(LineHeight);
            result = 31 * result + (int)(temp ^ ((long)((ulong)temp >> 32)));
            result = 31 * result + Color.GetHashCode();
            return result;
        }
    }
}
