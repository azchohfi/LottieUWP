using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Display;

namespace LottieUWP
{
    internal sealed class FileCompositionLoader
    {
        private readonly CancellationToken _cancellationToken;

        internal FileCompositionLoader(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        internal async Task<LottieComposition> Execute(params System.IO.Stream[] @params)
        {
            var tcs = new TaskCompletionSource<LottieComposition>();
            var resolutionScale = DisplayInformation.GetForCurrentView().ResolutionScale;
            await Task.Run(() =>
            {
                tcs.SetResult(LottieComposition.Factory.FromInputStream(resolutionScale, @params[0]));
            }, _cancellationToken);
            return await tcs.Task;
        }
    }
}