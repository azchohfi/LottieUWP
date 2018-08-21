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

        internal float ScaleX { get; }

        internal float ScaleY { get; }

        public override string ToString()
        {
            return ScaleX + "x" + ScaleY;
        }
    }
}