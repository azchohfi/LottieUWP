using System.Collections.Generic;
using Windows.Data.Json;

namespace LottieUWP
{
    internal class ShapeData
    {
        private readonly IList<CubicCurveData> _curves = new List<CubicCurveData>();
        private PointF _initialPoint;
        private bool _closed;

        private ShapeData(PointF initialPoint, bool closed, IList<CubicCurveData> curves)
        {
            _initialPoint = initialPoint;
            _closed = closed;
            ((List<CubicCurveData>)_curves).AddRange(curves);
        }

        internal ShapeData()
        {
        }

        private void SetInitialPoint(float x, float y)
        {
            if (_initialPoint == null)
            {
                _initialPoint = new PointF();
            }
            _initialPoint.X = x;
            _initialPoint.Y = y;
        }

        internal virtual PointF InitialPoint => _initialPoint;

        internal virtual bool Closed => _closed;

        internal virtual IList<CubicCurveData> Curves => _curves;

        internal virtual void InterpolateBetween(ShapeData shapeData1, ShapeData shapeData2, float percentage)
        {
            if (_initialPoint == null)
            {
                _initialPoint = new PointF();
            }
            _closed = shapeData1.Closed || shapeData2.Closed;

            if (_curves.Count > 0 && _curves.Count != shapeData1.Curves.Count && _curves.Count != shapeData2.Curves.Count)
            {
                throw new System.InvalidOperationException("Curves must have the same number of control points. This: " + Curves.Count + "\tShape 1: " + shapeData1.Curves.Count + "\tShape 2: " + shapeData2.Curves.Count);
            }
            if (_curves.Count == 0)
            {
                for (int i = shapeData1.Curves.Count - 1; i >= 0; i--)
                {
                    _curves.Add(new CubicCurveData());
                }
            }

            PointF initialPoint1 = shapeData1.InitialPoint;
            PointF initialPoint2 = shapeData2.InitialPoint;


            SetInitialPoint(MiscUtils.Lerp(initialPoint1.X, initialPoint2.X, percentage), MiscUtils.Lerp(initialPoint1.Y, initialPoint2.Y, percentage));

            for (int i = _curves.Count - 1; i >= 0; i--)
            {
                CubicCurveData curve1 = shapeData1.Curves[i];
                CubicCurveData curve2 = shapeData2.Curves[i];

                PointF cp11 = curve1.ControlPoint1;
                PointF cp21 = curve1.ControlPoint2;
                PointF vertex1 = curve1.Vertex;

                PointF cp12 = curve2.ControlPoint1;
                PointF cp22 = curve2.ControlPoint2;
                PointF vertex2 = curve2.Vertex;

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

            public ShapeData ValueFromObject(IJsonValue @object, float scale)
            {
                JsonObject pointsData = null;
                if (@object.ValueType == JsonValueType.Array)
                {
                    var firstObject = @object.GetArray()[0];
                    if (firstObject.ValueType == JsonValueType.Object && firstObject.GetObject().ContainsKey("v"))
                    {
                        pointsData = firstObject.GetObject();
                    }
                }
                else if (@object.ValueType == JsonValueType.Object && @object.GetObject().ContainsKey("v"))
                {
                    pointsData = @object.GetObject();
                }

                if (pointsData == null)
                {
                    return null;
                }

                var pointsArray = pointsData.GetNamedArray("v", null);
                var inTangents = pointsData.GetNamedArray("i", null);
                var outTangents = pointsData.GetNamedArray("o", null);
                bool closed = pointsData.GetNamedBoolean("c", false);

                if (pointsArray == null || inTangents == null || outTangents == null || pointsArray.Count != inTangents.Count || pointsArray.Count != outTangents.Count)
                {
                    throw new System.InvalidOperationException("Unable to process points array or tangents. " + pointsData);
                }
                if (pointsArray.Count == 0)
                {
                    return new ShapeData(new PointF(), false, new List<CubicCurveData>());
                }

                int length = pointsArray.Count;
                PointF vertex = VertexAtIndex(0, pointsArray);
                vertex.X *= scale;
                vertex.Y *= scale;
                PointF initialPoint = vertex;
                IList<CubicCurveData> curves = new List<CubicCurveData>(length);

                for (int i = 1; i < length; i++)
                {
                    vertex = VertexAtIndex(i, pointsArray);
                    PointF previousVertex = VertexAtIndex(i - 1, pointsArray);
                    PointF cp1 = VertexAtIndex(i - 1, outTangents);
                    PointF cp2 = VertexAtIndex(i, inTangents);
                    PointF shapeCp1 = MiscUtils.AddPoints(previousVertex, cp1);
                    PointF shapeCp2 = MiscUtils.AddPoints(vertex, cp2);

                    shapeCp1.X *= scale;
                    shapeCp1.Y *= scale;
                    shapeCp2.X *= scale;
                    shapeCp2.Y *= scale;
                    vertex.X *= scale;
                    vertex.Y *= scale;

                    curves.Add(new CubicCurveData(shapeCp1, shapeCp2, vertex));
                }

                if (closed)
                {
                    vertex = VertexAtIndex(0, pointsArray);
                    PointF previousVertex = VertexAtIndex(length - 1, pointsArray);
                    PointF cp1 = VertexAtIndex(length - 1, outTangents);
                    PointF cp2 = VertexAtIndex(0, inTangents);

                    PointF shapeCp1 = MiscUtils.AddPoints(previousVertex, cp1);
                    PointF shapeCp2 = MiscUtils.AddPoints(vertex, cp2);

                    if (scale != 1f)
                    {
                        shapeCp1.X *= scale;
                        shapeCp1.Y *= scale;
                        shapeCp2.X *= scale;
                        shapeCp2.Y *= scale;
                        vertex.X *= scale;
                        vertex.Y *= scale;
                    }

                    curves.Add(new CubicCurveData(shapeCp1, shapeCp2, vertex));
                }
                return new ShapeData(initialPoint, closed, curves);
            }

            internal static PointF VertexAtIndex(int idx, JsonArray points)
            {
                if (idx >= points.Count)
                {
                    throw new System.ArgumentException("Invalid index " + idx + ". There are only " + points.Count + " points.");
                }

                var pointArray = points.GetArrayAt((uint)idx);
                var x = pointArray[0];
                var y = pointArray[1];
                return new PointF(x != null ? (float)x.GetNumber() : 0, y != null ? (float)y.GetNumber() : 0);
            }
        }
    }
}