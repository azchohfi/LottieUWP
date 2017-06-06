using Windows.Data.Json;

namespace LottieUWP
{
    /// <summary>
    /// Data class describing an image asset exported by bodymovin.
    /// </summary>
    public class LottieImageAsset
    {
        private LottieImageAsset(int width, int height, string id, string fileName)
        {
            Width = width;
            Height = height;
            Id = id;
            FileName = fileName;
        }

        internal static class Factory
        {
            internal static LottieImageAsset NewInstance(JsonObject imageJson)
            {
                return new LottieImageAsset((int)imageJson.GetNamedNumber("w"), (int)imageJson.GetNamedNumber("h"), imageJson.GetNamedString("id"), imageJson.GetNamedString("p"));
            }
        }

        public virtual int Width { get; }

        public virtual int Height { get; }

        public virtual string Id { get; }

        public virtual string FileName { get; }
    }
}