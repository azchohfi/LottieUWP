using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LottieUWP.Model
{
    internal sealed class JsonCompositionLoader
    {
        private readonly CancellationToken _cancellationToken;

        internal JsonCompositionLoader(CancellationToken cancellationToken)
        {
            Utils.Utils.DpScale();
            _cancellationToken = cancellationToken;
        }

        internal async Task<LottieComposition> Execute(params JsonReader[] @params)
        {
            var tcs = new TaskCompletionSource<LottieComposition>();
            await Task.Run(() =>
            {
                try
                {
                    tcs.SetResult(LottieComposition.Factory.FromJsonSync(@params[0]));
                }
                catch (IOException e)
                {
                    throw new InvalidOperationException(e.Message);
                }
            }, _cancellationToken);
            return await tcs.Task;
        }
    }
}