namespace LottieUWP.Value
{
    public class ScaleXy
    {
        internal ScaleXy(float sx, float sy)
        {
            ScaleX = sx;
            ScaleY = sy;
        }

        internal ScaleXy() : this(1f, 1f)
        {
        }

        internal virtual float ScaleX { get; }

        internal virtual float ScaleY { get; }

        public override string ToString()
        {
            return ScaleX + "x" + ScaleY;
        }
    }
}