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

        private FontCharacter(List<ShapeGroup> shapes, char character, int size, double width, string style, string fontFamily)
        {
            Shapes = shapes;
            _character = character;
            _size = size;
            Width = width;
            _style = style;
            _fontFamily = fontFamily;
        }

        public List<ShapeGroup> Shapes { get; }

        private int _size;

        public double Width { get; }

        private readonly string _style;

        public static class Factory
        {
            public static FontCharacter NewInstance(JsonReader reader, LottieComposition composition)
            {
                char character = '\0';
                int size = 0;
                double width = 0;
                string style = null;
                string fontFamily = null;
                List<ShapeGroup> shapes = new List<ShapeGroup>();

                reader.BeginObject();
                while (reader.HasNext())
                {
                    switch (reader.NextName())
                    {
                        case "ch":
                            character = reader.NextString()[0];
                            break;
                        case "size":
                            size = reader.NextInt();
                            break;
                        case "w":
                            width = reader.NextDouble();
                            break;
                        case "style":
                            style = reader.NextString();
                            break;
                        case "fFamily":
                            fontFamily = reader.NextString();
                            break;
                        case "data":
                            reader.BeginObject();
                            while (reader.HasNext())
                            {
                                if ("shapes".Equals(reader.NextString()))
                                {
                                    reader.BeginArray();
                                    while (reader.HasNext())
                                    {
                                        shapes.Add((ShapeGroup)ShapeGroup.ShapeItemWithJson(reader, composition));
                                    }
                                    reader.EndArray();
                                }
                                else
                                {
                                    reader.SkipValue();
                                }
                            }
                            reader.EndObject();
                            break;
                        default:
                            reader.SkipValue();
                            break;
                    }
                }
                reader.EndObject();

                return new FontCharacter(shapes, character, size, width, style, fontFamily);
            }
        }

        public override int GetHashCode()
        {
            return HashFor(_character, _fontFamily, _style);
        }
    }
}
