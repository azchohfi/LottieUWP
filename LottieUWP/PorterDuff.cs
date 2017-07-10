using Microsoft.Graphics.Canvas;

namespace LottieUWP
{
    public static class PorterDuff
    {
        public enum Mode
        {
            Clear,
            DstIn,
            DstOut,
            SrcAtop
        }

        public static CanvasComposite ToCanvasComposite(Mode mode)
        {
            switch (mode)
            {
                case Mode.SrcAtop:
                    return CanvasComposite.SourceAtop;
                case Mode.DstIn:
                    return CanvasComposite.DestinationIn;
                case Mode.DstOut:
                    return CanvasComposite.DestinationOut;
                //case Mode.Clear:
                default:
                    return CanvasComposite.Copy;
            }
        }
    }
}