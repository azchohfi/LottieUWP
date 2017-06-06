using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace LottieUWP
{
    /// <summary>
    /// Can be used for debugging. When needed, you can acquire and
    /// draw to this bitmap layer when rendering to an offscreen
    /// buffer to view the contents.
    /// </summary>
    internal class CanvasPool
    {
        internal static readonly CanvasPool Instance = new CanvasPool();

        private CanvasPool()
        {
        }

        private readonly Dictionary<long, IList<WriteableBitmap>> _availableBitmaps = new Dictionary<long, IList<WriteableBitmap>>();
        private readonly Dictionary<BitmapCanvas, WriteableBitmap> _canvasBitmapMap = new Dictionary<BitmapCanvas, WriteableBitmap>();
        private readonly Dictionary<WriteableBitmap, BitmapCanvas> _bitmapCanvasMap = new Dictionary<WriteableBitmap, BitmapCanvas>();

        internal virtual BitmapCanvas Acquire(int width, int height)
        {
            var key = GetKey(width, height);
            _availableBitmaps.TryGetValue(key, out IList<WriteableBitmap> bitmaps);
            if (bitmaps == null)
            {
                bitmaps = new List<WriteableBitmap>();
                _availableBitmaps.Add(key, bitmaps);
            }

            BitmapCanvas canvas;
            if (bitmaps.Count == 0)
            {
                var bitmap = new WriteableBitmap(width, height); //config
                canvas = new BitmapCanvas(bitmap);
                _canvasBitmapMap[canvas] = bitmap;
                _bitmapCanvasMap[bitmap] = canvas;
            }
            else
            {
                var bitmap = bitmaps[0];
                bitmaps.RemoveAt(0);
                canvas = _bitmapCanvasMap[bitmap];
                if (canvas == null)
                    throw new Exception("Canvas should not be null!");
            }
            canvas.Bitmap.Clear(Colors.Transparent);
            return canvas;
        }

        internal virtual void Release(BitmapCanvas canvas)
        {
            if (canvas == null)
                throw new Exception("Canvas should not be null!");
            var bitmap = _canvasBitmapMap[canvas];
            if (bitmap == null)
                throw new Exception("Bitmap should not be null!");
            var key = GetKey(bitmap);
            _availableBitmaps.TryGetValue(key, out IList<WriteableBitmap> bitmaps);
            if (bitmaps == null)
                throw new Exception("Bitmaps should not be null!");
            if (bitmaps.Contains(bitmap))
            {
                throw new InvalidOperationException("Canvas already released.");
            }
            bitmaps.Add(bitmap);
        }

        internal virtual void RecycleBitmaps()
        {
            for (var i = 0; i < _availableBitmaps.Count; i++)
            {
                var bitmaps = _availableBitmaps[i];

                for (var j = bitmaps.Count - 1; j >= 0; j--)
                {
                    var bitmap = bitmaps[j];
                    var canvas = _bitmapCanvasMap[bitmap];
                    _bitmapCanvasMap.Remove(bitmap);
                    _canvasBitmapMap.Remove(canvas);
                    //bitmap.recycle(); // TODO: Urgent, dispose!
                    bitmaps.RemoveAt(j);
                }
            }
            if (_bitmapCanvasMap.Count > 0)
            {
                throw new InvalidOperationException("Not all canvases have been released!");
            }
        }

        private int GetKey(WriteableBitmap bitmap)
        {
            return GetKey(bitmap.PixelWidth, bitmap.PixelHeight);
        }

        private int GetKey(int width, int height)
        {
            return (width & 0xffff) << 17 | (height & 0xffff) << 1; // | (config.ordinal() & 0x1);
        }
    }
}