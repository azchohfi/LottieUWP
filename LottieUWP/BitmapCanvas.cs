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
            Clip = new RectangleGeometry
            {
                Rect = new Rect(0, 0, Width, Height)
            };
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
                Fill = new SolidColorBrush(paint.Color)
            };
            paint.PathEffect?.Apply(rectangle, paint);
            SetLeft(rectangle, x1);
            SetTop(rectangle, y1);
            Children.Add(rectangle);
        }

        internal void DrawRect(Rect rect, Paint paint)
        {
            // TODO paint.ColorFilter
            var gradient = paint.Shader as Gradient;
            var brush = gradient != null ? gradient.GetBrush(paint.Alpha) : new SolidColorBrush(paint.Color);

            var rectangle = new Rectangle
            {
                Width = rect.Width,
                Height = rect.Height,
                RenderTransform = GetCurrentRenderTransform(),
                Fill = brush
            };
            paint.PathEffect?.Apply(rectangle, paint);
            SetLeft(rectangle, rect.Left);
            SetTop(rectangle, rect.Top);
            Children.Add(rectangle);
        }

        public void DrawPath(Path path, Paint paint)
        {
            var firstPoint = path.Contours.FirstOrDefault()?.First;
            if (firstPoint == null)
                return;

            var pathFigureCollection = new PathFigureCollection();
            var windowsPath = GetWindowsPath(path, paint, pathFigureCollection);
            var pathFigure = new PathFigure
            {
                StartPoint = new Point(firstPoint.X, firstPoint.Y),
                IsClosed = false,
                Segments = new PathSegmentCollection()
            };
            pathFigureCollection.Add(pathFigure);
            Children.Add(windowsPath);

            var returnDecision = Path.DrawReturnType.JustDraw;
            for (var i = 0; i < path.Contours.Count; i++)
            {
                if (returnDecision == Path.DrawReturnType.NewPath)
                {
                    firstPoint = path.Contours[i].First;
                    pathFigureCollection = new PathFigureCollection();
                    windowsPath = GetWindowsPath(path, paint, pathFigureCollection);
                    pathFigure = new PathFigure
                    {
                        StartPoint = new Point(firstPoint.X, firstPoint.Y),
                        IsClosed = false,
                        Segments = new PathSegmentCollection()
                    };
                    pathFigureCollection.Add(pathFigure);
                    Children.Add(windowsPath);
                }

                returnDecision = path.Contours[i].AddPathSegment(pathFigure);

                if (returnDecision == Path.DrawReturnType.NewFigure)
                {
                    pathFigure = new PathFigure
                    {
                        StartPoint = new Point(path.Contours[i].First.X, path.Contours[i].First.Y),
                        IsClosed = false,
                        Segments = new PathSegmentCollection()
                    };
                    pathFigureCollection.Add(pathFigure);
                }
            }
        }

        private Windows.UI.Xaml.Shapes.Path GetWindowsPath(Path path, Paint paint, PathFigureCollection pathFigureCollection)
        {
            // TODO paint.ColorFilter
            var isStroke = paint.Style == Paint.PaintStyle.Stroke;

            var gradient = paint.Shader as Gradient;
            var brush = gradient != null ? gradient.GetBrush(paint.Alpha) : new SolidColorBrush(paint.Color);

            var windowsPath = new Windows.UI.Xaml.Shapes.Path
            {
                Stroke = isStroke ? brush : null,
                StrokeThickness = paint.StrokeWidth,
                StrokeDashCap = paint.StrokeCap,
                StrokeLineJoin = paint.StrokeJoin,
                RenderTransform = GetCurrentRenderTransform(),
                Data = new PathGeometry
                {
                    FillRule = path.FillType == PathFillType.EvenOdd ? FillRule.EvenOdd : FillRule.Nonzero,
                    Figures = pathFigureCollection
                }
            };
            paint.PathEffect?.Apply(windowsPath, paint);
            if (!isStroke)
            {
                windowsPath.Fill = brush;
            }
            return windowsPath;
        }

        private MatrixTransform GetCurrentRenderTransform()
        {
            return new MatrixTransform
            {
                Matrix = new Windows.UI.Xaml.Media.Matrix(_matrix[0, 0], _matrix[1, 0], _matrix[0, 1], _matrix[1, 1], _matrix[0, 2], _matrix[1, 2])
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
            var copy = new DenseMatrix(3);
            _matrix.CopyTo(copy);
            _matrixSaves.Push(copy);
        }

        public void SaveLayer(Rect rect, Paint contentPaint, object allSaveFlag)
        {
            var copy = new DenseMatrix(3);
            _matrix.CopyTo(copy);
            _matrixSaves.Push(copy);
        }

        public void Restore()
        {
            _matrix = _matrixSaves.Pop();
        }

        public void DrawBitmap(ImageSource bitmap, Rect src, Rect dst, Paint paint)
        {
            // TODO paint.ColorFilter
            _matrix.MapRect(ref dst);

            var image = new Image
            {
                Width = src.Width,
                Height = src.Height,
                Stretch = Stretch.Fill,
                RenderTransform = GetCurrentRenderTransform(),
                Source = bitmap,
                Opacity = paint.Alpha / 255f
            };
            SetLeft(image, dst.X);
            SetTop(image, dst.Y);
            Children.Add(image);
        }

        public void GetClipBounds(out Rect originalClipRect)
        {
            RectExt.Set(ref originalClipRect, 0, 0, Width, Height);
        }

        public void Clear(Color color)
        {
            Children.Clear();
            _matrixSaves.Clear();
            Background = new SolidColorBrush(color);
        }

        public Viewbox GetImage()
        {
            return new Viewbox
            {
                Child = this,
                Stretch = Stretch.Uniform,
                StretchDirection = StretchDirection.DownOnly
            };
        }

        public void Translate(float dx, float dy)
        {
            _matrix = MatrixExt.PreTranslate(_matrix, dx, dy);
        }

        public void SetMatrix(DenseMatrix matrix)
        {
            _matrix = matrix;
        }

        public void DrawText(char character, Paint paint)
        {
            var gradient = paint.Shader as Gradient;
            var color = paint.Color;
            if (paint.ColorFilter != null)
                color = paint.ColorFilter.Apply(color);
            var brush = gradient != null ? gradient.GetBrush(paint.Alpha) : new SolidColorBrush(color);

            var textblock = new TextBlock
            {
                Text = new string(character, 1),
                RenderTransform = GetCurrentRenderTransform(),
                Foreground = brush,
                FontSize = paint.TextSize,
                FontFamily = paint.Typeface.FontFamily,
                FontStyle = paint.Typeface.Style,
                FontWeight = paint.Typeface.Weight
            };
            Children.Add(textblock);
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