using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.UI.Xaml.Media.Imaging;

namespace LottieUWP
{
    internal class ImageAssetBitmapManager
    {
        private readonly string _imagesFolder;
        private IImageAssetDelegate _assetDelegate;
        private readonly IDictionary<string, LottieImageAsset> _imageAssets;
        private readonly IDictionary<string, WriteableBitmap> _bitmaps = new Dictionary<string, WriteableBitmap>();

        internal ImageAssetBitmapManager(string imagesFolder, IImageAssetDelegate assetDelegate, IDictionary<string, LottieImageAsset> imageAssets)
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
            AssetDelegate = assetDelegate;
        }

        internal virtual IImageAssetDelegate AssetDelegate
        {
            set => _assetDelegate = value;
        }

        internal WriteableBitmap UpdateBitmap(string id, WriteableBitmap bitmap)
        {
            _bitmaps.Add(id, bitmap);
            return bitmap;
        }

        internal virtual WriteableBitmap BitmapForId(string id)
        {
            WriteableBitmap bitmap = _bitmaps[id];
            if (bitmap == null)
            {
                LottieImageAsset imageAsset = _imageAssets[id];
                if (imageAsset == null)
                {
                    return null;
                }
                if (_assetDelegate != null)
                {
                    bitmap = _assetDelegate.FetchBitmap(imageAsset);
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
                        throw new System.InvalidOperationException("You must set an images folder before loading an image." + " Set it with LottieComposition#setImagesFolder or LottieDrawable#setImagesFolder");
                    }
                    @is = File.OpenRead(_imagesFolder + imageAsset.FileName);
                }
                catch (IOException e)
                {
                    Debug.WriteLine($"Unable to open asset. {e}", L.Tag);
                    return null;
                }
                //BitmapFactory.Options opts = new BitmapFactory.Options(); // TODO: handle  density
                //opts.inScaled = true;
                //opts.inDensity = 160;
                bitmap = BitmapFactory.FromStream(@is).Result; // TODO: use await...

                _bitmaps[id] = bitmap;
            }
            return bitmap;
        }

        internal virtual void RecycleBitmaps()
        {
            var keyValuePairs = _bitmaps.SetOfKeyValuePairs();
            for (int i = keyValuePairs.Count - 1; i >= 0; i--)
            {
                KeyValuePair<string, WriteableBitmap> entry = keyValuePairs.ElementAt(i);
                //entry.Value.recycle(); // TODO: Urgent, dispose!
                keyValuePairs.Remove(entry);
            }
        }

        //internal virtual bool hasSameContext(Context context)
        //{
        //    return context == null && this.context == null || context != null && this.context.Equals(context);
        //}
    }
}