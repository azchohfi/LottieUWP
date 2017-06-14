using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using MathNet.Numerics.LinearAlgebra.Single;

namespace LottieUWP
{
    public class BitmapCanvas : Canvas
    {
        private DenseMatrix _matrix = DenseMatrix.CreateIdentity(3);
        private readonly Stack<DenseMatrix> _matrixSaves = new Stack<DenseMatrix>();

        public BitmapCanvas(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public static int ClipSaveFlag;
        public static int ClipToLayerSaveFlag;
        public static int MatrixSaveFlag;
        public static int AllSaveFlag;

        public void DrawRect(double x1, double y1, double x2, double y2, Paint paint)
        {
            var rectangle = new Rectangle
            {
                Width = x2 - x1,
                Height = y2 - y1,
                RenderTransform = GetCurrentRenderTransform(),
                Fill = new SolidColorBrush(paint.PathEffect?.GetColor(paint) ?? paint.Color)
            };
            SetLeft(rectangle, x1);
            SetTop(rectangle, y1);
            Children.Add(rectangle);
        }

        internal void DrawRect(Rect rect, Paint paint)
        {
            var rectangle = new Rectangle
            {
                Width = rect.Width,
                Height = rect.Height,
                RenderTransform = GetCurrentRenderTransform(),
                Fill = new SolidColorBrush(paint.PathEffect?.GetColor(paint) ?? paint.Color)
            };
            SetLeft(rectangle, rect.Left);
            SetTop(rectangle, rect.Top);
            Children.Add(rectangle);
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
            for (int i = 0; i < path.Points.Count; i++)
            {
                var pointCollection = new PointCollection();
                foreach (var pointF in path.Points[i])
                {
                    pointCollection.Add(new Point(pointF.X, pointF.Y));
                }
                var polygon = new Polygon
                {
                    FillRule = path.FillType == PathFillType.EvenOdd ? FillRule.EvenOdd : FillRule.Nonzero,
                    Points = pointCollection,
                    RenderTransform = GetCurrentRenderTransform(),
                    Fill = new SolidColorBrush(paint.PathEffect?.GetColor(paint) ?? paint.Color)
                };
                Children.Add(polygon);
            }
        }

        private void DrawPathStroke(Path path, Paint paint)
        {
            for (var i = 0; i < path.Points.Count; i++)
            {
                var pointCollection = new PointCollection();
                foreach (var pointF in path.Points[i])
                {
                    pointCollection.Add(new Point(pointF.X, pointF.Y));
                }
                var dashPathEffect = paint.PathEffect as DashPathEffect;

                var polyline = new Polyline
                {
                    Points = pointCollection,
                    Stroke = new SolidColorBrush(paint.Color),
                    StrokeThickness = paint.StrokeWidth,
                    StrokeDashCap = paint.StrokeCap,
                    StrokeLineJoin = paint.StrokeJoin,
                    StrokeDashArray = dashPathEffect?.Intervals,
                    StrokeDashOffset = dashPathEffect?.Phase ?? 0,
                    RenderTransform = GetCurrentRenderTransform()
                };
                //paint.PathEffect?.GetColor(paint)
                Children.Add(polyline);
            }
        }

        private MatrixTransform GetCurrentRenderTransform()
        {
            return new MatrixTransform
            {
                Matrix = new Windows.UI.Xaml.Media.Matrix(_matrix[0, 0], _matrix[0, 1], _matrix[1, 0], _matrix[1, 1], _matrix[2, 0], _matrix[2, 1])
            };
        }

        public bool ClipRect(Rect newClipRect)
        {
            return true;
        }

        public void ClipRect(Rect originalClipRect, Region.Op replace)
        {

        }

        public void Concat(DenseMatrix parentMatrix)
        {
            _matrix = MatrixExt.PreConcat(_matrix, parentMatrix);
        }

        // concat or clipRect
        public void Save()
        {
            _matrixSaves.Push(_matrix);
        }

        public void SaveLayer(Rect rect, Paint contentPaint, object allSaveFlag)
        {
            _matrixSaves.Push(_matrix);
        }

        public void Restore()
        {
            _matrix = _matrixSaves.Pop();
        }

        public void DrawBitmap(ImageSource bitmap, Rect src, Rect dst, Paint paint)
        {
            _matrix.MapRect(ref dst);
            
            var image = new Image
            {
                Width = dst.Width,
                Height = dst.Height,
                Stretch = Stretch.Fill,
                RenderTransform = GetCurrentRenderTransform(),
                Source = bitmap
                //Fill = new SolidColorBrush(paint.PathEffect?.GetColor(paint) ?? paint.Color)
            };
            SetLeft(image, dst.X);
            SetTop(image, dst.Y);
            Children.Add(image);
            // TODO: Should use paint.ColorFilter and paint.Alpha
            //_bitmap.Blit(dst, writeableBitmap, src, WriteableBitmapExtensions.BlendMode.Additive);
        }

        public void GetClipBounds(out Rect originalClipRect)
        {
            RectExt.Set(ref originalClipRect, 0, 0, Width, Height);
        }

        public void Clear(Color color)
        {
            Children.Clear();
            Background = new SolidColorBrush(color);
        }

        public Viewbox GetImage()
        {
            return new Viewbox
            {
                Child = this,
                Width = Width,
                Height = Height,
                Stretch = Stretch.None
            };
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