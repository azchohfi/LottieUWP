using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Graphics.Display;

namespace LottieUWP
{
    internal sealed class JsonCompositionLoader
    {
        private readonly CancellationToken _cancellationToken;

        internal JsonCompositionLoader(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        internal async Task<LottieComposition> Execute(params JsonObject[] @params)
        {
            var tcs = new TaskCompletionSource<LottieComposition>();
            var resolutionScale = DisplayInformation.GetForCurrentView().ResolutionScale;
            await Task.Run(() =>
            {
                tcs.SetResult(LottieComposition.Factory.FromJsonSync(resolutionScale, @params[0]));
            }, _cancellationToken);
            return await tcs.Task;
        }
    }
}