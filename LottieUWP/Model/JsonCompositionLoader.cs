using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace LottieUWP.Model
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
            Utils.Utils.DpScale();
            await Task.Run(() =>
            {
                try
                {
                    var reader = new JsonReader(new StringReader(@params[0].ToString()));
                    tcs.SetResult(LottieComposition.Factory.FromJsonSync(reader));
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