using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    public class BitmapCanvas
    {
        internal readonly WriteableBitmap Bitmap;

        public BitmapCanvas(WriteableBitmap bitmap)
        {
            Bitmap = bitmap;
        }

        public static int ClipSaveFlag;
        public static int ClipToLayerSaveFlag;
        public static int MatrixSaveFlag;
        public static int AllSaveFlag;

        public double Width => Bitmap.PixelWidth;

        public double Height => Bitmap.PixelHeight;

        public void SaveLayer(Rect rect, Paint contentPaint, object allSaveFlag)
        {

        }

        public void Restore()
        {

        }

        public void DrawRect(double x1, double y1, double x2, double y2, Paint paint)
        {
            Bitmap.DrawRectangle((int)x1, (int)y1, (int)x2, (int)y2, paint.Color);
        }

        internal void DrawRect(Rect rect, Paint paint)
        {
            Bitmap.DrawRectangle((int) rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom, paint.Color);
        }

        public void DrawPath(Path path, Paint paint)
        {
            if (paint.Style == Paint.PaintStyle.Stroke)
            {
                DrawPathStroke(path, paint);
            }
            else if (paint.Style == Paint.PaintStyle.Fill)
            {
                DrawPathFill(path, paint);
            }
            else if (paint.Style == Paint.PaintStyle.FillAndStroke)
            {
                DrawPathFill(path, paint);
                DrawPathStroke(path, paint);
            }
        }

        private void DrawPathFill(Path path, Paint paint)
        {
            if (path.FillType == PathFillType.EvenOdd)
            {
                var polygons = path.Points
                    .Select(poly => poly.Select(p => new[] {(int) p.X, (int) p.Y}).SelectMany(p => p).ToArray())
                    .ToArray();
                Bitmap.FillPolygonsEvenOdd(polygons, paint.Color);
            }
            else
            {
                if (paint.PathEffect != null)
                {
                    for (var i = 0; i < path.Points.Count; i++)
                    {
                        Bitmap.FillPolygon(
                            path.Points[i].Select(p => new[] { (int)p.X, (int)p.Y }).SelectMany(p => p).ToArray(),
                            paint.PathEffect.GetColor(paint));
                    }
                }
                else
                {
                    for (var i = 0; i < path.Points.Count; i++)
                    {
                        Bitmap.FillPolygon(
                            path.Points[i].Select(p => new[] {(int) p.X, (int) p.Y}).SelectMany(p => p).ToArray(),
                            paint.Color);
                    }
                }
            }
        }

        private void DrawPathStroke(Path path, Paint paint)
        {
            for (var i = 0; i < path.Points.Count; i++)
            {
                for (int j = 0; j < path.Points[i].Count - 1; j++)
                {
                    var p1 = path.Points[i][j];
                    var p2 = path.Points[i][j + 1];
                    Bitmap.DrawLineAa((int) p1.X, (int) p1.Y, (int) p2.X, (int) p2.Y, paint.Color, (int)paint.StrokeWidth);
                }
            }
        }

        public bool ClipRect(Rect newClipRect)
        {
            return true;
        }

        public void ClipRect(Rect originalClipRect, Region.Op replace)
        {

        }

        public void GetClipBounds(out Rect originalClipRect)
        {
            RectExt.Set(ref originalClipRect, 0, 0, Bitmap.PixelWidth, Bitmap.PixelHeight);
        }

        public void Save()
        {

        }

        public void Concat(DenseMatrix parentMatrix)
        {
            
        }

        public void DrawBitmap(BitmapSource bitmap, Rect src, Rect dst, Paint paint)
        {
            if (bitmap is WriteableBitmap writeableBitmap)
            {
                // TODO: Should use paint.ColorFilter and paint.Alpha
                Bitmap.Blit(dst, writeableBitmap, src, WriteableBitmapExtensions.BlendMode.Additive);
            }
        }
    }

    public class Region
    {
        public enum Op
        {
            Replace
        }
    }
}