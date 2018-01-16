namespace LottieUWP.Model.Animatable
{
    public class AnimatableTextProperties
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
            internal static AnimatableTextProperties NewInstance(JsonReader reader, LottieComposition composition)
            {
                AnimatableTextProperties anim = null;
                reader.BeginObject();
                while (reader.HasNext())
                {
                    switch (reader.NextName())
                    {
                        case "a":
                            anim = ParseAnimatableTextProperties(reader, composition);
                            break;
                        default:
                            reader.SkipValue();
                            break;
                    }
                }
                reader.EndObject();
                if (anim == null)
                {
                    // Not sure if this is possible.
                    return new AnimatableTextProperties(null, null, null, null);
                }

                return anim;
            }

            private static AnimatableTextProperties ParseAnimatableTextProperties(JsonReader reader, LottieComposition composition)
            {
                AnimatableColorValue color = null;
                AnimatableColorValue stroke = null;
                AnimatableFloatValue strokeWidth = null;
                AnimatableFloatValue tracking = null;

                reader.BeginObject();
                while (reader.HasNext())
                {
                    switch (reader.NextName())
                    {
                        case "fc":
                            color = AnimatableColorValue.Factory.NewInstance(reader, composition);
                            break;
                        case "sc":
                            stroke = AnimatableColorValue.Factory.NewInstance(reader, composition);
                            break;
                        case "sw":
                            strokeWidth = AnimatableFloatValue.Factory.NewInstance(reader, composition);
                            break;
                        case "t":
                            tracking = AnimatableFloatValue.Factory.NewInstance(reader, composition);
                            break;
                        default:
                            reader.SkipValue();
                            break;
                    }
                }
                reader.EndObject();

                return new AnimatableTextProperties(color, stroke, strokeWidth, tracking);
            }
        }
    }
}
