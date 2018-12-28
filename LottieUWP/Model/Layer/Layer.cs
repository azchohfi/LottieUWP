using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Windows.UI;
using LottieUWP.Value;
using LottieUWP.Model.Animatable;
using LottieUWP.Model.Content;

namespace LottieUWP.Model.Layer
{
    public class Layer
    {
        public enum LayerType
        {
            PreComp,
            Solid,
            Image,
            Null,
            Shape,
            Text,
            Unknown
        }

        public enum MatteType
        {
            None,
            Add,
            Invert,
            Unknown
        }

        private readonly LayerType _layerType;
        private readonly MatteType _matteType;

        public Layer(List<IContentModel> shapes, LottieComposition composition, string layerName, long layerId, LayerType layerType, long parentId, string refId, List<Mask> masks, AnimatableTransform transform, int solidWidth, int solidHeight, Color solidColor, float timeStretch, float startFrame, int preCompWidth, int preCompHeight, AnimatableTextFrame text, AnimatableTextProperties textProperties, List<Keyframe<float?>> inOutKeyframes, MatteType matteType, AnimatableFloatValue timeRemapping, bool hidden)
        {
            Shapes = shapes;
            Composition = composition;
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
            IsHidden = hidden;
        }

        internal LottieComposition Composition { get; }

        internal float TimeStretch { get; }

        internal float StartFrame { get; }

        internal float StartProgress => StartFrame / Composition.DurationFrames;

        internal List<Keyframe<float?>> InOutKeyframes { get; }

        internal long Id { get; }

        internal string Name { get; }

        internal string RefId { get; }

        internal int PreCompWidth { get; }

        internal int PreCompHeight { get; }

        internal AnimatableTextFrame Text { get; }

        internal AnimatableTextProperties TextProperties { get; }

        internal AnimatableFloatValue TimeRemapping { get; }

        internal List<Mask> Masks { get; }

        internal LayerType GetLayerType()
        {
            return _layerType;
        }

        internal MatteType GetMatteType()
        {
            return _matteType;
        }

        internal long ParentId { get; }

        internal List<IContentModel> Shapes { get; }

        internal AnimatableTransform Transform { get; }

        internal Color SolidColor { get; }

        internal int SolidHeight { get; }

        internal int SolidWidth { get; }

        internal bool IsHidden { get; }

        public override string ToString()
        {
            return ToString("");
        }

        internal string ToString(string prefix)
        {
            var sb = new StringBuilder();
            sb.Append(prefix).Append(Name).Append("\n");
            var parent = Composition.LayerModelForId(ParentId);
            if (parent != null)
            {
                sb.Append("\t\tParents: ").Append(parent.Name);
                parent = Composition.LayerModelForId(parent.ParentId);
                while (parent != null)
                {
                    sb.Append("->").Append(parent.Name);
                    parent = Composition.LayerModelForId(parent.ParentId);
                }
                sb.Append(prefix).Append("\n");
            }
            if (Masks.Count > 0)
            {
                sb.Append(prefix).Append("\tMasks: ").Append(Masks.Count).Append("\n");
            }
            if (SolidWidth != 0 && SolidHeight != 0)
            {
                sb.Append(prefix).Append("\tBackground: ").Append(string.Format(CultureInfo.InvariantCulture, "{0}x{1} {2}\n", SolidWidth, SolidHeight, SolidColor));
            }
            if (Shapes.Count > 0)
            {
                sb.Append(prefix).Append("\tShapes:\n");
                foreach (var shape in Shapes)
                {
                    sb.Append(prefix).Append("\t\t").Append(shape).Append("\n");
                }
            }
            return sb.ToString();
        }
    }
}