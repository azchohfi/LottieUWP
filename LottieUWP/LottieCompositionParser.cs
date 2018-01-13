using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Foundation;
using LottieUWP.Model;
using LottieUWP.Model.Layer;

namespace LottieUWP
{
    public static class LottieCompositionParser
    {
        public static LottieComposition Parse(JsonReader reader)
        {
            var scale = Utils.Utils.DpScale();
            var width = -1;
            var composition = new LottieComposition();

            reader.BeginObject();
            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "w":
                        width = reader.NextInt();
                        break;
                    case "h":
                        var height = reader.NextInt();
                        var scaledWidth = (int)(width * scale);
                        var scaledHeight = (int)(height * scale);
                        composition.Bounds = new Rect(0, 0, scaledWidth, scaledHeight);
                        break;
                    case "ip":
                        composition.StartFrame = reader.NextDouble();
                        break;
                    case "op":
                        composition.EndFrame = reader.NextDouble();
                        break;
                    case "fr":
                        composition.FrameRate = reader.NextDouble();
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

        private static void ParseLayers(JsonReader reader, LottieComposition composition)
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

        private static void ParseAssets(JsonReader reader, LottieComposition composition)
        {
            reader.BeginArray();
            while (reader.HasNext())
            {
                string id = null;
                // For precomps
                var layers = new List<Layer>();
                var layerMap = new Dictionary<long, Layer>();
                // For images
                var width = 0;
                var height = 0;
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
                                var layer = Layer.Factory.NewInstance(reader, composition);
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
                    composition.Precomps.Add(id, layers);
                }
                else if (imageFileName != null)
                {
                    var image =
                        new LottieImageAsset(width, height, id, imageFileName, relativeFolder);
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
                            var font = Font.Factory.NewInstance(reader);
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
                var character =
                    FontCharacter.Factory.NewInstance(reader, composition);
                composition.Characters.Add(character.GetHashCode(), character);
            }
            reader.EndArray();
        }

        private static void AddLayer(List<Layer> layers, Dictionary<long, Layer> layerMap, Layer layer)
        {
            layers.Add(layer);
            layerMap[layer.Id] = layer;
        }
    }
}
