using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Xunit;

namespace LottieUWP.Tests
{
    public class LottieCompositionFactoryTest : IAsyncLifetime
    {
        private const string _json = "{\"v\":\"4.11.1\",\"fr\":60,\"ip\":0,\"op\":180,\"w\":300,\"h\":300,\"nm\":\"Comp 1\",\"ddd\":0,\"assets\":[]," +
          "\"layers\":[{\"ddd\":0,\"ind\":1,\"ty\":4,\"nm\":\"Shape Layer 1\",\"sr\":1,\"ks\":{\"o\":{\"a\":0,\"k\":100,\"ix\":11},\"r\":{\"a\":0," +
          "\"k\":0,\"ix\":10},\"p\":{\"a\":0,\"k\":[150,150,0],\"ix\":2},\"a\":{\"a\":0,\"k\":[0,0,0],\"ix\":1},\"s\":{\"a\":0,\"k\":[100,100,100]," +
          "\"ix\":6}},\"ao\":0,\"shapes\":[{\"ty\":\"rc\",\"d\":1,\"s\":{\"a\":0,\"k\":[100,100],\"ix\":2},\"p\":{\"a\":0,\"k\":[0,0],\"ix\":3}," +
          "\"r\":{\"a\":0,\"k\":0,\"ix\":4},\"nm\":\"Rectangle Path 1\",\"mn\":\"ADBE Vector Shape - Rect\",\"hd\":false},{\"ty\":\"fl\"," +
          "\"c\":{\"a\":0,\"k\":[0.928262987324,0,0,1],\"ix\":4},\"o\":{\"a\":0,\"k\":100,\"ix\":5},\"r\":1,\"nm\":\"Fill 1\",\"mn\":\"ADBE Vector " +
          "Graphic - Fill\",\"hd\":false}],\"ip\":0,\"op\":180,\"st\":0,\"bm\":0}]}";

        private const string _notJson = "not json";

        public async Task InitializeAsync()
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                Utils.Utils.DpScale();
            });
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public void TestLoadJsonString()
        {
            LottieResult<LottieComposition> result = LottieCompositionFactory.FromJsonStringSync(_json);
            Assert.Null(result.Exception);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public void TestLoadInvalidJsonString()
        {
            LottieResult<LottieComposition> result = LottieCompositionFactory.FromJsonStringSync(_notJson);
            Assert.NotNull(result.Exception);
            Assert.Null(result.Value);
        }

        [Fact]
        public void TestLoadJsonReader()
        {
            JsonReader reader = new JsonReader(new StringReader(_json));
            LottieResult<LottieComposition> result = LottieCompositionFactory.FromJsonReaderSync(reader);
            Assert.Null(result.Exception);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public void TestLoadInvalidJsonReader()
        {
            JsonReader reader = new JsonReader(new StringReader(_notJson));
            LottieResult<LottieComposition> result = LottieCompositionFactory.FromJsonReaderSync(reader);
            Assert.NotNull(result.Exception);
            Assert.Null(result.Value);
        }

        [Fact]
        public void TestLoadInvalidAssetName()
        {
            LottieResult<LottieComposition> result = LottieCompositionFactory.FromAssetSync("square2.json");
            Assert.Equal(typeof(FileNotFoundException), result.Exception.GetType());
            Assert.Null(result.Value);
        }

        [Fact]
        public void TestNonJsonAssetFile()
        {
            LottieResult<LottieComposition> result = LottieCompositionFactory.FromAssetSync("not_json.txt");
            Assert.NotNull(result.Exception);
            Assert.Null(result.Value);
        }
    }
}