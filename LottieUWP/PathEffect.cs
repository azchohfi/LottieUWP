using Windows.UI.Xaml.Shapes;

namespace LottieUWP
{
    public abstract class PathEffect
    {
        public abstract void Apply(Shape shape, Paint paint);
    }
}