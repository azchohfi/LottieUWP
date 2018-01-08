using System.Collections.Generic;

namespace LottieUWP
{
    internal static class HashMapHelperClass
    {
        internal static HashSet<KeyValuePair<TKey, TValue>> SetOfKeyValuePairs<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            var entries = new HashSet<KeyValuePair<TKey, TValue>>();
            foreach (var keyValuePair in dictionary)
            {
                entries.Add(keyValuePair);
            }
            return entries;
        }
    }
}