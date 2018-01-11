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
        private float _startFrame;
        private float _endFrame;
        private float _frameRate;

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

        public virtual Rect Bounds { get; private set; }

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

        /* Bodymovin version */
        internal int MajorVersion { get; private set; }
        internal int MinorVersion { get; private set; }
        internal int PatchVersion { get; private set; }

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

            [Obsolete]
            public static LottieComposition FromFileSync(ResolutionScale resolutionScale, string fileName)
            {
                return FromFileSync(fileName);
            }

            public static LottieComposition FromFileSync(string fileName)
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
                return FromInputStream(stream);
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

            internal static LottieComposition FromInputStream(Stream stream)
            {
                try
                {
                    return FromJsonSync(new JsonReader(new StreamReader(stream, Encoding.UTF8)));
                }
                catch (IOException e)
                {
                    Debug.WriteLine(new InvalidOperationException("Unable to find file.", e), LottieLog.Tag);
                    Debug.WriteLine("Failed to load composition.", LottieLog.Tag);
                }
                finally
                {
                    stream.CloseQuietly();
                }

                return null;
            }

            [Obsolete]
            internal static LottieComposition FromJsonSync(ResolutionScale resolutionScale, JsonObject json)
            {
                try
                {
                    return FromJsonSync(resolutionScale, new JsonReader(new StringReader(json.ToString())));
                }
                catch (IOException e)
                {
                    throw new InvalidOperationException("Unable to parse json", e);
                }
            }

            [Obsolete]
            internal static LottieComposition FromJsonSync(ResolutionScale resolutionScale, JsonReader reader)
            {
                return FromJsonSync(reader);
            }

            public static LottieComposition FromJsonSync(JsonReader reader)
            {
                float scale = Utils.Utils.DpScale();
                int width = -1;
                LottieComposition composition = new LottieComposition();

                reader.BeginObject();
                while (reader.HasNext())
                {
                    switch (reader.NextName())
                    {
                        case "w":
                            width = reader.NextInt();
                            break;
                        case "h":
                            int height = reader.NextInt();
                            int scaledWidth = (int)(width * scale);
                            int scaledHeight = (int)(height * scale);
                            composition.Bounds = new Rect(0, 0, scaledWidth, scaledHeight);
                            break;
                        case "ip":
                            composition._startFrame = reader.NextDouble();
                            break;
                        case "op":
                            composition._endFrame = reader.NextDouble();
                            break;
                        case "fr":
                            composition._frameRate = reader.NextDouble();
                            break;
                        case "v":
                            var version = reader.NextString();
                            var versions = Regex.Split(version, "\\.");
                            composition.MajorVersion = int.Parse(versions[0]);
                            composition.MinorVersion = int.Parse(versions[1]);
                            composition.PatchVersion = int.Parse(versions[2]);
                            if (!Utils.Utils.IsAtLeastVersion(composition, 4, 5, 0))
                            {
                                composition.AddWarning("Lottie only supports bodymovin >= 4.5.0");
                            }

                            break;
                        case "layers":
                            ParseLayers(reader, composition);
                            break;
                        case "assets":
                            ParseAssets(reader, composition);
                            break;
                        case "fonts":
                            ParseFonts(reader, composition);
                            break;
                        case "chars":
                            ParseChars(reader, composition);
                            break;
                        default:
                            reader.SkipValue();
                            break;
                    }
                }

                reader.EndObject();
                return composition;
            }

            internal static void ParseLayers(JsonReader reader, LottieComposition composition)
            {
                var imageCount = 0;
                reader.BeginArray();

                while (reader.HasNext())
                {
                    var layer = Layer.Factory.NewInstance(reader, composition);
                    if (layer.GetLayerType() == Layer.LayerType.Image)
                    {
                        imageCount++;
                    }

                    AddLayer(composition._layers, composition._layerMap, layer);

                    if (imageCount > 4)
                    {
                        composition.AddWarning($"You have {imageCount} images. Lottie should primarily be used with shapes. If you are using Adobe Illustrator, convert the Illustrator layers to shape layers.");
                    }
                }

                reader.EndArray();
            }

            internal static void ParseAssets(JsonReader reader, LottieComposition composition)
            {
                reader.BeginArray();
                while (reader.HasNext())
                {
                    string id = null;
                    // For precomps 
                    var layers = new List<Layer>();
                    var layerMap = new Dictionary<long, Layer>();
                    // For images 
                    int width = 0;
                    int height = 0;
                    string imageFileName = null;
                    string relativeFolder = null;
                    reader.BeginObject();
                    while (reader.HasNext())
                    {
                        switch (reader.NextName())
                        {
                            case "id":
                                id = reader.NextString();
                                break;
                            case "layers":
                                reader.BeginArray();
                                while (reader.HasNext())
                                {
                                    Layer layer = Layer.Factory.NewInstance(reader, composition);
                                    layerMap.Add(layer.Id, layer);
                                    layers.Add(layer);
                                }

                                reader.EndArray();
                                break;
                            case "w":
                                width = reader.NextInt();
                                break;
                            case "h":
                                height = reader.NextInt();
                                break;
                            case "p":
                                imageFileName = reader.NextString();
                                break;
                            case "u":
                                relativeFolder = reader.NextString();
                                break;
                            default:
                                reader.SkipValue();
                                break;
                        }
                    }
                    reader.EndObject();
                    if (layers.Any())
                    {
                        composition._precomps.Add(id, layers);
                    }
                    else if (imageFileName != null)
                    {
                        LottieImageAsset image = new LottieImageAsset(width, height, id, imageFileName, relativeFolder);
                        composition._images[image.Id] = image;
                    }
                }
                reader.EndArray();
            }

            private static void ParseFonts(JsonReader reader, LottieComposition composition)
            {
                reader.BeginObject();
                while (reader.HasNext())
                {
                    switch (reader.NextName())
                    {
                        case "list":
                            reader.BeginArray();
                            while (reader.HasNext())
                            {
                                Font font = Font.Factory.NewInstance(reader);
                                composition.Fonts.Add(font.Name, font);
                            }
                            reader.EndArray();
                            break;
                        default:
                            reader.SkipValue();
                            break;
                    }
                }
                reader.EndObject();
            }

            private static void ParseChars(JsonReader reader, LottieComposition composition)
            {
                reader.BeginArray();
                while (reader.HasNext())
                {
                    var character = FontCharacter.Factory.NewInstance(reader, composition);
                    composition.Characters.Add(character.GetHashCode(), character);
                }
                reader.EndArray();
            }

            internal static void AddLayer(List<Layer> layers, Dictionary<long, Layer> layerMap, Layer layer)
            {
                layers.Add(layer);
                layerMap[layer.Id] = layer;
            }
        }
    }
}