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
        private readonly Dictionary<string, CanvasBitmap> _bitmaps = new Dictionary<string, CanvasBitmap>();

        internal ImageAssetManager(string imagesFolder, IImageAssetDelegate @delegate, Dictionary<string, LottieImageAsset> imageAssets)
        {
            _imagesFolder = imagesFolder;
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

        internal virtual IImageAssetDelegate Delegate
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
                    if (_bitmaps.TryGetValue(id, out var removed))
                        _bitmaps.Remove(id);
                    return removed;
                }
                _bitmaps[id] = bitmap;
                return bitmap;
            }
        }

        internal virtual CanvasBitmap BitmapForId(CanvasDevice device, string id)
        {
            lock (this)
            {
                if (_bitmaps.TryGetValue(id, out CanvasBitmap bitmap))
                {
                    return bitmap;
                }

                var imageAsset = _imageAssets[id];
                if (imageAsset == null)
                {
                    return null;
                }
                if (_delegate != null)
                {
                    bitmap = _delegate.FetchBitmap(imageAsset);
                    if (bitmap != null)
                    {
                        _bitmaps[id] = bitmap;
                    }
                    return bitmap;
                }

                var filename = imageAsset.FileName;
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

                    _bitmaps[id] = bitmap;
                    return bitmap;
                }

                try
                {
                    if (string.IsNullOrEmpty(_imagesFolder))
                    {
                        throw new InvalidOperationException("You must set an images folder before loading an image. Set it with LottieDrawable.ImageAssetsFolder");
                    }
                    @is = File.OpenRead(_imagesFolder + imageAsset.FileName);
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

                _bitmaps[id] = bitmap;
                
                return bitmap;
            }
        }

        internal virtual void RecycleBitmaps()
        {
            lock (this)
            {
                for (var i = _bitmaps.Count - 1; i >= 0; i--)
                {
                    var entry = _bitmaps.ElementAt(i);
                    entry.Value.Dispose();
                    _bitmaps.Remove(entry.Key);
                }
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