using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace LottieUWP
{
    public class Paint
    {
        public static int AntiAliasFlag;
        public static int FilterBitmapFlag;

        public Paint(int flag)
        {

        }

        public Paint()
        {
        }

        public enum PaintStyle
        {
            Fill,
            Stroke
        }

        public byte Alpha
        {
            get => Color.A;
            set
            {
                var color = Color;
                color.A = value;
                Color = color;
            }
        }

        public Color Color { get; set; }
        public PaintStyle Style { get; set; }
        public ColorFilter ColorFilter { get; set; }
        public PenLineCap StrokeCap { get; set; }
        public PenLineJoin StrokeJoin { get; set; }
        public float StrokeWidth { get; set; }
        public PathEffect PathEffect { get; set; }
        public PorterDuffXfermode Xfermode { get; set; }
        public Shader Shader { get; set; }
        public Typeface Typeface { get; set; }
        public float TextSize { get; set; }

        public float MeasureText(char character)
        {
            var textBlock = new TextBlock
            {
                Text = character.ToString(),
                FontSize = TextSize,
                FontFamily = Typeface.FontFamily,
                FontStyle = Typeface.Style,
                FontWeight = Typeface.Weight
            };
            textBlock.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            return (float)textBlock.DesiredSize.Width;
        }
    }
}