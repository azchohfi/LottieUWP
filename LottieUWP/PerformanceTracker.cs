using System;
using System.Collections.Generic;
using System.Diagnostics;
using LottieUWP.Utils;

namespace LottieUWP
{
    public class PerformanceTracker
    {
        public class FrameRenderedEventArgs : EventArgs
        {
            public FrameRenderedEventArgs(float renderTimeMs)
            {
                RenderTimeMs = renderTimeMs;
            }

            public float RenderTimeMs { get; }
        }

        private bool _enabled;
        public event EventHandler<FrameRenderedEventArgs> FrameRendered;
        private readonly Dictionary<string, MeanCalculator> _layerRenderTimes = new Dictionary<string, MeanCalculator>();
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

        public bool Enabled
        {
            set => _enabled = value;
        }

        public void RecordRenderTime(string layerName, float millis)
        {
            if (!_enabled)
            {
                return;
            }
            if (!_layerRenderTimes.TryGetValue(layerName, out var meanCalculator))
            {
                meanCalculator = new MeanCalculator();
                _layerRenderTimes[layerName] = meanCalculator;
            }
            meanCalculator.Add(millis);
            if (layerName.Equals("__container"))
            {
                OnFrameRendered(new FrameRenderedEventArgs(millis));
            }
        }

        public void ClearRenderTimes()
        {
            _layerRenderTimes.Clear();
        }

        public void LogRenderTimes()
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

        public List<Tuple<string, float?>> SortedRenderTimes
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

        protected void OnFrameRendered(FrameRenderedEventArgs e)
        {
            FrameRendered?.Invoke(this, e);
        }
    }
}