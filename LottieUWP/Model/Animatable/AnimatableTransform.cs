using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Windows.Data.Json;
using LottieUWP.Animation;
using LottieUWP.Animation.Content;
using LottieUWP.Animation.Keyframe;
using LottieUWP.Model.Content;
using LottieUWP.Model.Layer;

namespace LottieUWP.Model.Animatable
{
    internal class AnimatableTransform : IModifierContent, IContentModel
    {
        private AnimatableTransform(AnimatablePathValue anchorPoint, IAnimatableValue<Vector2?> position, AnimatableScaleValue scale, AnimatableFloatValue rotation, AnimatableIntegerValue opacity, AnimatableFloatValue startOpacity, AnimatableFloatValue endOpacity)
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

            internal static AnimatableTransform NewInstance(JsonObject json, LottieComposition composition)
            {
                AnimatablePathValue anchorPoint;
                IAnimatableValue<Vector2?> position = null;
                AnimatableScaleValue scale;
                AnimatableFloatValue rotation = null;
                AnimatableIntegerValue opacity;
                AnimatableFloatValue startOpacity = null;
                AnimatableFloatValue endOpacity = null;
                var anchorJson = json.GetNamedObject("a", null);
                if (anchorJson != null)
                {
                    anchorPoint = new AnimatablePathValue(anchorJson["k"], composition);
                }
                else
                {
                    // Cameras don't have an anchor point property. Although we don't support them, at least
                    // we won't crash.
                    Debug.WriteLine("Layer has no transform property. You may be using an unsupported " + "layer type such as a camera.", LottieLog.Tag);
                    anchorPoint = new AnimatablePathValue();
                }

                var positionJson = json.GetNamedObject("p", null);
                if (positionJson != null)
                {
                    position = AnimatablePathValue.CreateAnimatablePathOrSplitDimensionPath(positionJson, composition);
                }
                else
                {
                    ThrowMissingTransform("position");
                }

                var scaleJson = json.GetNamedObject("s", null);
                if (scaleJson != null)
                {
                    scale = AnimatableScaleValue.Factory.NewInstance(scaleJson, composition);
                }
                else
                {
                    // Repeaters have start/end opacity instead of opacity 
                    scale = new AnimatableScaleValue(new List<IKeyframe<ScaleXy>>(), new ScaleXy());
                }

                var rotationJson = json.GetNamedObject("r", null);
                if (rotationJson == null)
                {
                    rotationJson = json.GetNamedObject("rz", null);
                }
                if (rotationJson != null)
                {
                    rotation = AnimatableFloatValue.Factory.NewInstance(rotationJson, composition, false);
                }
                else
                {
                    ThrowMissingTransform("rotation");
                }

                var opacityJson = json.GetNamedObject("o", null);
                if (opacityJson != null)
                {
                    opacity = AnimatableIntegerValue.Factory.NewInstance(opacityJson, composition);
                }
                else
                {
                    // Somehow some community animations don't have opacity in the transform.
                    opacity = new AnimatableIntegerValue(new List<IKeyframe<int?>>(), 100);
                }

                var startOpacityJson = json.GetNamedObject("so", null);
                if (startOpacityJson != null)
                {
                    startOpacity = AnimatableFloatValue.Factory.NewInstance(startOpacityJson, composition, false);
                }
                
                var endOpacityJson = json.GetNamedObject("eo", null);
                if (endOpacityJson != null)
                {
                    endOpacity = AnimatableFloatValue.Factory.NewInstance(endOpacityJson, composition, false);
                }

                return new AnimatableTransform(anchorPoint, position, scale, rotation, opacity, startOpacity, endOpacity);
            }

            private static void ThrowMissingTransform(string missingProperty)
            {
                throw new System.ArgumentException("Missing transform for " + missingProperty);
            }
        }

        internal virtual AnimatablePathValue AnchorPoint { get; }

        internal virtual IAnimatableValue<Vector2?> Position { get; }

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