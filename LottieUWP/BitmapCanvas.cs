using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using MathNet.Numerics.LinearAlgebra.Single;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Composition;

namespace LottieUWP
{
    public class BitmapCanvas : Canvas
    {
        private DenseMatrix _matrix = DenseMatrix.CreateIdentity(3);
        private readonly Stack<DenseMatrix> _matrixSaves = new Stack<DenseMatrix>();
        private readonly Stack<int> _flagSaves = new Stack<int>();

        class ClipSave
        {
            public ClipSave(Rect rect, CanvasActiveLayer layer)
            {
                Rect = rect;
                Layer = layer;
            }

            public Rect Rect { get; }
            public CanvasActiveLayer Layer { get; }
        }

        private readonly Stack<ClipSave> _clipSaves = new Stack<ClipSave>();
        private Rect _currentClip;

        private ContainerVisual GetVisual()
        {
            var hostVisual = ElementCompositionPreview.GetElementVisual(this);
            var root = hostVisual.Compositor.CreateContainerVisual();
            ElementCompositionPreview.SetElementChildVisual(this, root);
            return root;
        }

        public BitmapCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            _currentClip = new Rect(0, 0, Width, Height);
            Clip = new RectangleGeometry
            {
                Rect = _currentClip
            };
            
            _device = CanvasDevice.GetSharedDevice();

            var root = GetVisual();

            _compositor = root.Compositor;

            var compositionGraphicsDevice = CanvasComposition.CreateCompositionGraphicsDevice(_compositor, _device);

            var drawingSurfaceVisual = _compositor.CreateSpriteVisual();

            _drawingSurface = compositionGraphicsDevice.CreateDrawingSurface(new Size(Width, Height), DirectXPixelFormat.B8G8R8A8UIntNormalized, DirectXAlphaMode.Premultiplied);
            drawingSurfaceVisual.Brush = _compositor.CreateSurfaceBrush(_drawingSurface);

            root.Children.InsertAtTop(drawingSurfaceVisual);

            drawingSurfaceVisual.Size = _drawingSurface.Size.ToVector2();

            Clear(Colors.Transparent);
        }

        public static int MatrixSaveFlag = 0b00001;
        public static int ClipSaveFlag = 0b00010;
        //public static int HasAlphaLayerSaveFlag = 0b00100;
        //public static int FullColorLayerSaveFlag = 0b01000;
        public static int ClipToLayerSaveFlag = 0b10000;
        public static int AllSaveFlag = 0b11111;

        private readonly CanvasDevice _device;

        private readonly CompositionDrawingSurface _drawingSurface;
        private readonly Compositor _compositor;
        private CanvasDrawingSession _drawingSession;

        public void DrawRect(double x1, double y1, double x2, double y2, Paint paint)
        {
            UpdateDrawingSessionWithFlags(paint.Flags);
            _drawingSession.Transform = GetCurrentTransform();
            var brush = new CanvasSolidColorBrush(_device, paint.Color);

            if (paint.Style == Paint.PaintStyle.Stroke)
            {
                _drawingSession.DrawRectangle((float)x1, (float)y1, (float)(x2 - x1), (float)(y2 - y1), brush, paint.StrokeWidth, GetCanvasStrokeStyle(paint));
            }
            else
            {
                _drawingSession.FillRectangle((float)x1, (float)y1, (float)(x2 - x1), (float)(y2 - y1), brush);
            }

            Flush();
        }

        private static CanvasStrokeStyle GetCanvasStrokeStyle(Paint paint)
        {
            var style = new CanvasStrokeStyle
            {
                StartCap = paint.StrokeCap,
                DashCap = paint.StrokeCap,
                EndCap = paint.StrokeCap,
                LineJoin = paint.StrokeJoin,
            };
            paint.PathEffect?.Apply(style, paint);
            return style;
        }

        internal void DrawRect(Rect rect, Paint paint)
        {
            UpdateDrawingSessionWithFlags(paint.Flags);
            _drawingSession.Transform = GetCurrentTransform();
            var brush = new CanvasSolidColorBrush(_device, paint.Color);

            if (paint.Style == Paint.PaintStyle.Stroke)
            {
                _drawingSession.DrawRectangle(rect, brush, paint.StrokeWidth, GetCanvasStrokeStyle(paint));
            }
            else
            {
                _drawingSession.FillRectangle(rect, brush);
            }

            Flush();
        }

        public void DrawPath(Path path, Paint paint)
        {
            UpdateDrawingSessionWithFlags(paint.Flags);

            _drawingSession.Transform = GetCurrentTransform();

            var fill = path.FillType == PathFillType.Winding
                ? CanvasFilledRegionDetermination.Winding
                : CanvasFilledRegionDetermination.Alternate;
            //    FillRule = path.FillType == PathFillType.EvenOdd ? FillRule.EvenOdd : FillRule.Nonzero,

            var style = GetCanvasStrokeStyle(paint);

            var gradient = paint.Shader as Gradient;
            var brush = gradient != null ? gradient.GetBrush(paint.Alpha) : new CanvasSolidColorBrush(_device, paint.Color);
            brush = paint.ColorFilter?.Apply(this, brush) ?? brush;

            var canvasGeometries = new List<CanvasGeometry>();
            var canvasPathBuilder = new CanvasPathBuilder(_device);

            var returnDecision = Path.DrawReturnType.JustDraw;

            var closed = true;

            for (var i = 0; i < path.Contours.Count; i++)
            {
                if (returnDecision == Path.DrawReturnType.NewPath)
                {
                    canvasGeometries.Add(CanvasGeometry.CreatePath(canvasPathBuilder));

                    DrawFigure(CanvasGeometry.CreateGroup(_device, canvasGeometries.ToArray(), fill), paint, brush, style);

                    canvasGeometries.Clear();

                    canvasPathBuilder = new CanvasPathBuilder(_device);
                }

                returnDecision = path.Contours[i].AddPathSegment(canvasPathBuilder, ref closed);
            }

            if (!closed)
            {
                canvasPathBuilder.EndFigure(CanvasFigureLoop.Open);
            }
            canvasGeometries.Add(CanvasGeometry.CreatePath(canvasPathBuilder));

            DrawFigure(CanvasGeometry.CreateGroup(_device, canvasGeometries.ToArray(), fill), paint, brush, style);

            Flush();
        }

        private void DrawFigure(CanvasGeometry group, Paint paint, ICanvasBrush brush, CanvasStrokeStyle style)
        {
            if (paint.Style == Paint.PaintStyle.Stroke)
                _drawingSession.DrawGeometry(group, brush, paint.StrokeWidth, style);
            else
                _drawingSession.FillGeometry(group, brush);
        }

        private Matrix3x2 GetCurrentTransform()
        {
            return new Matrix3x2
            {
                M11 = _matrix[0, 0],
                M12 = _matrix[1, 0],
                M21 = _matrix[0, 1],
                M22 = _matrix[1, 1],
                M31 = _matrix[0, 2],
                M32 = _matrix[1, 2]
            };
        }

        public bool ClipRect(Rect rect)
        {
            _currentClip.Intersect(rect);
            return _currentClip.IsEmpty == false;
        }

        public void ClipReplaceRect(Rect rect)
        {
            _currentClip = rect;
        }

        public void Concat(DenseMatrix parentMatrix)
        {
            _matrix = MatrixExt.PreConcat(_matrix, parentMatrix);
        }

        public void Save()
        {
            _flagSaves.Push(MatrixSaveFlag | ClipSaveFlag);
            SaveMatrix();
            SaveClip(255);
        }

        public void SaveLayer(Rect bounds, Paint paint, int flags)
        {
            _flagSaves.Push(flags);
            if ((flags & MatrixSaveFlag) == MatrixSaveFlag)
            {
                SaveMatrix();
            }
            if ((flags & ClipSaveFlag) == ClipSaveFlag)
            {
                SaveClip(paint.Alpha);
            }
            if ((flags & ClipToLayerSaveFlag) == ClipToLayerSaveFlag)
            {
                UpdateDrawingSessionWithFlags(paint.Flags);

                //Attributes of the Paint - alpha, Xfermode are applied when the offscreen rendering target is drawn back when restore() is called.
                if (paint.Xfermode != null)
                {
                    var compositeEffect = new CompositeEffect
                    {
                        Mode = PorterDuff.ToCanvasComposite(paint.Xfermode.Mode)
                    };
                    compositeEffect.Sources.Add(new CompositionEffectSourceParameter("source1"));
                    compositeEffect.Sources.Add(new CompositionEffectSourceParameter("source2"));

                    var compositeEffectFactory = _compositor.CreateEffectFactory(compositeEffect);
                    var compositionBrush = compositeEffectFactory.CreateBrush();

                    compositionBrush.SetSourceParameter("source1", _compositor.CreateSurfaceBrush());
                    compositionBrush.SetSourceParameter("source2", _compositor.CreateSurfaceBrush());
                }

                // TODO create other drawing surface with *bounds* size, and start drawing in it
            }
        }

        private void SaveMatrix()
        {
            var copy = new DenseMatrix(3);
            _matrix.CopyTo(copy);
            _matrixSaves.Push(copy);
        }

        private void SaveClip(byte alpha)
        {
            var currentLayer = _drawingSession.CreateLayer(alpha / 255f, _currentClip);

            _clipSaves.Push(new ClipSave(_currentClip, currentLayer));
        }

        public void Restore()
        {
            var flags = _flagSaves.Pop();
            if ((flags & MatrixSaveFlag) == MatrixSaveFlag)
            {
                _matrix = _matrixSaves.Pop();
            }
            if ((flags & ClipSaveFlag) == ClipSaveFlag)
            {
                var clipSave = _clipSaves.Pop();
                _currentClip = clipSave.Rect;
                clipSave.Layer.Dispose();
            }
            if ((flags & ClipToLayerSaveFlag) == ClipToLayerSaveFlag)
            {
                // TODO draw current offlayer 
            }
        }

        public void DrawBitmap(CanvasBitmap bitmap, Rect src, Rect dst, Paint paint)
        {
            UpdateDrawingSessionWithFlags(paint.Flags);
            _drawingSession.Transform = GetCurrentTransform();

            var canvasComposite = CanvasComposite.SourceOver;
            // TODO paint.ColorFilter
            //if (paint.ColorFilter is PorterDuffColorFilter porterDuffColorFilter)
            //    canvasComposite = PorterDuff.ToCanvasComposite(porterDuffColorFilter.Mode);

            _drawingSession.DrawImage(bitmap, dst, src, paint.Alpha / 255f, CanvasImageInterpolation.NearestNeighbor, canvasComposite);

            Flush();
        }

        public void GetClipBounds(out Rect bounds)
        {
            RectExt.Set(ref bounds, _currentClip.X, _currentClip.Y, _currentClip.Width, _currentClip.Height);
        }

        public void Clear(Color color)
        {
            _drawingSession?.Dispose();
            _drawingSession = CanvasComposition.CreateDrawingSession(_drawingSurface);
            UpdateDrawingSessionWithFlags(0);

            _drawingSession.Clear(color);

            _matrixSaves.Clear();
            _flagSaves.Clear();
            _clipSaves.Clear();
        }

        private void UpdateDrawingSessionWithFlags(int flags)
        {
            Flush();

            _drawingSession.Antialiasing = (flags & Paint.AntiAliasFlag) == Paint.AntiAliasFlag
                ? CanvasAntialiasing.Antialiased
                : CanvasAntialiasing.Aliased;
        }

        public void Flush()
        {
            _drawingSession.Flush();
        }

        public UIElement GetImage()
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
            matrix.CopyTo(_matrix);
        }

        public Rect DrawText(char character, Paint paint)
        {
            var gradient = paint.Shader as Gradient;
            var brush = gradient != null ? gradient.GetBrush(paint.Alpha) : new CanvasSolidColorBrush(_device, paint.Color);
            brush = paint.ColorFilter?.Apply(this, brush) ?? brush;

            UpdateDrawingSessionWithFlags(paint.Flags);
            _drawingSession.Transform = GetCurrentTransform();

            var text = new string(character, 1);

            var textFormat = new CanvasTextFormat
            {
                FontSize = paint.TextSize,
                FontFamily = paint.Typeface.FontFamily,
                FontStyle = paint.Typeface.Style,
                FontWeight = paint.Typeface.Weight,
                VerticalAlignment = CanvasVerticalAlignment.Center
            };
            var textLayout = new CanvasTextLayout(_drawingSession, text, textFormat, 0.0f, 0.0f);
            _drawingSession.DrawText(text, 0, 0, brush, textFormat);

            Flush();

            return textLayout.LayoutBounds;
        }
    }
}