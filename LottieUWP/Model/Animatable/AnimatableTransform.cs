using System.Diagnostics;
using System.Numerics;
using LottieUWP.Animation.Content;
using LottieUWP.Animation.Keyframe;
using LottieUWP.Model.Content;
using LottieUWP.Model.Layer;
using LottieUWP.Value;
using Newtonsoft.Json;

namespace LottieUWP.Model.Animatable
{
    internal class AnimatableTransform : IModifierContent, IContentModel
    {
        private AnimatableTransform(AnimatablePathValue anchorPoint, IAnimatableValue<Vector2?, Vector2?> position, AnimatableScaleValue scale, AnimatableFloatValue rotation, AnimatableIntegerValue opacity, AnimatableFloatValue startOpacity, AnimatableFloatValue endOpacity)
        {
            AnchorPoint = anchorPoint;
            Position = position;
            Scale = scale;
            Rotation = rotation;
            Opacity = opacity;
            StartOpacity = startOpacity;
            EndOpacity = endOpacity;
        }

        internal static class Factory
        {
            internal static AnimatableTransform NewInstance()
            {
                var anchorPoint = new AnimatablePathValue();
                var position = new AnimatablePathValue();
                var scale = AnimatableScaleValue.Factory.NewInstance();
                var rotation = AnimatableFloatValue.Factory.NewInstance();
                var opacity = AnimatableIntegerValue.Factory.NewInstance();
                var startOpacity = AnimatableFloatValue.Factory.NewInstance();
                var endOpacity = AnimatableFloatValue.Factory.NewInstance();
                return new AnimatableTransform(anchorPoint, position, scale, rotation, opacity, startOpacity, endOpacity);
            }

            internal static AnimatableTransform NewInstance(JsonReader reader, LottieComposition composition)
            {
                AnimatablePathValue anchorPoint = null;
                IAnimatableValue<Vector2?, Vector2?> position = null;
                AnimatableScaleValue scale = null;
                AnimatableFloatValue rotation = null;
                AnimatableIntegerValue opacity = null;
                AnimatableFloatValue startOpacity = null;
                AnimatableFloatValue endOpacity = null;

                bool isObject = reader.Peek() == JsonToken.StartObject;
                if (isObject)
                {
                    reader.BeginObject();
                }
                while (reader.HasNext())
                {
                    switch (reader.NextName())
                    {
                        case "a":
                            reader.BeginObject();
                            while (reader.HasNext())
                            {
                                if (reader.NextString().Equals("k"))
                                {
                                    anchorPoint = new AnimatablePathValue(reader, composition);
                                }
                                else
                                {
                                    reader.SkipValue();
                                }
                            }
                            reader.EndObject();
                            break;
                        case "p":
                            position =
                                AnimatablePathValue.CreateAnimatablePathOrSplitDimensionPath(reader, composition);
                            break;
                        case "s":
                            scale = AnimatableScaleValue.Factory.NewInstance(reader, composition);
                            break;
                        case "rz":
                            composition.AddWarning("Lottie doesn't support 3D layers.");
                            rotation = AnimatableFloatValue.Factory.NewInstance(reader, composition, false);
                            break;
                        case "r":
                            rotation = AnimatableFloatValue.Factory.NewInstance(reader, composition, false);
                            break;
                        case "o":
                            opacity = AnimatableIntegerValue.Factory.NewInstance(reader, composition);
                            break;
                        case "so":
                            startOpacity =
                                AnimatableFloatValue.Factory.NewInstance(reader, composition, false);
                            break;
                        case "eo":
                            endOpacity =
                                AnimatableFloatValue.Factory.NewInstance(reader, composition, false);
                            break;
                        default:
                            reader.SkipValue();
                            break;
                    }
                }
                if (isObject)
                {
                    reader.EndObject();
                }

                if (anchorPoint == null)
                {
                    // Cameras don't have an anchor point property. Although we don't support them, at least
                    // we won't crash.
                    Debug.WriteLine("Layer has no transform property. You may be using an unsupported " + "layer type such as a camera.", LottieLog.Tag);
                    anchorPoint = new AnimatablePathValue();
                }

                if (scale == null)
                {
                    // Somehow some community animations don't have opacity in the transform.
                    scale = new AnimatableScaleValue(new ScaleXy(1f, 1f));
                }

                if (opacity == null)
                {
                    // Repeaters have start/end opacity instead of opacity 
                    opacity = new AnimatableIntegerValue(100);
                }

                return new AnimatableTransform(anchorPoint, position, scale, rotation, opacity, startOpacity, endOpacity);
            }
        }

        internal virtual AnimatablePathValue AnchorPoint { get; }

        internal virtual IAnimatableValue<Vector2?, Vector2?> Position { get; }

        internal virtual AnimatableScaleValue Scale { get; }

        internal virtual AnimatableFloatValue Rotation { get; }

        internal virtual AnimatableIntegerValue Opacity { get; }

        // Used for repeaters 
        internal virtual AnimatableFloatValue StartOpacity { get; } 
        internal virtual AnimatableFloatValue EndOpacity { get; }

        public virtual TransformKeyframeAnimation CreateAnimation()
        {
            return new TransformKeyframeAnimation(this);
        }

        public IContent ToContent(LottieDrawable drawable, BaseLayer layer)
        {
            return null;
        }
    }
}