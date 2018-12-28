using System.Collections.Generic;
using LottieUWP.Model.Content;

namespace LottieUWP.Parser
{
    static class ShapeGroupParser
    {
        internal static ShapeGroup Parse(JsonReader reader, LottieComposition composition)
        {
            string name = null;
            List<IContentModel> items = new List<IContentModel>();
            bool hidden = false;

            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "nm":
                        name = reader.NextString();
                        break;
                    case "hd":
                        hidden = reader.NextBoolean();
                        break;
                    case "it":
                        reader.BeginArray();
                        while (reader.HasNext())
                        {
                            IContentModel newItem = ContentModelParser.Parse(reader, composition);
                            if (newItem != null)
                            {
                                items.Add(newItem);
                            }
                        }
                        reader.EndArray();
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }

            return new ShapeGroup(name, items, hidden);
        }
    }
}
