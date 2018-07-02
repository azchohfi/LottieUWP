using System.Collections.Generic;
using LottieUWP.Model.Content;

namespace LottieUWP.Model
{
    public class FontCharacter
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

        public FontCharacter(List<ShapeGroup> shapes, char character, double size, double width, string style, string fontFamily)
        {
            Shapes = shapes;
            _character = character;
            _size = size;
            Width = width;
            _style = style;
            _fontFamily = fontFamily;
        }

        public List<ShapeGroup> Shapes { get; }

        private double _size;

        public double Width { get; }

        private readonly string _style;

        public override int GetHashCode()
        {
            return HashFor(_character, _fontFamily, _style);
        }
    }
}
