using System.Collections.Generic;
using Windows.UI;
using LottieUWP.Value;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Content;
using LottieUWP.Model.Layer;

namespace LottieUWP.Parser
{
    public static class LayerParser
    {
        public static Layer Parse(LottieComposition composition)
        {
            var bounds = composition.Bounds;
            return new Layer(new List<IContentModel>(), composition, "__container", -1, Layer.LayerType.PreComp, -1, null, new List<Mask>(), new AnimatableTransform(), 0, 0, default(Color), 0, 0, (int)bounds.Width, (int)bounds.Height, null, null, new List<Keyframe<float?>>(), Layer.MatteType.None, null, false);
        }

        public static Layer Parse(JsonReader reader, LottieComposition composition)
        {
            // This should always be set by After Effects. However, if somebody wants to minify 
            // and optimize their json, the name isn't critical for most cases so it can be removed. 
            string layerName = "UNSET";
            Layer.LayerType layerType = Layer.LayerType.Unknown;
            string refId = null;
            long layerId = 0;
            int solidWidth = 0;
            int solidHeight = 0;
            Color solidColor;
            int preCompWidth = 0;
            int preCompHeight = 0;
            long parentId = -1;
            float timeStretch = 1f;
            float startFrame = 0f;
            float inFrame = 0f;
            float outFrame = 0f;
            string cl = null;
            bool hidden = false;

            Layer.MatteType matteType = Layer.MatteType.None;
            AnimatableTransform transform = null;
            AnimatableTextFrame text = null;
            AnimatableTextProperties textProperties = null;
            AnimatableFloatValue timeRemapping = null;

            List<Mask> masks = new List<Mask>();
            List<IContentModel> shapes = new List<IContentModel>();

            reader.BeginObject();
            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "nm":
                        layerName = reader.NextString();
                        break;
                    case "ind":
                        layerId = reader.NextInt();
                        break;
                    case "refId":
                        refId = reader.NextString();
                        break;
                    case "ty":
                        int layerTypeInt = reader.NextInt();
                        if (layerTypeInt < (int)Layer.LayerType.Unknown)
                        {
                            layerType = (Layer.LayerType)layerTypeInt;
                        }
                        else
                        {
                            layerType = Layer.LayerType.Unknown;
                        }
                        break;
                    case "parent":
                        parentId = reader.NextInt();
                        break;
                    case "sw":
                        solidWidth = (int)(reader.NextInt() * Utils.Utils.DpScale());
                        break;
                    case "sh":
                        solidHeight = (int)(reader.NextInt() * Utils.Utils.DpScale());
                        break;
                    case "sc":
                        solidColor = Utils.Utils.GetSolidColorBrush(reader.NextString());
                        break;
                    case "ks":
                        transform = AnimatableTransformParser.Parse(reader, composition);
                        break;
                    case "tt":
                        matteType = (Layer.MatteType)reader.NextInt();
                        break;
                    case "masksProperties":
                        reader.BeginArray();
                        while (reader.HasNext())
                        {
                            masks.Add(MaskParser.Parse(reader, composition));
                        }
                        reader.EndArray();
                        break;
                    case "shapes":
                        reader.BeginArray();
                        while (reader.HasNext())
                        {
                            var shape = ContentModelParser.Parse(reader, composition);
                            if (shape != null)
                            {
                                shapes.Add(shape);
                            }
                        }
                        reader.EndArray();
                        break;
                    case "t":
                        reader.BeginObject();
                        while (reader.HasNext())
                        {
                            switch (reader.NextName())
                            {
                                case "d":
                                    text = AnimatableValueParser.ParseDocumentData(reader, composition);
                                    break;
                                case "a":
                                    reader.BeginArray();
                                    if (reader.HasNext())
                                    {
                                        textProperties = AnimatableTextPropertiesParser.Parse(reader, composition);
                                    }
                                    while (reader.HasNext())
                                    {
                                        reader.SkipValue();
                                    }
                                    reader.EndArray();
                                    break;
                                default:
                                    reader.SkipValue();
                                    break;
                            }
                        }
                        reader.EndObject();
                        break;
                    case "ef":
                        reader.BeginArray();
                        List<string> effectNames = new List<string>();
                        while (reader.HasNext())
                        {
                            reader.BeginObject();
                            while (reader.HasNext())
                            {
                                switch (reader.NextName())
                                {
                                    case "nm":
                                        effectNames.Add(reader.NextString());
                                        break;
                                    default:
                                        reader.SkipValue();
                                        break;
                                }
                            }
                            reader.EndObject();
                        }
                        reader.EndArray();
                        composition.AddWarning("Lottie doesn't support layer effects. If you are using them for " +
                            " fills, strokes, trim paths etc. then try adding them directly as contents " +
                            " in your shape. Found: " + effectNames);
                        break;
                    case "sr":
                        timeStretch = reader.NextDouble();
                        break;
                    case "st":
                        startFrame = reader.NextDouble();
                        break;
                    case "w":
                        preCompWidth = (int)(reader.NextInt() * Utils.Utils.DpScale());
                        break;
                    case "h":
                        preCompHeight = (int)(reader.NextInt() * Utils.Utils.DpScale());
                        break;
                    case "ip":
                        inFrame = reader.NextDouble();
                        break;
                    case "op":
                        outFrame = reader.NextDouble();
                        break;
                    case "tm":
                        timeRemapping = AnimatableValueParser.ParseFloat(reader, composition, false);
                        break;
                    case "cl":
                        cl = reader.NextString();
                        break;
                    case "hd":
                        hidden = reader.NextBoolean();
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }
            reader.EndObject();

            // Bodymovin pre-scales the in frame and out frame by the time stretch. However, that will 
            // cause the stretch to be double counted since the in out animation gets treated the same 
            // as all other animations and will have stretch applied to it again. 
            inFrame /= timeStretch;
            outFrame /= timeStretch;

            List<Keyframe<float?>> inOutKeyframes = new List<Keyframe<float?>>();
            // Before the in frame 
            if (inFrame > 0)
            {
                Keyframe<float?> preKeyframe = new Keyframe<float?>(composition, 0f, 0f, null, 0f, inFrame);
                inOutKeyframes.Add(preKeyframe);
            }

            // The + 1 is because the animation should be visible on the out frame itself. 
            outFrame = (outFrame > 0 ? outFrame : composition.EndFrame);
            Keyframe<float?> visibleKeyframe = new Keyframe<float?>(composition, 1f, 1f, null, inFrame, outFrame);
            inOutKeyframes.Add(visibleKeyframe);

            Keyframe<float?> outKeyframe = new Keyframe<float?>(composition, 0f, 0f, null, outFrame, float.MaxValue);
            inOutKeyframes.Add(outKeyframe);

            if (layerName.EndsWith(".ai") || "ai".Equals(cl))
            {
                composition.AddWarning("Convert your Illustrator layers to shape layers.");
            }

            return new Layer(shapes, composition, layerName, layerId, layerType, parentId, refId, masks, transform, solidWidth, solidHeight, solidColor, timeStretch, startFrame, preCompWidth, preCompHeight, text, textProperties, inOutKeyframes, matteType, timeRemapping, hidden);
        }
    }
}
