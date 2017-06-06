using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Graphics.Display;

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
        private readonly IDictionary<string, IList<Layer>> _precomps = new Dictionary<string, IList<Layer>>();
        private readonly IDictionary<string, LottieImageAsset> _images = new Dictionary<string, LottieImageAsset>();
        private readonly Dictionary<long, Layer> _layerMap = new Dictionary<long, Layer>();
        private readonly IList<Layer> _layers = new List<Layer>();
        private readonly long _startFrame;
        private readonly long _endFrame;
        private readonly int _frameRate;

        private LottieComposition(Rect bounds, long startFrame, long endFrame, int frameRate, float dpScale)
        {
            Bounds = bounds;
            _startFrame = startFrame;
            _endFrame = endFrame;
            _frameRate = frameRate;
            DpScale = dpScale;
        }

        internal virtual Layer LayerModelForId(long id)
        {
            _layerMap.TryGetValue(id, out Layer layer);
            return layer;
        }

        public virtual Rect Bounds { get; }

        public virtual long Duration
        {
            get
            {
                var frameDuration = _endFrame - _startFrame;
                return (long)(frameDuration / (float)_frameRate * 1000);
            }
        }

        internal virtual long EndFrame => _endFrame;

        internal virtual IList<Layer> Layers => _layers;

        internal virtual IList<Layer> GetPrecomps(string id)
        {
            return _precomps[id];
        }

        public virtual bool HasImages()
        {
            return _images.Count > 0;
        }

        internal virtual IDictionary<string, LottieImageAsset> Images => _images;

        internal virtual float DurationFrames => Duration * (float)_frameRate / 1000f;

        public virtual float DpScale { get; }

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
            public static async Task<LottieComposition> FromAssetFileNameAsync(string fileName, IOnCompositionLoadedListener loadedListener, CancellationToken cancellationToken = default(CancellationToken))
            {
                Stream stream;
                try
                {
                    stream = File.OpenRead(fileName);
                }
                catch (IOException e)
                {
                    throw new InvalidOperationException("Unable to find file " + fileName, e);
                }
                return await FromInputStreamAsync(stream, loadedListener, cancellationToken);
            }

            /// <summary>
            /// Loads a composition from an arbitrary input stream.
            /// <para>
            /// ex: fromInputStream(context, new FileInputStream(filePath), (composition) -> {});
            /// </para>
            /// </summary>
            public static async Task<LottieComposition> FromInputStreamAsync(Stream stream, IOnCompositionLoadedListener loadedListener, CancellationToken cancellationToken = default(CancellationToken))
            {
                var loader = new FileCompositionLoader(loadedListener, cancellationToken);
                return await loader.Execute(stream);
            }

            public static LottieComposition FromFileSync(ResolutionScale resolutionScale, string fileName)
            {
                Stream stream;
                try
                {
                    stream = File.OpenRead(fileName);
                }
                catch (IOException e)
                {
                    throw new InvalidOperationException("Unable to find file " + fileName, e);
                }
                return FromInputStream(resolutionScale, stream);
            }

            /// <summary>
            /// Loads a composition from a raw json object. This is useful for animations loaded from the
            /// network.
            /// </summary>
            public static async Task<LottieComposition> FromJsonAsync(JsonObject json, IOnCompositionLoadedListener loadedListener, CancellationToken cancellationToken = default(CancellationToken))
            {
                var loader = new JsonCompositionLoader(loadedListener, cancellationToken);
                return await loader.Execute(json);
            }

            internal static LottieComposition FromInputStream(ResolutionScale resolutionScale,  Stream stream)
            {
                try
                {
                    var size = stream.Length;
                    var buffer = new byte[size];
                    
                    stream.Read(buffer, 0, buffer.Length);
                    var json = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    var jsonObject = JsonObject.Parse(json);
                    return FromJsonSync(resolutionScale, jsonObject);
                }
                catch (IOException e)
                {
                    Debug.WriteLine("Failed to load composition.", new InvalidOperationException("Unable to find file.", e), "LOTTIE");
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to load composition.", new InvalidOperationException("Unable to load JSON.", e), "LOTTIE");
                }
                finally
                {
                    stream.CloseQuietly();
                }

                return null;
            }

            internal static LottieComposition FromJsonSync(ResolutionScale resolutionScale, JsonObject json)
            {
                Rect bounds;
                var scale = (float)resolutionScale / 100.0f;
                var width = (int)json.GetNamedNumber("w", -1);
                var height = (int)json.GetNamedNumber("h", -1);

                if (width != -1 && height != -1)
                {
                    var scaledWidth = (int)(width * scale);
                    var scaledHeight = (int)(height * scale);
                    bounds = new Rect(0, 0, scaledWidth, scaledHeight);
                }

                var startFrame = (long)json.GetNamedNumber("ip", 0);
                var endFrame = (long)json.GetNamedNumber("op", 0);
                var frameRate = (int)json.GetNamedNumber("fr", 0);
                var composition = new LottieComposition(bounds, startFrame, endFrame, frameRate, scale);
                var assetsJson = json.GetNamedArray("assets", null);
                ParseImages(assetsJson, composition);
                ParsePrecomps(assetsJson, composition);
                ParseLayers(json, composition);
                return composition;
            }

            internal static void ParseLayers(JsonObject json, LottieComposition composition)
            {
                var jsonLayers = json.GetNamedArray("layers", null);
                // This should never be null. Bodymovin always exports at least an empty array.
                // However, it seems as if the demarshalling from the React Native library sometimes
                // causes this to be null. The proper fix should be done there but this will prevent a crash.
                // https://github.com/airbnb/lottie-android/issues/279
                if (jsonLayers == null)
                {
                    return;
                }
                var length = jsonLayers.Count;
                for (var i = 0; i < length; i++)
                {
                    var layer = Layer.Factory.NewInstance(jsonLayers[i].GetObject(), composition);
                    AddLayer(composition._layers, composition._layerMap, layer);
                }
            }

            internal static void ParsePrecomps(JsonArray assetsJson, LottieComposition composition)
            {
                if (assetsJson == null)
                {
                    return;
                }
                var length = assetsJson.Count;
                for (var i = 0; i < length; i++)
                {
                    var assetJson = assetsJson[i].GetObject();
                    var layersJson = assetJson.GetNamedArray("layers", null);
                    if (layersJson == null)
                    {
                        continue;
                    }
                    IList<Layer> layers = new List<Layer>(layersJson.Count);
                    var layerMap = new Dictionary<long, Layer>();
                    for (var j = 0; j < layersJson.Count; j++)
                    {
                        var layer = Layer.Factory.NewInstance(layersJson[j].GetObject(), composition);
                        layerMap.Add(layer.Id, layer);
                        layers.Add(layer);
                    }
                    var id = assetJson.GetNamedString("id");
                    composition._precomps[id] = layers;
                }
            }

            internal static void ParseImages(JsonArray assetsJson, LottieComposition composition)
            {
                if (assetsJson == null)
                {
                    return;
                }
                var length = assetsJson.Count;
                for (var i = 0; i < length; i++)
                {
                    var assetJson = assetsJson[i].GetObject();
                    if (!assetJson.ContainsKey("p"))
                    {
                        continue;
                    }
                    var image = LottieImageAsset.Factory.NewInstance(assetJson);
                    composition._images[image.Id] = image;
                }
            }

            internal static void AddLayer(IList<Layer> layers, Dictionary<long, Layer> layerMap, Layer layer)
            {
                layers.Add(layer);
                layerMap.Add(layer.Id, layer);
            }
        }
    }
}