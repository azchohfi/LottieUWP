using Windows.UI;

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

        public enum Cap
        {
            Butt,
            Round,
            Square
        }

        public enum Join
        {
            Bevel,
            Miter,
            Round
        }

        public enum PaintStyle
        {
            Fill,
            FillAndStroke,
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
        public Cap StrokeCap { get; set; }
        public Join StrokeJoin { get; set; }
        public float StrokeWidth { get; set; }
        public PathEffect PathEffect { get; set; }
        public PorterDuffXfermode Xfermode { get; set; }
        public Shader Shader { get; set; }
    }
}