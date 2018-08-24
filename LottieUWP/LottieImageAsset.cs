using Microsoft.Graphics.Canvas;

namespace LottieUWP
{
    /// <summary>
    /// Data class describing an image asset exported by bodymovin.
    /// </summary>
    public class LottieImageAsset
    {
        internal LottieImageAsset(int width, int height, string id, string fileName, string dirName)
        {
            Width = width;
            Height = height;
            Id = id;
            FileName = fileName;
            DirName = dirName;
        }

        public int Width { get; }

        public int Height { get; }

        public string Id { get; }

        public string FileName { get; }

        public string DirName { get; }

        /** Pre-set a bitmap for this asset */
        // Returns the bitmap that has been stored for this image asset if one was explicitly set.
        public CanvasBitmap Bitmap { get; set; }
    }
}