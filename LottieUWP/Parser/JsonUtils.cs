using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI;
using Newtonsoft.Json;

namespace LottieUWP.Parser
{
    internal static class JsonUtils
    {
        /// <summary>
        /// [r,g,b]
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        internal static Color JsonToColor(JsonReader reader)
        {
            reader.BeginArray();
            var r = (byte)(reader.NextDouble() * 255);
            var g = (byte)(reader.NextDouble() * 255);
            var b = (byte)(reader.NextDouble() * 255);
            while (reader.HasNext())
            {
                reader.SkipValue();
            }
            reader.EndArray();
            return Color.FromArgb(255, r, g, b);
        }

        internal static List<Vector2> JsonToPoints(JsonReader reader, float scale)
        {
            List<Vector2> points = new List<Vector2>();

            reader.BeginArray();
            while (reader.Peek() == JsonToken.StartArray)
            {
                reader.BeginArray();
                points.Add(JsonToPoint(reader, scale));
                reader.EndArray();
            }
            reader.EndArray();
            return points;
        }

        internal static Vector2 JsonToPoint(JsonReader reader, float scale)
        {
            switch (reader.Peek())
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                    return JsonNumbersToPoint(reader, scale);
                case JsonToken.StartArray: return JsonArrayToPoint(reader, scale);
                case JsonToken.StartObject: return JsonObjectToPoint(reader, scale);
                default: throw new ArgumentException("Unknown point starts with " + reader.Peek());
            }
        }

        private static Vector2 JsonNumbersToPoint(JsonReader reader, float scale)
        {
            float x = reader.NextDouble();
            float y = reader.NextDouble();
            while (reader.HasNext())
            {
                reader.SkipValue();
            }
            return new Vector2(x * scale, y * scale);
        }

        private static Vector2 JsonArrayToPoint(JsonReader reader, float scale)
        {
            float x;
            float y;
            reader.BeginArray();
            x = reader.NextDouble();
            y = reader.NextDouble();
            while (reader.Peek() != JsonToken.EndArray)
            {
                reader.SkipValue();
            }
            reader.EndArray();
            return new Vector2(x * scale, y * scale);
        }

        private static Vector2 JsonObjectToPoint(JsonReader reader, float scale)
        {
            float x = 0f;
            float y = 0f;
            reader.BeginObject();
            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "x":
                        x = ValueFromObject(reader);
                        break;
                    case "y":
                        y = ValueFromObject(reader);
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }
            reader.EndObject();
            return new Vector2(x * scale, y * scale);
        }

        internal static float ValueFromObject(JsonReader reader)
        {
            JsonToken token = reader.Peek();
            switch (token)
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                    return reader.NextDouble();
                case JsonToken.StartArray:
                    reader.BeginArray();
                    float val = reader.NextDouble();
                    while (reader.HasNext())
                    {
                        reader.SkipValue();
                    }
                    reader.EndArray();
                    return val;
                default:
                    throw new ArgumentException("Unknown value for token of type " + token);
            }
        }
    }
}