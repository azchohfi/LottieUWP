using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LottieUWP.Model
{
    public sealed class AsyncCompositionLoader
    {
        private readonly CancellationToken _cancellationToken;
        private TaskCompletionSource<LottieComposition> _tcs;

        internal AsyncCompositionLoader(CancellationToken cancellationToken)
        {
            Utils.Utils.DpScale();
            _cancellationToken = cancellationToken;
        }

        internal async Task<LottieComposition> Execute(params JsonReader[] @params)
        {
            _tcs = new TaskCompletionSource<LottieComposition>();
            await Task.Run(() =>
            {
                try
                {
                    _tcs.SetResult(LottieComposition.Factory.FromJsonSync(@params[0]));
                }
                catch (IOException e)
                {
                    throw new InvalidOperationException(e.Message);
                }
            }, _cancellationToken);
            return await _tcs.Task;
        }

        public void Cancel()
        {
            _tcs.SetCanceled();
        }
    }
}