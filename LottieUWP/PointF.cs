using System;

namespace LottieUWP
{
    public class PointF
    {
        public PointF(float x, float y)
        {
            X = x;
            Y = y;
        }

        public PointF()
        {
            X = 0;
            Y = 0;
        }

        public float X { get; set; }
        public float Y { get; set; }

        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y);
        }

        public double LengthSquared()
        {
            return X * X + Y * Y;
        }

        public void Set(float x, float y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(PointF pointf)
        {
            return X == pointf.X && Y == pointf.Y;
        }

        public override string ToString()
        {
            return $"X:{X} / Y:{Y}";
        }
    }
}
