using System;
using System.Collections.Generic;
using System.Numerics;
using LottieUWP.Utils;

namespace LottieUWP.Model.Content
{
    public class ShapeData
    {
        private readonly List<CubicCurveData> _curves = new List<CubicCurveData>();
        private Vector2 _initialPoint;
        private bool _closed;

        public ShapeData(Vector2 initialPoint, bool closed, List<CubicCurveData> curves)
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

        internal Vector2 InitialPoint => _initialPoint;

        internal bool Closed => _closed;

        internal List<CubicCurveData> Curves => _curves;

        internal void InterpolateBetween(ShapeData shapeData1, ShapeData shapeData2, float percentage)
        {
            if (_initialPoint == null)
            {
                _initialPoint = new Vector2();
            }
            _closed = shapeData1.Closed || shapeData2.Closed;

            if (shapeData1.Curves.Count != shapeData2.Curves.Count)
            {
                LottieLog.Warn($"Curves must have the same number of control points. Shape 1: {shapeData1.Curves.Count}\tShape 2: {shapeData2.Curves.Count}");
            }

            if (_curves.Count == 0)
            {
                int points = Math.Min(shapeData1.Curves.Count, shapeData2.Curves.Count);
                for (int i = 0; i < points; i++)
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
    }
}