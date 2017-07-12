using System;
using Windows.Data.Json;
using Windows.UI;

namespace LottieUWP
{
    internal class DocumentData
    {
        internal readonly string Text;
        internal readonly string FontName;
        internal readonly int Size;
        internal readonly int Justification;
        internal readonly int Tracking;
        internal readonly double LineHeight;
        internal readonly Color Color;
        internal readonly Color StrokeColor;
        internal readonly int StrokeWidth;
        internal readonly bool StrokeOverFill;

        internal DocumentData(string text, string fontName, int size, int justification, int tracking, double lineHeight, Color color, Color strokeColor, int strokeWidth, bool strokeOverFill)
        {
            Text = text;
            FontName = fontName;
            Size = size;
            Justification = justification;
            Tracking = tracking;
            LineHeight = lineHeight;
            Color = color;
            StrokeColor = strokeColor;
            StrokeWidth = strokeWidth;
            StrokeOverFill = strokeOverFill;
        }

        internal static class Factory
        {
            internal static DocumentData NewInstance(JsonObject json)
            {
                string text = json.GetNamedString("t", "");
                string fontName = json.GetNamedString("f", "");
                int size = (int)json.GetNamedNumber("s", 0);
                int justification = (int)json.GetNamedNumber("j", 0);
                int tracking = (int)json.GetNamedNumber("tr", 0);
                double lineHeight = json.GetNamedNumber("lh", 0);
                var colorArray = json.GetNamedArray("fc");
                var color = Color.FromArgb(255, (byte)(colorArray.GetNumberAt(0) * 255), (byte)(colorArray.GetNumberAt(1) * 255), (byte)(colorArray.GetNumberAt(2) * 255));

                var strokeArray = json.GetNamedArray("sc", null);
                Color strokeColor;
                if (strokeArray != null)
                {
                    strokeColor = Color.FromArgb(
                        255,
                        (byte)(strokeArray.GetNumberAt(0) * 255),
                        (byte)(strokeArray.GetNumberAt(1) * 255),
                        (byte)(strokeArray.GetNumberAt(2) * 255));
                }

                int strokeWidth = (int)json.GetNamedNumber("sw", 0);
                var strokeOverFill = json.GetNamedBoolean("of", false);

                return new DocumentData(text, fontName, size, justification, tracking, lineHeight, color, strokeColor, strokeWidth, strokeOverFill);
            }
        }

        public override int GetHashCode()
        {
            int result;
            long temp;
            result = Text.GetHashCode();
            result = 31 * result + FontName.GetHashCode();
            result = 31 * result + Size;
            result = 31 * result + Justification;
            result = 31 * result + Tracking;
            temp = BitConverter.DoubleToInt64Bits(LineHeight);
            result = 31 * result + (int)(temp ^ ((long)((ulong)temp >> 32)));
            result = 31 * result + Color.GetHashCode();
            return result;
        }
    }
}
