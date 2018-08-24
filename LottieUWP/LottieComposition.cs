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

namespace LottieUWP
{
    /// <summary>
    /// After Effects/Bodymovin composition model. This is the serialized model from which the
    /// animation will be created.
    /// 
    /// To create one, use <see cref="LottieCompositionFactory"/>.
    /// 
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
        public Dictionary<string, Font> Fonts { get; private set; }
        public Dictionary<int, FontCharacter> Characters { get; private set; }
        private Dictionary<long, Layer> _layerMap;
        public List<Layer> Layers { get; private set; }

        // This is stored as a set to avoid duplicates.
        public Rect Bounds { get; private set; }
        public float StartFrame { get; private set; }
        public float EndFrame { get; private set; }
        public float FrameRate { get; private set; }

        internal void AddWarning(string warning)
        {
            Debug.WriteLine(warning, LottieLog.Tag);
            _warnings.Add(warning);
        }

        public List<string> Warnings => _warnings.ToList();

        public bool PerformanceTrackingEnabled
        {
            set => _performanceTracker.Enabled = value;
        }

        public PerformanceTracker PerformanceTracker => _performanceTracker;

        internal Layer LayerModelForId(long id)
        {
            _layerMap.TryGetValue(id, out Layer layer);
            return layer;
        }

        public float Duration
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

        internal List<Layer> GetPrecomps(string id)
        {
            return _precomps[id];
        }

        public bool HasImages => _images.Count > 0;

        public Dictionary<string, LottieImageAsset> Images => _images;

        internal float DurationFrames => EndFrame - StartFrame;

        public override string ToString()
        {
            var sb = new StringBuilder("LottieComposition:\n");
            foreach (var layer in Layers)
            {
                sb.Append(layer.ToString("\t"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// This will be removed in the next version of Lottie. <see cref="LottieCompositionFactory"/> has improved
        /// API names, failure handlers, and will return in-progress tasks so you will never parse the same
        /// animation twice in parallel.
        /// <see cref="LottieCompositionFactory"/>
        /// </summary>
        [Obsolete]
        public static class Factory
        {
            /// <summary>
            /// <see cref="LottieCompositionFactory.FromAsset(string)"/>
            /// </summary>
            [Obsolete]
            public static async Task<LottieComposition> FromAssetFileNameAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken))
            {
                return (await LottieCompositionFactory.FromAsset(null, fileName, cancellationToken)).Value;
            }

            /// <summary>
            /// <see cref="LottieCompositionFactory.FromJsonInputStream(Stream)"/>
            /// </summary>
            [Obsolete]
            public static async Task<LottieComposition> FromInputStreamAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
            {
                return (await LottieCompositionFactory.FromJsonInputStreamAsync(stream, null, cancellationToken)).Value;
            }

            /// <summary>
            /// <see cref="LottieCompositionFactory.FromJsonString(string)"/>
            /// </summary>
            [Obsolete]
            public static async Task<LottieComposition> FromJsonStringAsync(string jsonString, CancellationToken cancellationToken = default(CancellationToken))
            {
                return (await LottieCompositionFactory.FromJsonString(jsonString, null, cancellationToken)).Value;
            }

            /// <summary>
            /// <see cref="LottieCompositionFactory.FromJsonReader(JsonReader)"/>
            /// </summary>
            [Obsolete]
            public static async Task<LottieComposition> FromJsonReaderAsync(JsonReader reader, CancellationToken cancellationToken = default(CancellationToken))
            {
                return (await LottieCompositionFactory.FromJsonReader(reader, null, cancellationToken)).Value;
            }

            /// <summary>
            /// <see cref="LottieCompositionFactory.FromAssetSync(string)"/>
            /// </summary>
            [Obsolete]
            public static LottieComposition FromFileSync(string fileName)
            {
                return LottieCompositionFactory.FromAssetSync(null, fileName).Value;
            }

            /// <summary>
            /// <see cref="LottieCompositionFactory.FromJsonInputStreamSync(Stream)"/>
            /// </summary>
            [Obsolete]
            public static LottieComposition FromInputStreamSync(Stream stream)
            {
                return LottieCompositionFactory.FromJsonInputStreamSync(stream, null).Value;
            }

            /// <summary>
            /// This will now auto-close the input stream!
            /// <see cref="LottieCompositionFactory.FromJsonInputStreamSync(Stream, bool)"/>
            /// </summary>
            [Obsolete]
            public static LottieComposition FromInputStreamSync(Stream stream, bool close)
            {
                if (close)
                {
                    Debug.WriteLine("Lottie now auto-closes input stream!", LottieLog.Tag);
                }
                return LottieCompositionFactory.FromJsonInputStreamSync(stream, null).Value;
            }

            /// <summary>
            /// <see cref="LottieCompositionFactory.FromJsonSync(JsonReader)"/>
            /// </summary>
            [Obsolete]
            public static LottieComposition FromJsonSync(JsonReader reader)
            {
                return LottieCompositionFactory.FromJsonReaderSync(reader, null).Value;
            }
        }
    }
}