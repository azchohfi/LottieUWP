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

        internal virtual BitmapCanvas Acquire(int width, int height)//, WriteableBitmap.Config config)
        {
            int key = GetKey(width, height);//, config);
            IList<WriteableBitmap> bitmaps;
            _availableBitmaps.TryGetValue(key, out bitmaps);
            if (bitmaps == null)
            {
                bitmaps = new List<WriteableBitmap>();
                _availableBitmaps.Add(key, bitmaps);
            }

            BitmapCanvas canvas;
            if (bitmaps.Count == 0)
            {
                WriteableBitmap bitmap = new WriteableBitmap(width, height); //config
                canvas = new BitmapCanvas(bitmap);
                _canvasBitmapMap[canvas] = bitmap;
                _bitmapCanvasMap[bitmap] = canvas;
            }
            else
            {
                WriteableBitmap bitmap = bitmaps[0];
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
            WriteableBitmap bitmap = _canvasBitmapMap[canvas];
            if (bitmap == null)
                throw new Exception("Bitmap should not be null!");
            int key = GetKey(bitmap);
            IList<WriteableBitmap> bitmaps;
            _availableBitmaps.TryGetValue(key, out bitmaps);
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
            for (int i = 0; i < _availableBitmaps.Count; i++)
            {
                IList<WriteableBitmap> bitmaps = _availableBitmaps[i];

                for (int j = bitmaps.Count - 1; j >= 0; j--)
                {
                    WriteableBitmap bitmap = bitmaps[j];
                    BitmapCanvas canvas = _bitmapCanvasMap[bitmap];
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
            return GetKey(bitmap.PixelWidth, bitmap.PixelHeight); //, bitmap.Config);
        }

        private int GetKey(int width, int height)//, WriteableBitmap.Config config)
        {
            return (width & 0xffff) << 17 | (height & 0xffff) << 1; // | (config.ordinal() & 0x1);
        }
    }
}