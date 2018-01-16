﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Foundation;
using LottieUWP.Model;
using LottieUWP.Model.Layer;

namespace LottieUWP.Parser
{
    public static class LottieCompositionParser
    {
        public static LottieComposition Parse(JsonReader reader)
        {
            var scale = Utils.Utils.DpScale();
            float startFrame = 0f;
            float endFrame = 0f;
            float frameRate = 0f;
            int majorVersion = 0;
            int minorVersion = 0;
            int patchVersion = 0;
            Dictionary<long, Layer> layerMap = new Dictionary<long, Layer>();
            List<Layer> layers = new List<Layer>();
            int width = 0;
            int height = 0;
            Dictionary<string, List<Layer>> precomps = new Dictionary<string, List<Layer>>();
            Dictionary<string, LottieImageAsset> images = new Dictionary<string, LottieImageAsset>();
            Dictionary<string, Font> fonts = new Dictionary<string, Font>();
            Dictionary<int, FontCharacter> characters = new Dictionary<int, FontCharacter>();
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
                        height = reader.NextInt();
                        break;
                    case "ip":
                        startFrame = reader.NextDouble();
                        break;
                    case "op":
                        endFrame = reader.NextDouble();
                        break;
                    case "fr":
                        frameRate = reader.NextDouble();
                        break;
                    case "v":
                        var version = reader.NextString();
                        var versions = Regex.Split(version, "\\.");
                        majorVersion = int.Parse(versions[0]);
                        minorVersion = int.Parse(versions[1]);
                        patchVersion = int.Parse(versions[2]);
                        if (!Utils.Utils.IsAtLeastVersion(composition, 4, 5, 0))
                        {
                            composition.AddWarning("Lottie only supports bodymovin >= 4.5.0");
                        }
                        break;
                    case "layers":
                        ParseLayers(reader, composition, layers, layerMap);
                        break;
                    case "assets":
                        ParseAssets(reader, composition, precomps, images);
                        break;
                    case "fonts":
                        ParseFonts(reader, fonts);
                        break;
                    case "chars":
                        ParseChars(reader, composition, characters);
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }
            reader.EndObject();

            int scaledWidth = (int)(width * scale);
            int scaledHeight = (int)(height * scale);
            Rect bounds = new Rect(0, 0, scaledWidth, scaledHeight);

            composition.Init(bounds, startFrame, endFrame, frameRate, majorVersion, minorVersion,
                patchVersion, layers, layerMap, precomps, images, characters, fonts);

            return composition;
        }

        private static void ParseLayers(JsonReader reader, LottieComposition composition, List<Layer> layers, Dictionary<long, Layer> layerMap)
        {
            var imageCount = 0;
            reader.BeginArray();
            while (reader.HasNext())
            {
                var layer = LayerParser.Parse(reader, composition);
                if (layer.GetLayerType() == Layer.LayerType.Image)
                {
                    imageCount++;
                }
                layers.Add(layer);
                layerMap[layer.Id] = layer;

                if (imageCount > 4)
                {
                    LottieLog.Warn($"You have {imageCount} images. Lottie should primarily be used with shapes. If you are using Adobe Illustrator, convert the Illustrator layers to shape layers.");
                }
            }
            reader.EndArray();
        }

        private static void ParseAssets(JsonReader reader, LottieComposition composition, Dictionary<string, List<Layer>> precomps, Dictionary<string, LottieImageAsset> images)
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
                                var layer = LayerParser.Parse(reader, composition);
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
                    precomps.Add(id, layers);
                }
                else if (imageFileName != null)
                {
                    var image =
                        new LottieImageAsset(width, height, id, imageFileName, relativeFolder);
                    images[image.Id] = image;
                }
            }
            reader.EndArray();
        }

        private static void ParseFonts(JsonReader reader, Dictionary<string, Font> fonts)
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
                            fonts.Add(font.Name, font);
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

        private static void ParseChars(JsonReader reader, LottieComposition composition, Dictionary<int, FontCharacter> characters)
        {
            reader.BeginArray();
            while (reader.HasNext())
            {
                var character = FontCharacter.Factory.NewInstance(reader, composition);
                characters.Add(character.GetHashCode(), character);
            }
            reader.EndArray();
        }
    }
}
