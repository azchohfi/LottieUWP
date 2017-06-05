using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Display;

namespace LottieUWP
{
    internal sealed class FileCompositionLoader
    {
        private readonly IOnCompositionLoadedListener _loadedListener;
        private readonly CancellationToken _cancellationToken;

        internal FileCompositionLoader(IOnCompositionLoadedListener loadedListener, CancellationToken cancellationToken)
        {
            _loadedListener = loadedListener;
            _cancellationToken = cancellationToken;
        }

        internal async Task<LottieComposition> Execute(params System.IO.Stream[] @params)
        {
            TaskCompletionSource<LottieComposition> tcs = new TaskCompletionSource<LottieComposition>();
            var resolutionScale = DisplayInformation.GetForCurrentView().ResolutionScale;
            await Task.Run(() =>
            {
                tcs.SetResult(LottieComposition.Factory.FromInputStream(resolutionScale, @params[0]));
            }, _cancellationToken);
            var result = await tcs.Task;
            _loadedListener.OnCompositionLoaded(result);
            return result;
        }
    }
}