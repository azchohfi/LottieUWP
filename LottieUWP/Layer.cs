using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.UI;

namespace LottieUWP
{
    internal class Layer
    {
        private static readonly string Tag = typeof(Layer).Name;

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

        private readonly IList<object> _shapes;
        private readonly LottieComposition _composition;
        private readonly LayerType _layerType;
        private readonly MatteType _matteType;

        private Layer(IList<object> shapes, LottieComposition composition, string layerName, long layerId, LayerType layerType, long parentId, string refId, IList<Mask> masks, AnimatableTransform transform, int solidWidth, int solidHeight, Color solidColor, float timeStretch, float startProgress, int preCompWidth, int preCompHeight, IList<IKeyframe<float?>> inOutKeyframes, MatteType matteType)
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
            StartProgress = startProgress;
            PreCompWidth = preCompWidth;
            PreCompHeight = preCompHeight;
            InOutKeyframes = inOutKeyframes;
            _matteType = matteType;
        }

        internal virtual LottieComposition Composition => _composition;

        internal virtual float TimeStretch { get; }

        internal virtual float StartProgress { get; }

        internal virtual IList<IKeyframe<float?>> InOutKeyframes { get; }

        internal virtual long Id { get; }

        internal virtual string Name { get; }

        internal virtual string RefId { get; }

        internal virtual int PreCompWidth { get; }

        internal virtual int PreCompHeight { get; }

        internal virtual IList<Mask> Masks { get; }

        internal virtual LayerType GetLayerType()
        {
            return _layerType;
        }

        internal virtual MatteType GetMatteType()
        {
            return _matteType;
        }

        internal virtual long ParentId { get; }

        internal virtual IList<object> Shapes => _shapes;

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
            StringBuilder sb = new StringBuilder();
            sb.Append(prefix).Append(Name).Append("\n");
            Layer parent = _composition.LayerModelForId(ParentId);
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
                foreach (object shape in _shapes)
                {
                    sb.Append(prefix).Append("\t\t").Append(shape).Append("\n");
                }
            }
            return sb.ToString();
        }

        internal class Factory
        {
            internal static Layer NewInstance(LottieComposition composition)
            {
                // TODO: make sure in out keyframes work
                Rect bounds = composition.Bounds;
                return new Layer(new List<object>(), composition, null, -1, LayerType.PreComp, -1, null, new List<Mask>(), AnimatableTransform.Factory.NewInstance(), 0, 0, default(Color), 0, 0, (int)bounds.Width, (int)bounds.Height, new List<IKeyframe<float?>>(), MatteType.None);
            }

            internal static Layer NewInstance(JsonObject json, LottieComposition composition)
            {
                string layerName = json.GetNamedString("nm");
                string refId = json.GetNamedString("refId", string.Empty);
                long layerId = (long)json.GetNamedNumber("ind");
                int solidWidth = 0;
                int solidHeight = 0;
                Color solidColor;
                int preCompWidth = 0;
                int preCompHeight = 0;
                LayerType layerType;
                int layerTypeInt = (int)json.GetNamedNumber("ty", -1);
                if (layerTypeInt < (int)LayerType.Unknown)
                {
                    layerType = (LayerType)layerTypeInt;
                }
                else
                {
                    layerType = LayerType.Unknown;
                }

                long parentId = (long)json.GetNamedNumber("parent", -1);

                if (layerType == LayerType.Solid)
                {
                    solidWidth = (int)(json.GetNamedNumber("sw") * composition.DpScale);
                    solidHeight = (int)(json.GetNamedNumber("sh") * composition.DpScale);
                    solidColor = Utils.GetSolidColorBrush(json.GetNamedString("sc"));
                    Debug.WriteLine("\tSolid=" + string.Format("{0:X}", solidColor) + " " + solidWidth + "x" + solidHeight + " " + composition.Bounds, Tag);
                }

                AnimatableTransform transform = AnimatableTransform.Factory.NewInstance(json.GetNamedObject("ks"), composition);
                MatteType matteType = (MatteType)(int)json.GetNamedNumber("tt", 0);
                IList<object> shapes = new List<object>();
                IList<Mask> masks = new List<Mask>();
                IList<IKeyframe<float?>> inOutKeyframes = new List<IKeyframe<float?>>();
                var jsonMasks = json.GetNamedArray("masksProperties", null);
                if (jsonMasks != null)
                {
                    for (int i = 0; i < jsonMasks.Count; i++)
                    {
                        Mask mask = Mask.Factory.NewMask(jsonMasks[i].GetObject(), composition);
                        masks.Add(mask);
                    }
                }

                var shapesJson = json.GetNamedArray("shapes", null);
                if (shapesJson != null)
                {
                    for (int i = 0; i < shapesJson.Count; i++)
                    {
                        object shape = ShapeGroup.ShapeItemWithJson(shapesJson[i].GetObject(), composition);
                        if (shape != null)
                        {
                            shapes.Add(shape);
                        }
                    }
                }

                float timeStretch = (float)json.GetNamedNumber("sr", 1.0);
                float startFrame = (float)json.GetNamedNumber("st");
                float frames = composition.DurationFrames;
                float startProgress = startFrame / frames;

                if (layerType == LayerType.PreComp)
                {
                    preCompWidth = (int)(json.GetNamedNumber("w") * composition.DpScale);
                    preCompHeight = (int)(json.GetNamedNumber("h") * composition.DpScale);
                }

                float inFrame = (float)json.GetNamedNumber("ip");
                float outFrame = (float)json.GetNamedNumber("op");

                // Before the in frame
                if (inFrame > 0)
                {
                    Keyframe<float?> preKeyframe = new Keyframe<float?>(composition, 0f, 0f, null, 0f, inFrame);
                    inOutKeyframes.Add(preKeyframe);
                }

                // The + 1 is because the animation should be visible on the out frame itself.
                outFrame = outFrame > 0 ? outFrame : composition.EndFrame + 1;
                Keyframe<float?> visibleKeyframe = new Keyframe<float?>(composition, 1f, 1f, null, inFrame, outFrame);
                inOutKeyframes.Add(visibleKeyframe);

                if (outFrame <= composition.DurationFrames)
                {
                    Keyframe<float?> outKeyframe = new Keyframe<float?>(composition, 0f, 0f, null, outFrame, composition.EndFrame);
                    inOutKeyframes.Add(outKeyframe);
                }

                return new Layer(shapes, composition, layerName, layerId, layerType, parentId, refId, masks, transform, solidWidth, solidHeight, solidColor, timeStretch, startProgress, preCompWidth, preCompHeight, inOutKeyframes, matteType);
            }
        }
    }
}