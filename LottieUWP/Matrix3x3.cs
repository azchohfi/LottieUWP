using System.Numerics;

namespace LottieUWP
{
    public class Matrix3X3
    {
        public float M11;
        public float M12;
        public float M13;
        public float M21;
        public float M22;
        public float M23;
        public float M31;
        public float M32;
        public float M33;

        public static Matrix3X3 CreateIdentity() => new Matrix3X3
        {
            M11 = 1,
            M12 = 0,
            M13 = 0,
            M21 = 0,
            M22 = 1,
            M23 = 0,
            M31 = 0,
            M32 = 0,
            M33 = 1
        };

        public void Set(Matrix3X3 m)
        {
            M11 = m.M11;
            M12 = m.M12;
            M13 = m.M13;
            M21 = m.M21;
            M22 = m.M22;
            M23 = m.M23;
            M31 = m.M31;
            M32 = m.M32;
            M33 = m.M33;
        }

        public void Reset()
        {
            M11 = 1;
            M12 = 0;
            M13 = 0;
            M21 = 0;
            M22 = 1;
            M23 = 0;
            M31 = 0;
            M32 = 0;
            M33 = 1;
        }

        public static Matrix3X3 operator *(Matrix3X3 m1, Matrix3X3 m2)
        {
            return new Matrix3X3
            {
                M11 = m1.M11 * m2.M11 + m1.M12 * m2.M21 + m1.M13 * m2.M31,
                M12 = m1.M11 * m2.M12 + m1.M12 * m2.M22 + m1.M13 * m2.M32,
                M13 = m1.M11 * m2.M13 + m1.M12 * m2.M23 + m1.M13 * m2.M33,
                M21 = m1.M21 * m2.M11 + m1.M22 * m2.M21 + m1.M23 * m2.M31,
                M22 = m1.M21 * m2.M12 + m1.M22 * m2.M22 + m1.M23 * m2.M32,
                M23 = m1.M21 * m2.M13 + m1.M22 * m2.M23 + m1.M23 * m2.M33,
                M31 = m1.M31 * m2.M11 + m1.M32 * m2.M21 + m1.M33 * m2.M31,
                M32 = m1.M31 * m2.M12 + m1.M32 * m2.M22 + m1.M33 * m2.M32,
                M33 = m1.M31 * m2.M13 + m1.M32 * m2.M23 + m1.M33 * m2.M33
            };
        }

        public Vector2 Transform(Vector2 v)
        {
            return new Vector2(
                v.X * M11 + v.Y * M12 + M13,
                v.X * M21 + v.Y * M22 + M23);
        }
    }
}
