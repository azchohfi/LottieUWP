using System.Linq;
using Xunit;

namespace LottieUWP.Tests
{
    public class PerformanceTrackerTest
    {
        private readonly PerformanceTracker _performanceTracker;

        public PerformanceTrackerTest()
        {
            _performanceTracker = new PerformanceTracker
            {
                Enabled = true
            };
        }

        [Fact]
        public void TestDisabled()
        {
            _performanceTracker.Enabled = false;
            _performanceTracker.RecordRenderTime("Hello", 16f);
            Assert.False(_performanceTracker.SortedRenderTimes.Any());
        }

        [Fact]
        public void TestOneFrame()
        {
            _performanceTracker.RecordRenderTime("Hello", 16f);
            var sortedRenderTimes = _performanceTracker.SortedRenderTimes;
            Assert.Single(sortedRenderTimes);
            Assert.Equal("Hello", sortedRenderTimes[0].Item1);
            Assert.Equal(16f, sortedRenderTimes[0].Item2);
        }

        [Fact]
        public void TestTwoFrames()
        {
            _performanceTracker.RecordRenderTime("Hello", 16f);
            _performanceTracker.RecordRenderTime("Hello", 8f);
            var sortedRenderTimes = _performanceTracker.SortedRenderTimes;
            Assert.Single(sortedRenderTimes);
            Assert.Equal("Hello", sortedRenderTimes[0].Item1);
            Assert.Equal(12f, sortedRenderTimes[0].Item2);
        }

        [Fact]
        public void TestTwoLayers()
        {
            _performanceTracker.RecordRenderTime("Hello", 16f);
            _performanceTracker.RecordRenderTime("World", 8f);
            var sortedRenderTimes = _performanceTracker.SortedRenderTimes;
            Assert.Equal(2, sortedRenderTimes.Count);
            Assert.Equal("Hello", sortedRenderTimes[0].Item1);
            Assert.Equal(16f, sortedRenderTimes[0].Item2);
            Assert.Equal("World", sortedRenderTimes[1].Item1);
            Assert.Equal(8f, sortedRenderTimes[1].Item2);
        }

        [Fact]
        public void TestTwoLayersAlternatingFrames()
        {
            _performanceTracker.RecordRenderTime("Hello", 16f);
            _performanceTracker.RecordRenderTime("World", 8f);
            _performanceTracker.RecordRenderTime("Hello", 32f);
            _performanceTracker.RecordRenderTime("World", 4f);
            var sortedRenderTimes = _performanceTracker.SortedRenderTimes;
            Assert.Equal(2, sortedRenderTimes.Count);
            Assert.Equal("Hello", sortedRenderTimes[0].Item1);
            Assert.Equal(24f, sortedRenderTimes[0].Item2);
            Assert.Equal("World", sortedRenderTimes[1].Item1);
            Assert.Equal(6f, sortedRenderTimes[1].Item2);
        }
    }
}
