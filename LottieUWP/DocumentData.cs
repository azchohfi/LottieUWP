using System;
using Windows.Data.Json;
using Windows.UI;

namespace LottieUWP
{
    internal class DocumentData
    {
        internal string Text;
        internal string FontName;
        internal int Size;
        internal int Justification;
        internal int Tracking;
        internal double LineHeight;
        internal Color Color;
        internal Color StrokeColor;
        internal int StrokeWidth;
        internal bool StrokeOverFill;

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

        internal virtual void Set(DocumentData documentData)
        {
            Text = documentData.Text;
            FontName = documentData.FontName;
            Size = documentData.Size;
            Justification = documentData.Justification;
            Tracking = documentData.Tracking;
            LineHeight = documentData.LineHeight;
            Color = documentData.Color;
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
