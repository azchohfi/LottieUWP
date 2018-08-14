using System;
using System.Collections.Generic;

namespace LottieUWP.Model
{
    internal class LottieCompositionCache
    {
        public static LottieCompositionCache Instance { get; } = new LottieCompositionCache();

        private readonly Dictionary<string, LottieComposition> _strongRefCache = new Dictionary<string, LottieComposition>();
        private readonly Dictionary<string, WeakReference<LottieComposition>> _weakRefCache = new Dictionary<string, WeakReference<LottieComposition>>();

        internal LottieCompositionCache()
        {
        }

        public LottieComposition GetRawRes(int rawRes)
        {
            return Get(rawRes.ToString());
        }

        public LottieComposition Get(string assetName)
        {
            if (_strongRefCache.ContainsKey(assetName))
            {
                return _strongRefCache[assetName];
            }
            else if (_weakRefCache.ContainsKey(assetName))
            {
                WeakReference<LottieComposition> compRef = _weakRefCache[assetName];
                compRef.TryGetTarget(out var target);
                return target;
            }
            return null;
        }

        public void Put(int rawRes, LottieComposition composition, LottieAnimationView.CacheStrategy cacheStrategy)
        {
            Put(rawRes.ToString(), composition, cacheStrategy);
        }

        public void Put(string cacheKey, LottieComposition composition, LottieAnimationView.CacheStrategy cacheStrategy)
        {
            if (cacheStrategy == LottieAnimationView.CacheStrategy.Strong)
            {
                _strongRefCache[cacheKey] = composition;
            }
            else if (cacheStrategy == LottieAnimationView.CacheStrategy.Weak)
            {
                _weakRefCache[cacheKey] = new WeakReference<LottieComposition>(composition);
            }
        }
    }
}
