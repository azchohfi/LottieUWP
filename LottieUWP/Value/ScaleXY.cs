using LottieUWP.Model.Animatable;
using Newtonsoft.Json;

namespace LottieUWP.Value
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

            public ScaleXy ValueFromObject(JsonReader reader, float scale)
            {
                var isArray = reader.Peek() == JsonToken.StartArray;
                if (isArray)
                {
                    reader.BeginArray();
                }
                var sx = reader.NextDouble();
                var sy = reader.NextDouble();
                while (reader.HasNext())
                {
                    reader.SkipValue();
                }
                if (isArray)
                {
                    reader.EndArray();
                }
                return new ScaleXy(sx / 100f * scale, sy / 100f * scale);
            }
        }
    }
}