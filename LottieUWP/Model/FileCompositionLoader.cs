using System.Threading;
using System.Threading.Tasks;

namespace LottieUWP.Model
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
            Utils.Utils.DpScale();
            await Task.Run(() =>
            {
                tcs.SetResult(LottieComposition.Factory.FromInputStream(@params[0]));
            }, _cancellationToken);
            return await tcs.Task;
        }
    }
}