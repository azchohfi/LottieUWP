using LottieUWP.Model;
using LottieUWP.Model.Layer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Xunit;

namespace LottieUWP.Tests
{
    public class LottieDrawableTest
    {
        private LottieComposition CreateComposition(int startFrame, int endFrame)
        {
            LottieComposition composition = new LottieComposition();
            composition.Init(new Rect(), startFrame, endFrame, 1000, new List<Layer>(),
                new Dictionary<long, Layer>(0), new Dictionary<string, List<Layer>>(0),
                new Dictionary<string, LottieImageAsset>(0), new Dictionary<int, FontCharacter>(0),
                new Dictionary<string, Font>(0));
            return composition;
        }

        [Fact]
        public async Task TestMinFrame()
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                var composition = CreateComposition(31, 391);
                var drawable = new LottieDrawable();

                drawable.SetComposition(composition);
                drawable.MinProgress = 0.42f;
                Assert.Equal(182.2f, drawable.MinFrame);

                drawable.ClearComposition();
            });
        }

        [Fact]
        public async Task TestMinWithStartFrameFrame()
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                var composition = CreateComposition(100, 200);
                var drawable = new LottieDrawable();
                drawable.SetComposition(composition);
                drawable.MinProgress = 0.5f;
                Assert.Equal(150f, drawable.MinFrame);
            });
        }

        [Fact]
        public async Task TestMaxFrame()
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                var composition = CreateComposition(31, 391);
                var drawable = new LottieDrawable();
                drawable.SetComposition(composition);
                drawable.MaxProgress = 0.25f;
                Assert.Equal(121f, drawable.MaxFrame);
            });
        }

        [Fact]
        public async Task TestMinMaxFrame()
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                var composition = CreateComposition(31, 391);
                var drawable = new LottieDrawable();
                drawable.SetComposition(composition);
                drawable.SetMinAndMaxProgress(0.25f, 0.42f);
                Assert.Equal(121f, drawable.MinFrame);
                Assert.Equal(182f, drawable.MaxFrame);
            });
        }
    }
}
