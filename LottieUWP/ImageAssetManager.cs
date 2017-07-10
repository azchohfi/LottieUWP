using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Graphics.Canvas;

namespace LottieUWP
{
    internal class ImageAssetManager
    {
        private readonly string _imagesFolder;
        private IImageAssetDelegate _delegate;
        private readonly IDictionary<string, LottieImageAsset> _imageAssets;
        private readonly IDictionary<string, CanvasBitmap> _bitmaps = new Dictionary<string, CanvasBitmap>();

        internal ImageAssetManager(string imagesFolder, IImageAssetDelegate @delegate, IDictionary<string, LottieImageAsset> imageAssets)
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
            set => _delegate = value;
        }

        internal CanvasBitmap UpdateBitmap(string id, CanvasBitmap bitmap)
        {
            _bitmaps.Add(id, bitmap);
            return bitmap;
        }

        internal virtual CanvasBitmap BitmapForId(string id)
        {
            CanvasBitmap bitmap;
            if (!_bitmaps.TryGetValue(id, out bitmap))
            {
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

                Stream @is;
                try
                {
                    if (string.IsNullOrEmpty(_imagesFolder))
                    {
                        throw new InvalidOperationException("You must set an images folder before loading an image." + " Set it with LottieDrawable.ImageAssetsFolder");
                    }
                    @is = File.OpenRead(_imagesFolder + imageAsset.FileName);
                }
                catch (IOException e)
                {
                    Debug.WriteLine($"Unable to open asset. {e}", LottieLog.Tag);
                    return null;
                }
                var task = CanvasBitmap.LoadAsync(CanvasDevice.GetSharedDevice(), @is.AsRandomAccessStream(), 160).AsTask();
                task.Wait();
                bitmap = task.Result;

                @is.Dispose();

                _bitmaps[id] = bitmap;
            }
            return bitmap;
        }

        internal virtual void RecycleBitmaps()
        {
            var keyValuePairs = new HashSet<KeyValuePair<string, CanvasBitmap>>();
            foreach (var keyValuePair in _bitmaps)
            {
                keyValuePairs.Add(keyValuePair);
            }

            for (var i = keyValuePairs.Count - 1; i >= 0; i--)
            {
                var entry = keyValuePairs.ElementAt(i);
                entry.Value.Dispose();
                keyValuePairs.Remove(entry);
            }
        }
    }
}