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

        public virtual int Width { get; }

        public virtual int Height { get; }

        public virtual string Id { get; }

        public virtual string FileName { get; }

        public virtual string DirName { get; }
    }
}