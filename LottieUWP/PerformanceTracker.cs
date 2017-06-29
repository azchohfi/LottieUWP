using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LottieUWP
{
    public class PerformanceTracker
    {
        private bool _enabled;
        private readonly IDictionary<string, MeanCalculator> _layerRenderTimes = new Dictionary<string, MeanCalculator>();
        private readonly IComparer<Tuple<string, float?>> _floatComparator = new ComparatorAnonymousInnerClass();

        private class ComparatorAnonymousInnerClass : IComparer<Tuple<string, float?>>
        {
            public int Compare(Tuple<string, float?> o1, Tuple<string, float?> o2)
            {
                var r1 = o1.Item2;
                var r2 = o2.Item2;
                if (r2 > r1)
                {
                    return 1;
                }
                if (r1 > r2)
                {
                    return -1;
                }
                return 0;
            }
        }

        internal virtual bool Enabled
        {
            set => _enabled = value;
        }

        internal virtual void RecordRenderTime(string layerName, float millis)
        {
            if (!_enabled)
            {
                return;
            }
            var meanCalculator = _layerRenderTimes[layerName];
            if (meanCalculator == null)
            {
                meanCalculator = new MeanCalculator();
                _layerRenderTimes[layerName] = meanCalculator;
            }
            meanCalculator.Add(millis);
        }

        public virtual void ClearRenderTimes()
        {
            _layerRenderTimes.Clear();
        }

        public virtual void LogRenderTimes()
        {
            if (!_enabled)
            {
                return;
            }
            var sortedRenderTimes = SortedRenderTimes;
            Debug.WriteLine("Render times:", LottieLog.Tag);
            for (var i = 0; i < sortedRenderTimes.Count; i++)
            {
                var layer = sortedRenderTimes[i];
                Debug.WriteLine(string.Format("\t\t{0,30}:{1:F2}", layer.Item1, layer.Item2), LottieLog.Tag);
            }
        }

        public virtual IList<Tuple<string, float?>> SortedRenderTimes
        {
            get
            {
                if (!_enabled)
                {
                    return new List<Tuple<string, float?>>();
                }
                var sortedRenderTimes = new List<Tuple<string, float?>>(_layerRenderTimes.Count);
                foreach (var e in _layerRenderTimes.SetOfKeyValuePairs())
                {
                    sortedRenderTimes.Add(new Tuple<string, float?>(e.Key, e.Value.Mean));
                }
                sortedRenderTimes.Sort(_floatComparator);
                return sortedRenderTimes;
            }
        }
    }
}

internal static class HashMapHelperClass
{
    internal static HashSet<KeyValuePair<TKey, TValue>> SetOfKeyValuePairs<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
    {
        var entries = new HashSet<KeyValuePair<TKey, TValue>>();
        foreach (var keyValuePair in dictionary)
        {
            entries.Add(keyValuePair);
        }
        return entries;
    }
}
