using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.Foundation;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    public static class MatrixExt
    {
        public static DenseMatrix PreConcat(DenseMatrix matrix, DenseMatrix transformAnimationMatrix)
        {
            return matrix * transformAnimationMatrix;
        }

        public static DenseMatrix PreTranslate(DenseMatrix matrix, float dx, float dy)
        {
            return matrix * GetTranslate(dx, dy);
        }

        private static DenseMatrix GetTranslate(float dx, float dy)
        {
            return new DenseMatrix(3, 3)
            {
                [0, 0] = 1,
                [0, 2] = dx,
                [1, 1] = 1,
                [1, 2] = dy,
                [2, 2] = 1
            };
        }

        public static DenseMatrix PreRotate(DenseMatrix matrix, float rotation)
        {
            var angle = MathExt.ToRadians(rotation);
            var sin = Math.Sin(angle);
            var cos = Math.Cos(angle);

            return matrix * GetRotate(sin, cos);
        }

        public static DenseMatrix PreRotate(DenseMatrix matrix, float rotation, float px, float py)
        {
            var angle = MathExt.ToRadians(rotation);
            var sin = Math.Sin(angle);
            var cos = Math.Cos(angle);

            var tmp = GetTranslate(-px, -py) * GetRotate(sin, cos) * GetTranslate(px, py);

            return matrix * tmp;
        }

        private static DenseMatrix GetRotate(double sin, double cos)
        {
            return new DenseMatrix(3, 3)
            {
                [0, 0] = (float)cos,
                [0, 1] = (float)-sin,
                [1, 0] = (float)sin,
                [1, 1] = (float)cos,
                [2, 2] = 1
            };
        }

        public static DenseMatrix PreScale(DenseMatrix matrix, float scaleX, float scaleY)
        {
            var scaleMatrix = new DenseMatrix(3, 3)
            {
                [0, 0] = scaleX,
                [1, 1] = scaleY,
                [2, 2] = 1
            };

            return matrix * scaleMatrix;
        }

        public static void MapRect(this DenseMatrix matrix, ref Rect rect)
        {
            try
            {
                var multiplied = matrix *
                    DenseMatrix.OfArray(new[,]
                    {
                        {(float) rect.Left, (float) rect.Right, (float) rect.Left,   (float) rect.Right},
                        {(float) rect.Top,  (float) rect.Top,   (float) rect.Bottom, (float) rect.Bottom},
                        {1,                 1,                  1, 1}
                    });

                var xMin = Math.Min(Math.Min(Math.Min(multiplied[0, 0], multiplied[1, 0]), multiplied[0, 2]), multiplied[0, 3]);
                var xMax = Math.Max(Math.Max(Math.Max(multiplied[0, 0], multiplied[1, 0]), multiplied[0, 2]), multiplied[0, 3]);
                var yMax = Math.Max(Math.Max(Math.Max(multiplied[1, 0], multiplied[1, 0]), multiplied[1, 2]), multiplied[1, 3]);
                var yMin = Math.Min(Math.Min(Math.Min(multiplied[1, 0], multiplied[1, 0]), multiplied[1, 2]), multiplied[1, 3]);

                RectExt.Set(ref rect, new Rect(new Point(xMin, yMax), new Point(xMax, yMin)));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public static void MapPoints(this DenseMatrix matrix, ref float[] points)
        {
            var multiplied = matrix * 
                DenseMatrix.OfArray(new[,]
                {
                    {points[0], points[2]},
                    {points[1], points[3]},
                    {1,         1}
                });

            points[0] = multiplied[0, 0];
            points[1] = multiplied[1, 0];
            points[2] = multiplied[0, 1];
            points[3] = multiplied[1, 1];
        }

        public static IEnumerable<IEnumerable<T>> Partition<T>
            (this IEnumerable<T> source, int size)
        {
            T[] array = null;
            var count = 0;
            foreach (var item in source)
            {
                if (array == null)
                {
                    array = new T[size];
                }
                array[count] = item;
                count++;
                if (count == size)
                {
                    yield return new ReadOnlyCollection<T>(array);
                    array = null;
                    count = 0;
                }
            }
            if (array != null)
            {
                Array.Resize(ref array, count);
                yield return new ReadOnlyCollection<T>(array);
            }
        }

        public static void Set(this DenseMatrix matrix, DenseMatrix newMatrix)
        {
            newMatrix.CopyTo(matrix);
        }

        static readonly DenseMatrix Identity = DenseMatrix.CreateIdentity(3);

        public static void Reset(this DenseMatrix matrix)
        {
            Identity.CopyTo(matrix);
        }
    }
}