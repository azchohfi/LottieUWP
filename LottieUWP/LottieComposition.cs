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
using LottieUWP.Parser;
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
        private readonly HashSet<string> _warnings = new HashSet<string>();
        private Dictionary<string, List<Layer>> _precomps;
        private Dictionary<string, LottieImageAsset> _images;
        /** Map of font names to fonts */
        public virtual Dictionary<string, Font> Fonts { get; private set; }
        public virtual Dictionary<int, FontCharacter> Characters { get; private set; }
        private Dictionary<long, Layer> _layerMap;
        public List<Layer> Layers { get; private set; }

        // This is stored as a set to avoid duplicates.
        public virtual Rect Bounds { get; private set; }
        public float StartFrame { get; private set; }
        public float EndFrame { get; private set; }
        public float FrameRate { get; private set; }

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
                return (long)(DurationFrames / FrameRate * 1000);
            }
        }

        public void Init(Rect bounds, float startFrame, float endFrame, float frameRate, List<Layer> layers, Dictionary<long, Layer> layerMap, Dictionary<string, List<Layer>> precomps, Dictionary<string, LottieImageAsset> images, Dictionary<int, FontCharacter> characters, Dictionary<string, Font> fonts)
        {
            Bounds = bounds;
            StartFrame = startFrame;
            EndFrame = endFrame;
            FrameRate = frameRate;
            Layers = layers;
            _layerMap = layerMap;
            _precomps = precomps;
            _images = images;
            Characters = characters;
            Fonts = fonts;
        }

        internal virtual List<Layer> GetPrecomps(string id)
        {
            return _precomps[id];
        }

        public virtual bool HasImages => _images.Count > 0;

        public virtual Dictionary<string, LottieImageAsset> Images => _images;

        internal virtual float DurationFrames => EndFrame - StartFrame;

        public override string ToString()
        {
            var sb = new StringBuilder("LottieComposition:\n");
            foreach (var layer in Layers)
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
                var loader = new AsyncCompositionLoader(cancellationToken);
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
                return FromInputStreamSync(stream, true);
            }

            public static LottieComposition FromInputStreamSync(Stream stream, bool close)
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
                    if (close)
                    {
                        stream.CloseQuietly();
                    }
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