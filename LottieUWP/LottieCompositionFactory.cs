using LottieUWP.Parser;
using LottieUWP.Utils;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LottieUWP
{
    /// <summary>
    /// Helpers to create a LottieComposition.
    /// </summary>
    public static class LottieCompositionFactory
    {
        static LottieCompositionFactory()
        {
            Utils.Utils.DpScale();
        }

        public static async Task<LottieResult<LottieComposition>> FromAsset(string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Task.Run(() =>
            {
                return FromAssetSync(fileName);
            }, cancellationToken).ConfigureAwait(false);
        }

        public static LottieResult<LottieComposition> FromAssetSync(string fileName)
        {
            try
            {
                return FromJsonInputStreamSync(File.OpenRead(fileName));
            }
            catch (IOException e)
            {
                return new LottieResult<LottieComposition>(e);
            }
        }

        public static async Task<LottieResult<LottieComposition>> FromJsonInputStream(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Task.Run(() =>
            {
                return FromJsonInputStreamSync(stream);
            }, cancellationToken).ConfigureAwait(false);
        }

        public static LottieResult<LottieComposition> FromJsonInputStreamSync(Stream stream)
        {
            return FromJsonInputStreamSync(stream, true);
        }

        /// <summary>
        /// Return a LottieComposition for the given Stream to json.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="close"></param>
        /// <returns></returns>
        public static LottieResult<LottieComposition> FromJsonInputStreamSync(Stream stream, bool close)
        {
            try
            {
                return FromJsonReaderSync(new JsonReader(new StreamReader(stream, Encoding.UTF8)));
            }
            finally
            {
                if (close)
                {
                    stream.CloseQuietly();
                }
            }
        }

        public static async Task<LottieResult<LottieComposition>> FromJsonString(string json, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Task.Run(() =>
            {
                return FromJsonStringSync(json);
            }, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Return a LottieComposition for the specified raw json string. 
        /// If loading from a file, it is preferable to use the InputStream.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static LottieResult<LottieComposition> FromJsonStringSync(string json)
        {
            return FromJsonReaderSync(new JsonReader(new StringReader(json)));
        }

        public static async Task<LottieResult<LottieComposition>> FromJsonReader(JsonReader reader, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await Task.Run(() =>
            {
                return FromJsonReaderSync(reader);
            }, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Return a LottieComposition for the specified json. 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static LottieResult<LottieComposition> FromJsonReaderSync(JsonReader reader)
        {
            try
            {
                return new LottieResult<LottieComposition>(LottieCompositionParser.Parse(reader));
            }
            catch (Exception e)
            {
                return new LottieResult<LottieComposition>(e);
            }
        }
    }
}
