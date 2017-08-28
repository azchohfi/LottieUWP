using System.Collections.Generic;
using System.Linq;
using Windows.Data.Json;
using LottieUWP.Model.Content;

namespace LottieUWP.Model
{
    internal class FontCharacter
    {
        internal static int HashFor(char character, string fontFamily, string style)
        {
            var result = 0;
            result = 31 * result + character;
            result = 31 * result + fontFamily.GetHashCode();
            result = 31 * result + style.GetHashCode();
            return result;
        }

        private readonly char _character;
        private readonly string _fontFamily;

        private FontCharacter(List<ShapeGroup> shapes, char character, int size, double width, string style, string fontFamily)
        {
            Shapes = shapes;
            _character = character;
            _size = size;
            Width = width;
            _style = style;
            _fontFamily = fontFamily;
        }

        internal List<ShapeGroup> Shapes { get; }

        private int _size;

        internal double Width { get; }

        private readonly string _style;

        internal static class Factory
        {
            internal static FontCharacter NewInstance(JsonObject json, LottieComposition composition)
            {
                var character = json.GetNamedString("ch").ElementAt(0);
                var size = (int)json.GetNamedNumber("size", 0);
                var width = json.GetNamedNumber("w", 0);
                var style = json.GetNamedString("style", "");
                var fontFamily = json.GetNamedString("fFamily", "");
                var shapesJson = json.GetNamedObject("data", null)?.GetNamedArray("shapes", null);
                var shapes = new List<ShapeGroup>();
                if (shapesJson != null)
                {
                    shapes = new List<ShapeGroup>(shapesJson.Count);
                    for (uint i = 0; i < shapesJson.Count; i++)
                    {
                        shapes.Add((ShapeGroup)ShapeGroup.ShapeItemWithJson(shapesJson.GetObjectAt(i), composition));
                    }
                }
                return new FontCharacter(shapes, character, size, width, style, fontFamily);
            }
        }

        public override int GetHashCode()
        {
            return HashFor(_character, _fontFamily, _style);
        }
    }
}
