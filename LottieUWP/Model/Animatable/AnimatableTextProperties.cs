namespace LottieUWP.Model.Animatable
{
    public class AnimatableTextProperties
    {
        internal readonly AnimatableColorValue _color;
        internal readonly AnimatableColorValue _stroke;
        internal readonly AnimatableFloatValue _strokeWidth;
        internal readonly AnimatableFloatValue _tracking;

        public AnimatableTextProperties(AnimatableColorValue color, AnimatableColorValue stroke, AnimatableFloatValue strokeWidth, AnimatableFloatValue tracking)
        {
            _color = color;
            _stroke = stroke;
            _strokeWidth = strokeWidth;
            _tracking = tracking;
        }
    }
}
