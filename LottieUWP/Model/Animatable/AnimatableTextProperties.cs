using Windows.Data.Json;

namespace LottieUWP.Model.Animatable
{
    internal class AnimatableTextProperties
    {
        internal readonly AnimatableColorValue _color;
        internal readonly AnimatableColorValue _stroke;
        internal readonly AnimatableFloatValue _strokeWidth;
        internal readonly AnimatableFloatValue _tracking;

        internal AnimatableTextProperties(AnimatableColorValue color, AnimatableColorValue stroke, AnimatableFloatValue strokeWidth, AnimatableFloatValue tracking)
        {
            _color = color;
            _stroke = stroke;
            _strokeWidth = strokeWidth;
            _tracking = tracking;
        }

        internal static class Factory
        {
            internal static AnimatableTextProperties NewInstance(JsonObject json, LottieComposition composition)
            {
                if (json == null || !json.ContainsKey("a"))
                {
                    return new AnimatableTextProperties(null, null, null, null);
                }

                var animatablePropertiesJson = json.GetNamedObject("a");
                
                var colorJson = animatablePropertiesJson.GetNamedObject("fc", null);
                AnimatableColorValue color = null;
                if (colorJson != null)
                {
                    color = AnimatableColorValue.Factory.NewInstance(colorJson, composition);
                }

                var strokeJson = animatablePropertiesJson.GetNamedObject("sc", null);
                AnimatableColorValue stroke = null;
                if (strokeJson != null)
                {
                    stroke = AnimatableColorValue.Factory.NewInstance(strokeJson, composition);
                }

                var strokeWidthJson = animatablePropertiesJson.GetNamedObject("sw", null);
                AnimatableFloatValue strokeWidth = null;
                if (strokeWidthJson != null)
                {
                    strokeWidth = AnimatableFloatValue.Factory.NewInstance(strokeWidthJson, composition);
                }

                var trackingJson = animatablePropertiesJson.GetNamedObject("t", null);
                AnimatableFloatValue tracking = null;
                if (trackingJson != null)
                {
                    tracking = AnimatableFloatValue.Factory.NewInstance(trackingJson, composition);
                }

                return new AnimatableTextProperties(color, stroke, strokeWidth, tracking);
            }
        }
    }
}
