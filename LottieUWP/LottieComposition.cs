using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Graphics.Display;
using LottieUWP.Model;
using LottieUWP.Model.Layer;
using LottieUWP.Utils;
using System.Text.RegularExpressions;

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
        private readonly Dictionary<string, List<Layer>> _precomps = new Dictionary<string, List<Layer>>();
        private readonly Dictionary<string, LottieImageAsset> _images = new Dictionary<string, LottieImageAsset>();
        /** Map of font names to fonts */

        private readonly Dictionary<long, Layer> _layerMap = new Dictionary<long, Layer>();
        private readonly List<Layer> _layers = new List<Layer>();
        // This is stored as a set to avoid duplicates.
        private readonly HashSet<string> _warnings = new HashSet<string>();
        private readonly PerformanceTracker _performanceTracker = new PerformanceTracker();
        private readonly float _startFrame;
        private readonly float _endFrame;
        private readonly float _frameRate;

        private LottieComposition(Rect bounds, long startFrame, long endFrame, float frameRate, float dpScale, int major, int minor, int patch)
        {
            Bounds = bounds;
            _startFrame = startFrame;
            _endFrame = endFrame;
            _frameRate = frameRate;
            DpScale = dpScale;
            MajorVersion = major;
            MinorVersion = minor;
            PatchVersion = patch;
            if (!Utils.Utils.IsAtLeastVersion(this, 4, 5, 0))
            {
                AddWarning("Lottie only supports bodymovin >= 4.5.0");
            }
        }

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

        public virtual Rect Bounds { get; }

        public virtual float Duration
        {
            get
            {
                var frameDuration = _endFrame - _startFrame;
                return (long)(frameDuration / _frameRate * 1000);
            }
        }

        internal virtual float StartFrame => _startFrame;

        internal virtual float EndFrame => _endFrame;

        internal virtual List<Layer> Layers => _layers;

        internal virtual List<Layer> GetPrecomps(string id)
        {
            return _precomps[id];
        }

        internal virtual Dictionary<string, Font> Fonts { get; } = new Dictionary<string, Font>();

        public virtual bool HasImages()
        {
            return _images.Count > 0;
        }

        public virtual Dictionary<string, LottieImageAsset> Images => _images;

        internal virtual Dictionary<int, FontCharacter> Characters { get; } = new Dictionary<int, FontCharacter>();

        internal virtual float DurationFrames => Duration * _frameRate / 1000f;

        internal virtual float DpScale { get; }

        /* Bodymovin version */
        internal int MajorVersion { get; }
        internal int MinorVersion { get; }
        internal int PatchVersion { get; }

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
                    throw new InvalidOperationException("Unable to find file " + fileName, e);
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
                var loader = new FileCompositionLoader(cancellationToken);
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
            public static async Task<LottieComposition> FromJsonAsync(JsonObject json, CancellationToken cancellationToken = default(CancellationToken))
            {
                var loader = new JsonCompositionLoader(cancellationToken);
                return await loader.Execute(json);
            }

            internal static LottieComposition FromInputStream(ResolutionScale resolutionScale, Stream stream)
            {
                try
                {
                    using (var bufferedReader = new StreamReader(stream, Encoding.UTF8))
                    {
                        var total = new StringBuilder();
                        string line;
                        while ((line = bufferedReader.ReadLine()) != null)
                        {
                            total.Append(line);
                        }
                        var jsonObject = JsonObject.Parse(total.ToString());
                        return FromJsonSync(resolutionScale, jsonObject);
                    }
                }
                catch (IOException e)
                {
                    Debug.WriteLine(new InvalidOperationException("Unable to find file.", e), LottieLog.Tag);
                    Debug.WriteLine("Failed to load composition.", LottieLog.Tag);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(new InvalidOperationException("Unable to load JSON.", e), LottieLog.Tag);
                    Debug.WriteLine("Failed to load composition.", LottieLog.Tag);
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
                var frameRate = (float)json.GetNamedNumber("fr", 0);
                var version = json.GetNamedString("v");
                var versions = Regex.Split(version, "\\.");
                var major = int.Parse(versions[0]);
                var minor = int.Parse(versions[1]);
                var patch = int.Parse(versions[2]);
                var composition = new LottieComposition(bounds, startFrame, endFrame, frameRate, scale, major, minor, patch);
                var assetsJson = json.GetNamedArray("assets", null);
                ParseImages(assetsJson, composition);
                ParsePrecomps(assetsJson, composition);
                ParseFonts(json.GetNamedObject("fonts", null), composition);
                ParseChars(json.GetNamedArray("chars", null), composition);
                ParseLayers(json, composition);
                return composition;
            }

            internal static void ParseLayers(JsonObject json, LottieComposition composition)
            {
                var jsonLayers = json.GetNamedArray("layers", null);
                // This should never be null. Bodymovin always exports at least an empty array.
                // However, it seems as if the unmarshalling from the React Native library sometimes
                // causes this to be null. The proper fix should be done there but this will prevent a crash.
                // https://github.com/airbnb/lottie-android/issues/279
                if (jsonLayers == null)
                {
                    return;
                }
                var length = jsonLayers.Count;
                var imageCount = 0;
                for (var i = 0; i < length; i++)
                {
                    var layer = Layer.Factory.NewInstance(jsonLayers[i].GetObject(), composition);
                    if (layer.GetLayerType() == Layer.LayerType.Image)
                    {
                        imageCount++;
                    }
                    AddLayer(composition._layers, composition._layerMap, layer);
                }

                if (imageCount > 4)
                {
                    composition.AddWarning($"You have {imageCount} images. Lottie should primarily be used with shapes. If you are using Adobe Illustrator, convert the Illustrator layers to shape layers.");
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
                    var layers = new List<Layer>(layersJson.Count);
                    var layerMap = new Dictionary<long, Layer>();
                    for (var j = 0; j < layersJson.Count; j++)
                    {
                        var layer = Layer.Factory.NewInstance(layersJson[j].GetObject(), composition);
                        layerMap[layer.Id] = layer;
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

            private static void ParseFonts(JsonObject fonts, LottieComposition composition)
            {
                var fontsList = fonts?.GetNamedArray("list", null);
                if (fontsList == null)
                {
                    return;
                }
                var length = fontsList.Count;
                for (uint i = 0; i < length; i++)
                {
                    var font = Font.Factory.NewInstance(fontsList.GetObjectAt(i));
                    composition.Fonts.Add(font.Name, font);
                }
            }

            private static void ParseChars(JsonArray charsJson, LottieComposition composition)
            {
                if (charsJson == null)
                    return;

                var length = charsJson.Count;
                for (uint i = 0; i < length; i++)
                {
                    var character = FontCharacter.Factory.NewInstance(charsJson.GetObjectAt(i), composition);
                    composition.Characters.Add(character.GetHashCode(), character);
                }
            }

            internal static void AddLayer(List<Layer> layers, Dictionary<long, Layer> layerMap, Layer layer)
            {
                layers.Add(layer);
                layerMap[layer.Id] = layer;
            }
        }
    }
}