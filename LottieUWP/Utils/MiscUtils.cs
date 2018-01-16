﻿using System;
using System.Collections.Generic;
using LottieUWP.Model;
using LottieUWP.Model.Content;

namespace LottieUWP.Utils
{
    internal static class MiscUtils
    {
        internal static void GetPathFromData(ShapeData shapeData, Path outPath)
        {
            outPath.Reset();
            var initialPoint = shapeData.InitialPoint;
            outPath.MoveTo(initialPoint.X, initialPoint.Y);
            var currentPoint = initialPoint;
            for (var i = 0; i < shapeData.Curves.Count; i++)
            {
                var curveData = shapeData.Curves[i];
                var cp1 = curveData.ControlPoint1;
                var cp2 = curveData.ControlPoint2;
                var vertex = curveData.Vertex;

                if (cp1.Equals(currentPoint) && cp2.Equals(vertex))
                {
                    outPath.LineTo(vertex.X, vertex.Y);
                }
                else
                {
                    outPath.CubicTo(cp1.X, cp1.Y, cp2.X, cp2.Y, vertex.X, vertex.Y);
                }
                currentPoint.X = vertex.X;
                currentPoint.Y = vertex.Y;
            }
            if (shapeData.Closed)
            {
                outPath.Close();
            }
        }

        internal static float Lerp(float a, float b, float percentage)
        {
            return a + percentage * (b - a);
        }

        internal static double Lerp(double a, double b, double percentage)
        {
            return a + percentage * (b - a);
        }

        internal static int Lerp(int a, int b, float percentage)
        {
            return (int)(a + percentage * (b - a));
        }

        internal static int FloorMod(float x, float y)
        {
            return FloorMod((int)x, (int)y);
        }

        private static int FloorMod(int x, int y)
        {
            return x - FloorDiv(x, y) * y;
        }

        private static int FloorDiv(int x, int y)
        {
            var r = x / y;
            // if the signs are different and modulo not zero, round down
            if ((x ^ y) < 0 && r * y != x)
            {
                r--;
            }
            return r;
        }

        internal static float Clamp(float number, float min, float max)
        {
            return Math.Max(min, Math.Min(max, number));
        }

        internal static double Clamp(double number, double min, double max)
        {
            return Math.Max(min, Math.Min(max, number));
        }

        /// <summary>
        /// Helper method for any <see cref="IKeyPathElementContent"/> that will check if the content 
        /// fully matches the keypath then will add itself as the final key, resolve it, and add 
        /// it to the accumulator list. 
        /// 
        /// Any <see cref="IKeyPathElementContent"/> should call through to this as its implementation of 
        /// <see cref="IKeyPathElement.ResolveKeyPath"/>.
        /// </summary>
        /// <param name="keyPath"></param>
        /// <param name="depth"></param>
        /// <param name="accumulator"></param>
        /// <param name="currentPartialKeyPath"></param>
        /// <param name="content"></param>
        public static void ResolveKeyPath(KeyPath keyPath, int depth, List<KeyPath> accumulator, KeyPath currentPartialKeyPath, IKeyPathElementContent content)
        {
            if (keyPath.FullyResolvesTo(content.Name, depth))
            {
                currentPartialKeyPath = currentPartialKeyPath.AddKey(content.Name);
                accumulator.Add(currentPartialKeyPath.Resolve(content));
            }
        }
    }
}