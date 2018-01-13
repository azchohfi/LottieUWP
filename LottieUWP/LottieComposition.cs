using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using LottieUWP.Model;
using LottieUWP.Model.Layer;
using LottieUWP.Utils;

namespace LottieUWP
{
    /// <summary>
    /// After Effects/Bodymovin composition model. This is the serialized model from which the
    /// animation will be created.
    /// It can be used with a <seealso cref="LottieAnimationView"/> or
    /// <seealso cref="LottieDrawable"/>.
    /// </summary>
    public class LottieComposition
    {
        private readonly PerformanceTracker _performanceTracker = new PerformanceTracker();
        internal readonly Dictionary<string, List<Layer>> Precomps = new Dictionary<string, List<Layer>>();
        internal readonly Dictionary<string, LottieImageAsset> _images = new Dictionary<string, LottieImageAsset>();
        /** Map of font names to fonts */
        public virtual Dictionary<string, Font> Fonts { get; } = new Dictionary<string, Font>();
        public virtual Dictionary<int, FontCharacter> Characters { get; } = new Dictionary<int, FontCharacter>();
        internal readonly Dictionary<long, Layer> _layerMap = new Dictionary<long, Layer>();
        internal readonly List<Layer> _layers = new List<Layer>();
        // This is stored as a set to avoid duplicates.
        private readonly HashSet<string> _warnings = new HashSet<string>();
        public virtual Rect Bounds { get; internal set; }
        public float StartFrame { get; internal set; }
        public float EndFrame { get; internal set; }
        public float FrameRate { get; internal set; }

        internal void AddWarning(string warning)
        {
            Debug.WriteLine(warning, LottieLog.Tag);
            _warnings.Add(warning);
        }

        public List<string> Warnings => _warnings.ToList();

        public virtual bool PerformanceTrackingEnabled
        {
            set => _performanceTracker.Enabled = value;
        }

        public virtual PerformanceTracker PerformanceTracker => _performanceTracker;

        internal virtual Layer LayerModelForId(long id)
        {
            _layerMap.TryGetValue(id, out Layer layer);
            return layer;
        }

        public virtual float Duration
        {
            get
            {
                var frameDuration = EndFrame - StartFrame;
                return (long)(frameDuration / FrameRate * 1000);
            }
        }

        /* Bodymovin version */
        public int MajorVersion { get; internal set; }
        public int MinorVersion { get; internal set; }
        public int PatchVersion { get; internal set; }

        public List<Layer> Layers => _layers;

        internal virtual List<Layer> GetPrecomps(string id)
        {
            return Precomps[id];
        }

        public virtual bool HasImages => _images.Count > 0;

        public virtual Dictionary<string, LottieImageAsset> Images => _images;

        internal virtual float DurationFrames => Duration * FrameRate / 1000f;

        public override string ToString()
        {
            var sb = new StringBuilder("LottieComposition:\n");
            foreach (var layer in _layers)
            {
                sb.Append(layer.ToString("\t"));
            }
            return sb.ToString();
        }

        public static class Factory
        {
            /// <summary>
            /// Loads a composition from a file stored in /assets.
            /// </summary>
            public static async Task<LottieComposition> FromAssetFileNameAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken))
            {
                Stream stream;
                try
                {
                    stream = File.OpenRead(fileName);
                }
                catch (IOException e)
                {
                    throw new ArgumentException("Unable to find file " + fileName, e);
                }

                return await FromInputStreamAsync(stream, cancellationToken);
            }

            /// <summary>
            /// Loads a composition from an arbitrary input stream.
            /// <para>
            /// ex: fromInputStream(context, new FileInputStream(filePath), (composition) -> {});
            /// </para>
            /// </summary>
            public static async Task<LottieComposition> FromInputStreamAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
            {
                return await FromJsonReaderAsync(new JsonReader(new StreamReader(stream, Encoding.UTF8)), cancellationToken);
            }

            /// <summary>
            /// Loads a composition from a json string. This is preferable to loading a JSONObject because 
            /// internally, Lottie uses {@link JsonReader} so any original overhead to create the JSONObject 
            /// is wasted. 
            /// 
            /// This is the preferred method to use when loading an animation from the network because you 
            /// have the response body as a raw string already. No need to convert it to a JSONObject. 
            /// 
            /// If you do have a JSONObject, you can call: 
            ///    `new JsonReader(new StringReader(jsonObject));` 
            /// However, this is not recommended. 
            /// </summary>
            /// <param name="jsonString"></param>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public static async Task<LottieComposition> FromJsonStringAsync(string jsonString, CancellationToken cancellationToken = default(CancellationToken))
            {
                return await FromJsonReaderAsync(new JsonReader(new StringReader(jsonString)), cancellationToken);
            }

            /// <summary>
            /// Loads a composition from a json reader.
            /// ex: fromInputStream(context, new FileInputStream(filePath), (composition) -> {});
            /// </summary>
            /// <param name="reader"></param>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public static async Task<LottieComposition> FromJsonReaderAsync(JsonReader reader, CancellationToken cancellationToken = default(CancellationToken))
            {
                var loader = new JsonCompositionLoader(cancellationToken);
                return await loader.Execute(reader);
            }

            public static LottieComposition FromFileSync(string fileName)
            {
                try
                {
                    return FromInputStreamSync(File.OpenRead(fileName));
                }
                catch (IOException e)
                {
                    throw new InvalidOperationException("Unable to open asset " + fileName, e);
                }
            }

            public static LottieComposition FromInputStreamSync(Stream stream)
            {
                LottieComposition composition;
                try
                {
                    composition = FromJsonSync(new JsonReader(new StreamReader(stream, Encoding.UTF8)));
                }
                catch (IOException e)
                {
                    throw new InvalidOperationException("Unable to parse composition.", e);
                }
                finally
                {
                    stream.CloseQuietly();
                }

                return composition;
            }

            public static LottieComposition FromJsonSync(JsonReader reader)
            {
                return LottieCompositionParser.Parse(reader);
            }
        }
    }
}