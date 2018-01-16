using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LottieUWP.Model;
using LottieUWP.Model.Content;
using Newtonsoft.Json;

namespace LottieUWP.Parser
{
    public class ShapeDataParser : IValueParser<ShapeData>
    {
        public static readonly ShapeDataParser Instance = new ShapeDataParser();

        public ShapeData Parse(JsonReader reader, float scale)
        {
            // Sometimes the points data is in a array of length 1. Sometimes the data is at the top 
            // level. 
            if (reader.Peek() == JsonToken.StartArray)
            {
                reader.BeginArray();
            }

            bool closed = false;
            List<Vector2> pointsArray = null;
            List<Vector2> inTangents = null;
            List<Vector2> outTangents = null;
            reader.BeginObject();

            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "c":
                        closed = reader.NextBoolean();
                        break;
                    case "v":
                        pointsArray = JsonUtils.JsonToPoints(reader, scale);
                        break;
                    case "i":
                        inTangents = JsonUtils.JsonToPoints(reader, scale);
                        break;
                    case "o":
                        outTangents = JsonUtils.JsonToPoints(reader, scale);
                        break;
                }
            }

            reader.EndObject();

            if (reader.Peek() == JsonToken.EndArray)
            {
                reader.EndArray();
            }

            if (pointsArray == null || inTangents == null || outTangents == null)
            {
                throw new ArgumentException("Shape data was missing information.");
            }

            if (!pointsArray.Any())
            {
                return new ShapeData(new Vector2(), false, new List<CubicCurveData>());
            }

            int length = pointsArray.Count;
            var vertex = pointsArray[0];
            var initialPoint = vertex;
            List<CubicCurveData> curves = new List<CubicCurveData>(length);

            for (int i = 1; i < length; i++)
            {
                vertex = pointsArray[i];
                var previousVertex = pointsArray[i - 1];
                var cp1 = outTangents[i - 1];
                var cp2 = inTangents[i];
                var shapeCp1 = previousVertex + cp1;
                var shapeCp2 = vertex + cp2;
                curves.Add(new CubicCurveData(shapeCp1, shapeCp2, vertex));
            }

            if (closed)
            {
                vertex = pointsArray[0];
                var previousVertex = pointsArray[length - 1];
                var cp1 = outTangents[length - 1];
                var cp2 = inTangents[0];

                var shapeCp1 = previousVertex + cp1;
                var shapeCp2 = vertex + cp2;

                curves.Add(new CubicCurveData(shapeCp1, shapeCp2, vertex));
            }
            return new ShapeData(initialPoint, closed, curves);
        }
    }
}
