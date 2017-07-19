using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;

namespace LottieUWP
{
    public class BitmapCanvas
    {
        private Matrix3X3 _matrix = Matrix3X3.CreateIdentity();
        private readonly Stack<Matrix3X3> _matrixSaves = new Stack<Matrix3X3>();
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

        public BitmapCanvas(double width, double height)
        {
            Width = width;
            Height = height;
            _currentClip = new Rect(0, 0, Width, Height);
        }

        public double Width { get; }
        public double Height { get; }

        public static int MatrixSaveFlag = 0b00001;
        public static int ClipSaveFlag = 0b00010;
        //public static int HasAlphaLayerSaveFlag = 0b00100;
        //public static int FullColorLayerSaveFlag = 0b01000;
        public static int ClipToLayerSaveFlag = 0b10000;
        public static int AllSaveFlag = 0b11111;

        private CanvasDevice _device;

        private CanvasDrawingSession _drawingSession;

        internal CanvasActiveLayer CreateSession(CanvasDevice device, CanvasDrawingSession drawingSession)
        {
            _device = device;
            _drawingSession = drawingSession;

            return _drawingSession.CreateLayer(1f, CanvasGeometry.CreateRectangle(_device, _currentClip));
        }

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

            _drawingSession.Flush();
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

            _drawingSession.Flush();
        }

        public void DrawPath(Path path, Paint paint)
        {
            UpdateDrawingSessionWithFlags(paint.Flags);

            _drawingSession.Transform = GetCurrentTransform();

            var geometry = path.GetGeometry(_device);

            var gradient = paint.Shader as Gradient;
            var brush = gradient != null ? gradient.GetBrush(_device, paint.Alpha) : new CanvasSolidColorBrush(_device, paint.Color);
            brush = paint.ColorFilter?.Apply(this, brush) ?? brush;

            if (paint.Style == Paint.PaintStyle.Stroke)
                _drawingSession.DrawGeometry(geometry, brush, paint.StrokeWidth, GetCanvasStrokeStyle(paint));
            else
                _drawingSession.FillGeometry(geometry, brush);

            _drawingSession.Flush();
        }

        private Matrix3x2 GetCurrentTransform()
        {
            return new Matrix3x2
            {
                M11 = _matrix.M11,
                M12 = _matrix.M12,
                M21 = _matrix.M21,
                M22 = _matrix.M22,
                M31 = _matrix.M31,
                M32 = _matrix.M32
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

        public void Concat(Matrix3X3 parentMatrix)
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
                    //var compositeEffect = new CompositeEffect
                    //{
                    //    Mode = PorterDuff.ToCanvasComposite(paint.Xfermode.Mode)
                    //};
                    //compositeEffect.Sources.Add(new CompositionEffectSourceParameter("source1"));
                    //compositeEffect.Sources.Add(new CompositionEffectSourceParameter("source2"));
                    //
                    //var compositeEffectFactory = _compositor.CreateEffectFactory(compositeEffect);
                    //var compositionBrush = compositeEffectFactory.CreateBrush();
                    //
                    //compositionBrush.SetSourceParameter("source1", _compositor.CreateSurfaceBrush());
                    //compositionBrush.SetSourceParameter("source2", _compositor.CreateSurfaceBrush());
                }

                // TODO create other drawing surface with *bounds* size, and start drawing in it
            }
        }

        private void SaveMatrix()
        {
            var copy = new Matrix3X3();
            copy.Set(_matrix);
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

            _drawingSession.Flush();
        }

        public void GetClipBounds(out Rect bounds)
        {
            RectExt.Set(ref bounds, _currentClip.X, _currentClip.Y, _currentClip.Width, _currentClip.Height);
        }

        public void Clear(Color color)
        {
            UpdateDrawingSessionWithFlags(0);

            _drawingSession.Clear(color);

            _matrixSaves.Clear();
            _flagSaves.Clear();
            _clipSaves.Clear();
        }

        private void UpdateDrawingSessionWithFlags(int flags)
        {
            _drawingSession.Flush();

            _drawingSession.Antialiasing = (flags & Paint.AntiAliasFlag) == Paint.AntiAliasFlag
                ? CanvasAntialiasing.Antialiased
                : CanvasAntialiasing.Aliased;
        }

        public void Translate(float dx, float dy)
        {
            _matrix = MatrixExt.PreTranslate(_matrix, dx, dy);
        }

        public void SetMatrix(Matrix3X3 matrix)
        {
            _matrix.Set(matrix);
        }

        public Rect DrawText(char character, Paint paint)
        {
            var gradient = paint.Shader as Gradient;
            var brush = gradient != null ? gradient.GetBrush(_device, paint.Alpha) : new CanvasSolidColorBrush(_device, paint.Color);
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

            _drawingSession.Flush();

            return textLayout.LayoutBounds;
        }
    }
}