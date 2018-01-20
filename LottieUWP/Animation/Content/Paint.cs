using Windows.UI;
using Microsoft.Graphics.Canvas.Geometry;

namespace LottieUWP.Animation.Content
{
    public class Paint
    {
        public static int AntiAliasFlag = 0b01;
        public static int FilterBitmapFlag = 0b10;

        public int Flags { get; }

        public Paint(int flags)
        {
            Flags = flags;
        }

        public Paint()
            : this(0)
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

        public Color Color { get; set; } = Colors.Transparent;
        public PaintStyle Style { get; set; }
        public ColorFilter ColorFilter { get; set; }
        public CanvasCapStyle StrokeCap { get; set; }
        public CanvasLineJoin StrokeJoin { get; set; }
        public float StrokeWidth { get; set; }
        public PathEffect PathEffect { get; set; }
        public PorterDuffXfermode Xfermode { get; set; }
        public Shader Shader { get; set; }
        public Typeface Typeface { get; set; }
        public float TextSize { get; set; }
    }
}