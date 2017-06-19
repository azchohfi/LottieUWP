using System.Collections.Generic;
using System.Linq;
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
            var dashPathEffect = paint.PathEffect as DashPathEffect;

            var pathSegmentCollection = new PathSegmentCollection();

            var firstPoint = path.Contours.FirstOrDefault()?.Points?.First();
            if (firstPoint == null)
                return;

            var windowsPath = new Windows.UI.Xaml.Shapes.Path
            {
                Stroke = new SolidColorBrush(paint.Color),
                StrokeThickness = paint.StrokeWidth,
                StrokeDashCap = paint.StrokeCap,
                StrokeLineJoin = paint.StrokeJoin,
                StrokeDashArray = dashPathEffect?.Intervals,
                StrokeDashOffset = dashPathEffect?.Phase ?? 0,
                RenderTransform = GetCurrentRenderTransform(),
                Data = new PathGeometry
                {
                    FillRule = path.FillType == PathFillType.EvenOdd ? FillRule.EvenOdd : FillRule.Nonzero,
                    Figures = new PathFigureCollection
                    {
                        new PathFigure
                        {
                            StartPoint = new Point(firstPoint.X, firstPoint.Y),
                            Segments = pathSegmentCollection
                        }
                    }
                }
            };
            if (paint.Style != Paint.PaintStyle.Stroke)
            {
                windowsPath.Fill = new SolidColorBrush(paint.PathEffect?.GetColor(paint) ?? paint.Color);
            }

            for (var i = 0; i < path.Contours.Count; i++)
            {
                var contour = path.Contours[i];

                var pointCollection = new PointCollection();
                PathSegment pathSegment;
                if (contour is Path.BezierCurve)
                {
                    foreach (var pointF in contour.Points.Skip(1))
                    {
                        pointCollection.Add(new Point(pointF.X, pointF.Y));
                    }
                    pathSegment = new PolyBezierSegment
                    {
                        Points = pointCollection
                    };
                }
                else
                {
                    foreach (var pointF in contour.Points)
                    {
                        pointCollection.Add(new Point(pointF.X, pointF.Y));
                    }
                    pathSegment = new PolyLineSegment
                    {
                        Points = pointCollection
                    };
                }
                pathSegmentCollection.Add(pathSegment);
            }
            Children.Add(windowsPath);
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
                Source = bitmap,
                Opacity = paint.Alpha / 255f
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