using System;
using System.Collections.Generic;
using System.Numerics;
using LottieUWP.Model.Animatable;
using LottieUWP.Utils;
using Newtonsoft.Json;

namespace LottieUWP.Model.Content
{
    public class ShapeData
    {
        private readonly List<CubicCurveData> _curves = new List<CubicCurveData>();
        private Vector2 _initialPoint;
        private bool _closed;

        private ShapeData(Vector2 initialPoint, bool closed, List<CubicCurveData> curves)
        {
            _initialPoint = initialPoint;
            _closed = closed;
            _curves.AddRange(curves);
        }

        internal ShapeData()
        {
        }

        private void SetInitialPoint(float x, float y)
        {
            if (_initialPoint == null)
            {
                _initialPoint = new Vector2();
            }
            _initialPoint.X = x;
            _initialPoint.Y = y;
        }

        internal virtual Vector2 InitialPoint => _initialPoint;

        internal virtual bool Closed => _closed;

        internal virtual List<CubicCurveData> Curves => _curves;

        internal virtual void InterpolateBetween(ShapeData shapeData1, ShapeData shapeData2, float percentage)
        {
            if (_initialPoint == null)
            {
                _initialPoint = new Vector2();
            }
            _closed = shapeData1.Closed || shapeData2.Closed;

            if (_curves.Count > 0 && _curves.Count != shapeData1.Curves.Count && _curves.Count != shapeData2.Curves.Count)
            {
                throw new InvalidOperationException("Curves must have the same number of control points. This: " + Curves.Count + "\tShape 1: " + shapeData1.Curves.Count + "\tShape 2: " + shapeData2.Curves.Count);
            }
            if (_curves.Count == 0)
            {
                for (var i = shapeData1.Curves.Count - 1; i >= 0; i--)
                {
                    _curves.Add(new CubicCurveData());
                }
            }

            var initialPoint1 = shapeData1.InitialPoint;
            var initialPoint2 = shapeData2.InitialPoint;
            
            SetInitialPoint(MiscUtils.Lerp(initialPoint1.X, initialPoint2.X, percentage), MiscUtils.Lerp(initialPoint1.Y, initialPoint2.Y, percentage));

            for (var i = _curves.Count - 1; i >= 0; i--)
            {
                var curve1 = shapeData1.Curves[i];
                var curve2 = shapeData2.Curves[i];

                var cp11 = curve1.ControlPoint1;
                var cp21 = curve1.ControlPoint2;
                var vertex1 = curve1.Vertex;

                var cp12 = curve2.ControlPoint1;
                var cp22 = curve2.ControlPoint2;
                var vertex2 = curve2.Vertex;

                _curves[i].SetControlPoint1(MiscUtils.Lerp(cp11.X, cp12.X, percentage), MiscUtils.Lerp(cp11.Y, cp12.Y, percentage));
                _curves[i].SetControlPoint2(MiscUtils.Lerp(cp21.X, cp22.X, percentage), MiscUtils.Lerp(cp21.Y, cp22.Y, percentage));
                _curves[i].SetVertex(MiscUtils.Lerp(vertex1.X, vertex2.X, percentage), MiscUtils.Lerp(vertex1.Y, vertex2.Y, percentage));
            }
        }

        public override string ToString()
        {
            return "ShapeData{" + "numCurves=" + _curves.Count + "closed=" + _closed + '}';
        }

        internal class Factory : IAnimatableValueFactory<ShapeData>
        {
            internal static readonly Factory Instance = new Factory();

            public ShapeData ValueFromObject(JsonReader reader, float scale)
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
                if (pointsArray.Count == 0)
                {
                    return new ShapeData(new Vector2(), false, new List<CubicCurveData>());
                }

                var length = pointsArray.Count;
                var vertex = pointsArray[0];
                var initialPoint = vertex;
                var curves = new List<CubicCurveData>(length);

                for (var i = 1; i < length; i++)
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
}