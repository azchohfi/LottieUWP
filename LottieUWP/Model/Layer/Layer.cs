using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Windows.UI;
using LottieUWP.Animation;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Content;

namespace LottieUWP.Model.Layer
{
    internal class Layer
    {
        internal enum LayerType
        {
            PreComp,
            Solid,
            Image,
            Null,
            Shape,
            Text,
            Unknown
        }

        internal enum MatteType
        {
            None,
            Add,
            Invert,
            Unknown
        }

        private readonly List<IContentModel> _shapes;
        private readonly LottieComposition _composition;
        private readonly LayerType _layerType;
        private readonly MatteType _matteType;

        private Layer(List<IContentModel> shapes, LottieComposition composition, string layerName, long layerId, LayerType layerType, long parentId, string refId, List<Mask> masks, AnimatableTransform transform, int solidWidth, int solidHeight, Color solidColor, float timeStretch, float startFrame, int preCompWidth, int preCompHeight, AnimatableTextFrame text, AnimatableTextProperties textProperties, List<Keyframe<float?>> inOutKeyframes, MatteType matteType, AnimatableFloatValue timeRemapping)
        {
            _shapes = shapes;
            _composition = composition;
            Name = layerName;
            Id = layerId;
            _layerType = layerType;
            ParentId = parentId;
            RefId = refId;
            Masks = masks;
            Transform = transform;
            SolidWidth = solidWidth;
            SolidHeight = solidHeight;
            SolidColor = solidColor;
            TimeStretch = timeStretch;
            StartFrame = startFrame;
            PreCompWidth = preCompWidth;
            PreCompHeight = preCompHeight;
            Text = text;
            TextProperties = textProperties;
            InOutKeyframes = inOutKeyframes;
            _matteType = matteType;
            TimeRemapping = timeRemapping;
        }

        internal virtual LottieComposition Composition => _composition;

        internal virtual float TimeStretch { get; }

        internal virtual float StartFrame { get; }

        internal virtual float StartProgress => StartFrame / _composition.DurationFrames;

        internal virtual List<Keyframe<float?>> InOutKeyframes { get; }

        internal virtual long Id { get; }

        internal virtual string Name { get; }

        internal virtual string RefId { get; }

        internal virtual int PreCompWidth { get; }

        internal virtual int PreCompHeight { get; }

        internal virtual AnimatableTextFrame Text { get; }

        internal virtual AnimatableTextProperties TextProperties { get; }

        internal virtual AnimatableFloatValue TimeRemapping { get; }

        internal virtual List<Mask> Masks { get; }

        internal virtual LayerType GetLayerType()
        {
            return _layerType;
        }

        internal virtual MatteType GetMatteType()
        {
            return _matteType;
        }

        internal virtual long ParentId { get; }

        internal virtual List<IContentModel> Shapes => _shapes;

        internal virtual AnimatableTransform Transform { get; }

        internal virtual Color SolidColor { get; }

        internal virtual int SolidHeight { get; }

        internal virtual int SolidWidth { get; }

        public override string ToString()
        {
            return ToString("");
        }

        internal virtual string ToString(string prefix)
        {
            var sb = new StringBuilder();
            sb.Append(prefix).Append(Name).Append("\n");
            var parent = _composition.LayerModelForId(ParentId);
            if (parent != null)
            {
                sb.Append("\t\tParents: ").Append(parent.Name);
                parent = _composition.LayerModelForId(parent.ParentId);
                while (parent != null)
                {
                    sb.Append("->").Append(parent.Name);
                    parent = _composition.LayerModelForId(parent.ParentId);
                }
                sb.Append(prefix).Append("\n");
            }
            if (Masks.Count > 0)
            {
                sb.Append(prefix).Append("\tMasks: ").Append(Masks.Count).Append("\n");
            }
            if (SolidWidth != 0 && SolidHeight != 0)
            {
                sb.Append(prefix).Append("\tBackground: ").Append(string.Format(CultureInfo.InvariantCulture, "%dx%d %X\n", SolidWidth, SolidHeight, SolidColor));
            }
            if (_shapes.Count > 0)
            {
                sb.Append(prefix).Append("\tShapes:\n");
                foreach (var shape in _shapes)
                {
                    sb.Append(prefix).Append("\t\t").Append(shape).Append("\n");
                }
            }
            return sb.ToString();
        }

        internal static class Factory
        {
            internal static Layer NewInstance(LottieComposition composition)
            {
                var bounds = composition.Bounds;
                return new Layer(new List<IContentModel>(), composition, "__container", -1, LayerType.PreComp, -1, null, new List<Mask>(), AnimatableTransform.Factory.NewInstance(), 0, 0, default(Color), 0, 0, (int)bounds.Width, (int)bounds.Height, null, null, new List<Keyframe<float?>>(), MatteType.None, null);
            }

            internal static Layer NewInstance(JsonReader reader, LottieComposition composition)
            {
                string layerName = null;
                LayerType layerType = LayerType.Unknown;
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

                MatteType matteType = MatteType.None;
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
                            if (layerTypeInt < (int)LayerType.Unknown)
                            {
                                layerType = (LayerType)layerTypeInt;
                            }
                            else
                            {
                                layerType = LayerType.Unknown;
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
                            transform = AnimatableTransform.Factory.NewInstance(reader, composition);
                            break;
                        case "tt":
                            matteType = (MatteType)reader.NextInt();
                            break;
                        case "masksProperties":
                            reader.BeginArray();
                            while (reader.HasNext())
                            {
                                masks.Add(Mask.Factory.NewMask(reader, composition));
                            }

                            reader.EndArray();
                            break;
                        case "shapes":
                            reader.BeginArray();
                            while (reader.HasNext())
                            {
                                var shape = ShapeGroup.ShapeItemWithJson(reader, composition);
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
                                        text = AnimatableTextFrame.Factory.NewInstance(reader, composition);
                                        break;
                                    case "a":
                                        reader.BeginArray();
                                        if (reader.HasNext())
                                        {
                                            textProperties = AnimatableTextProperties.Factory.NewInstance(reader, composition);
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
                            timeRemapping = AnimatableFloatValue.Factory.NewInstance(reader, composition, false);
                            break;
                        case "cl":
                            cl = reader.NextString();
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
                outFrame = (outFrame > 0 ? outFrame : composition.EndFrame) + 1;
                Keyframe<float?> visibleKeyframe = new Keyframe<float?>(composition, 1f, 1f, null, inFrame, outFrame);
                inOutKeyframes.Add(visibleKeyframe);

                Keyframe<float?> outKeyframe = new Keyframe<float?>(composition, 0f, 0f, null, outFrame, float.MaxValue);
                inOutKeyframes.Add(outKeyframe);

                if (layerName.EndsWith(".ai") || "ai".Equals(cl))
                {
                    composition.AddWarning("Convert your Illustrator layers to shape layers.");
                }

                return new Layer(shapes, composition, layerName, layerId, layerType, parentId, refId, masks, transform, solidWidth, solidHeight, solidColor, timeStretch, startFrame, preCompWidth, preCompHeight, text, textProperties, inOutKeyframes, matteType, timeRemapping);
            }
        }
    }
}