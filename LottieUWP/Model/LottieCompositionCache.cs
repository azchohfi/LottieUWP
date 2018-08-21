namespace LottieUWP.Model
{
    internal class LottieCompositionCache
    {
        //private static readonly int _cacheSizeMB = 10;
        private static readonly int _cacheSizeCount = 10;

        public static LottieCompositionCache Instance { get; } = new LottieCompositionCache();

        private readonly LruCache<string, LottieComposition> _cache = new LruCache<string, LottieComposition>(_cacheSizeCount);//1024 * 1024 * _cacheSizeMB);

        internal LottieCompositionCache()
        {
        }

        public LottieComposition GetRawRes(int rawRes)
        {
            return Get(rawRes.ToString());
        }

        public LottieComposition Get(string assetName)
        {
            return _cache.Get(assetName);
        }

        public void Put(int rawRes, LottieComposition composition)
        {
            Put(rawRes.ToString(), composition);
        }

        public void Put(string cacheKey, LottieComposition composition)
        {
            if (cacheKey == null)
            {
                return;
            }
            _cache.Put(cacheKey, composition);
        }
    }
}
