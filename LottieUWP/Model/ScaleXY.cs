using Windows.Data.Json;
using LottieUWP.Model.Animatable;

namespace LottieUWP.Model
{
    internal class ScaleXy
    {
        internal ScaleXy(float sx, float sy)
        {
            ScaleX = sx;
            ScaleY = sy;
        }

        internal ScaleXy() : this(1f, 1f)
        {
        }

        internal virtual float ScaleX { get; }

        internal virtual float ScaleY { get; }

        public override string ToString()
        {
            return ScaleX + "x" + ScaleY;
        }

        internal class Factory : IAnimatableValueFactory<ScaleXy>
        {
            internal static readonly Factory Instance = new Factory();

            public ScaleXy ValueFromObject(IJsonValue @object, float scale)
            {
                var array = @object.GetArray();
                return new ScaleXy((float)array.GetNumberAt(0, 1) / 100f * scale, (float)array.GetNumberAt(1, 1) / 100f * scale);
            }
        }
    }
}