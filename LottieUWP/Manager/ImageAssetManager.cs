using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace LottieUWP.Manager
{
    internal class ImageAssetManager : IDisposable
    {
        private readonly string _imagesFolder;
        private IImageAssetDelegate _delegate;
        private readonly Dictionary<string, LottieImageAsset> _imageAssets;
        private readonly CanvasDevice _context;

        internal ImageAssetManager(string imagesFolder, IImageAssetDelegate @delegate, Dictionary<string, LottieImageAsset> imageAssets, CanvasDevice context)
        {
            _imagesFolder = imagesFolder;
            _context = context;
            if (!string.IsNullOrEmpty(imagesFolder) && _imagesFolder[_imagesFolder.Length - 1] != '/')
            {
                _imagesFolder += '/';
            }

            //if (!(callback is UIElement)) // TODO: Makes sense on UWP?
            //{
            //    Debug.WriteLine("LottieDrawable must be inside of a view for images to work.", L.TAG);
            //    this.imageAssets = new Dictionary<string, LottieImageAsset>();
            //    return;
            //}

            _imageAssets = imageAssets;
            Delegate = @delegate;
        }

        internal IImageAssetDelegate Delegate
        {
            set
            {
                lock (this)
                {
                    _delegate = value;
                }
            }
        }

        /// <summary>
        /// Returns the previously set bitmap or null.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        internal CanvasBitmap UpdateBitmap(string id, CanvasBitmap bitmap)
        {
            lock (this)
            {
                if (bitmap == null)
                {
                    if (_imageAssets.TryGetValue(id, out var asset))
                    {
                        var ret = asset.Bitmap;
                        asset.Bitmap = null;
                        return ret;
                    }
                    return null;
                }

                CanvasBitmap prevBitmap = null;
                if (_imageAssets.TryGetValue(id, out var prevAsset))
                {
                    prevBitmap = prevAsset.Bitmap;
                }
                PutBitmap(id, bitmap);
                return prevBitmap;
            }
        }

        internal CanvasBitmap BitmapForId(CanvasDevice device, string id)
        {
            lock (this)
            {
                if (!_imageAssets.TryGetValue(id, out var asset))
                {
                    return null;
                }
                var bitmap = asset.Bitmap;
                if (bitmap != null)
                {
                    return bitmap;
                }

                if (_delegate != null)
                {
                    bitmap = _delegate.FetchBitmap(asset);
                    if (bitmap != null)
                    {
                        PutBitmap(id, bitmap);
                    }
                    return bitmap;
                }

                var filename = asset.FileName;
                Task<CanvasBitmap> task = null;
                Stream @is;

                if (filename.StartsWith("data:") && filename.IndexOf("base64,") > 0)
                {
                    // Contents look like a base64 data URI, with the format data:image/png;base64,<data>.
                    byte[] data;
                    try
                    {
                        data = Convert.FromBase64String(filename.Substring(filename.IndexOf(',') + 1));
                        @is = new MemoryStream(data);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"data URL did not have correct base64 format. {e}", LottieLog.Tag);
                        return null;
                    }
                    task = CanvasBitmap.LoadAsync(device, @is.AsRandomAccessStream(), 160).AsTask();
                    task.Wait();
                    bitmap = task.Result;

                    @is.Dispose();

                    PutBitmap(id, bitmap);
                    return bitmap;
                }

                try
                {
                    if (string.IsNullOrEmpty(_imagesFolder))
                    {
                        throw new InvalidOperationException("You must set an images folder before loading an image. Set it with LottieDrawable.ImageAssetsFolder");
                    }
                    @is = File.OpenRead(_imagesFolder + asset.FileName);
                }
                catch (IOException e)
                {
                    Debug.WriteLine($"Unable to open asset. {e}", LottieLog.Tag);
                    return null;
                }
                task = CanvasBitmap.LoadAsync(device, @is.AsRandomAccessStream(), 160).AsTask();
                task.Wait();
                bitmap = task.Result;

                @is.Dispose();

                PutBitmap(id, bitmap);

                return bitmap;
            }
        }

        internal void RecycleBitmaps()
        {
            lock (this)
            {
                foreach (var entry in _imageAssets)
                {
                    var asset = entry.Value;
                    var bitmap = asset.Bitmap;
                    if (bitmap != null)
                    {
                        bitmap.Dispose();
                        asset.Bitmap = null;
                    }
                }
            }
        }

        public bool HasSameContext(CanvasDevice context)
        {
            return context == null && _context == null || _context.Equals(context);
        }

        private CanvasBitmap PutBitmap(string key, CanvasBitmap bitmap)
        {
            lock (this)
            {
                _imageAssets[key].Bitmap = bitmap;
                return bitmap;
            }
        }

        private void Dispose(bool disposing)
        {
            RecycleBitmaps();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ImageAssetManager()
        {
            Dispose(false);
        }
    }
}