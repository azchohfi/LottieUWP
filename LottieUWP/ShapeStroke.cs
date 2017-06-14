using System.Collections.Generic;
using Windows.Data.Json;
using Windows.UI.Xaml.Media;

namespace LottieUWP
{
    internal class ShapeStroke
    {
        internal sealed class LineCapType
        {
            public static readonly LineCapType Butt = new LineCapType("Butt", InnerEnum.Butt);
            public static readonly LineCapType Round = new LineCapType("Round", InnerEnum.Round);
            public static readonly LineCapType Unknown = new LineCapType("Unknown", InnerEnum.Unknown);

            private static readonly IList<LineCapType> ValueList = new List<LineCapType>();

            static LineCapType()
            {
                ValueList.Add(Butt);
                ValueList.Add(Round);
                ValueList.Add(Unknown);
            }

            public enum InnerEnum
            {
                Butt,
                Round,
                Unknown
            }

            public readonly InnerEnum InnerEnumValue;
            private readonly string _nameValue;
            private readonly int _ordinalValue;
            private static int _nextOrdinal;

            private LineCapType(string name, InnerEnum innerEnum)
            {
                _nameValue = name;
                _ordinalValue = _nextOrdinal++;
                InnerEnumValue = innerEnum;
            }

            internal PenLineCap ToPaintCap()
            {
                switch (InnerEnumValue)
                {
                    case InnerEnum.Butt:
                        return PenLineCap.Flat;
                    case InnerEnum.Round:
                        return PenLineCap.Round;
                    case InnerEnum.Unknown:
                    default:
                        return PenLineCap.Square;
                }
            }

            public static IList<LineCapType> Values()
            {
                return ValueList;
            }

            public int Ordinal()
            {
                return _ordinalValue;
            }

            public override string ToString()
            {
                return _nameValue;
            }

            public static LineCapType ValueOf(string name)
            {
                foreach (var enumInstance in ValueList)
                {
                    if (enumInstance._nameValue == name)
                    {
                        return enumInstance;
                    }
                }
                throw new System.ArgumentException(name);
            }
        }

        internal sealed class LineJoinType
        {
            public static readonly LineJoinType Miter = new LineJoinType("Miter", InnerEnum.Miter);
            public static readonly LineJoinType Round = new LineJoinType("Round", InnerEnum.Round);
            public static readonly LineJoinType Bevel = new LineJoinType("Bevel", InnerEnum.Bevel);

            private static readonly IList<LineJoinType> ValueList = new List<LineJoinType>();

            static LineJoinType()
            {
                ValueList.Add(Miter);
                ValueList.Add(Round);
                ValueList.Add(Bevel);
            }

            public enum InnerEnum
            {
                Miter,
                Round,
                Bevel
            }

            public readonly InnerEnum InnerEnumValue;
            private readonly string _nameValue;
            private readonly int _ordinalValue;
            private static int _nextOrdinal;

            private LineJoinType(string name, InnerEnum innerEnum)
            {
                _nameValue = name;
                _ordinalValue = _nextOrdinal++;
                InnerEnumValue = innerEnum;
            }

            internal PenLineJoin ToPaintJoin()
            {
                switch (InnerEnumValue)
                {
                    case InnerEnum.Bevel:
                        return PenLineJoin.Bevel;
                    case InnerEnum.Miter:
                        return PenLineJoin.Miter;
                    case InnerEnum.Round:
                    default:
                        return PenLineJoin.Round;
                }
            }

            public static IList<LineJoinType> Values()
            {
                return ValueList;
            }

            public int Ordinal()
            {
                return _ordinalValue;
            }

            public override string ToString()
            {
                return _nameValue;
            }

            public static LineJoinType ValueOf(string name)
            {
                foreach (var enumInstance in ValueList)
                {
                    if (enumInstance._nameValue == name)
                    {
                        return enumInstance;
                    }
                }
                throw new System.ArgumentException(name);
            }
        }

        private ShapeStroke(string name, AnimatableFloatValue offset, IList<AnimatableFloatValue> lineDashPattern, AnimatableColorValue color, AnimatableIntegerValue opacity, AnimatableFloatValue width, LineCapType capType, LineJoinType joinType)
        {
            Name = name;
            DashOffset = offset;
            LineDashPattern = lineDashPattern;
            Color = color;
            Opacity = opacity;
            Width = width;
            CapType = capType;
            JoinType = joinType;
        }

        internal static class Factory
        {
            internal static ShapeStroke NewInstance(JsonObject json, LottieComposition composition)
            {
                var name = json.GetNamedString("nm");
                IList<AnimatableFloatValue> lineDashPattern = new List<AnimatableFloatValue>();
                var color = AnimatableColorValue.Factory.NewInstance(json.GetNamedObject("c"), composition);
                var width = AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("w"), composition);
                var opacity = AnimatableIntegerValue.Factory.NewInstance(json.GetNamedObject("o"), composition);
                var capType = LineCapType.Values()[(int)(json.GetNamedNumber("lc") - 1)];
                var joinType = LineJoinType.Values()[(int)(json.GetNamedNumber("lj") - 1)];
                AnimatableFloatValue offset = null;

                if (json.ContainsKey("d"))
                {
                    var dashesJson = json.GetNamedArray("d");
                    for (uint i = 0; i < dashesJson.Count; i++)
                    {
                        var dashJson = dashesJson.GetObjectAt(i);
                        var n = dashJson.GetNamedString("n");
                        if (n.Equals("o"))
                        {
                            var value = dashJson.GetNamedObject("v");
                            offset = AnimatableFloatValue.Factory.NewInstance(value, composition);
                        }
                        else if (n.Equals("d") || n.Equals("g"))
                        {
                            var value = dashJson.GetNamedObject("v");
                            lineDashPattern.Add(AnimatableFloatValue.Factory.NewInstance(value, composition));
                        }
                    }
                    if (lineDashPattern.Count == 1)
                    {
                        // If there is only 1 value then it is assumed to be equal parts on and off.
                        lineDashPattern.Add(lineDashPattern[0]);
                    }
                }
                return new ShapeStroke(name, offset, lineDashPattern, color, opacity, width, capType, joinType);
            }
        }

        internal virtual string Name { get; }

        internal virtual AnimatableColorValue Color { get; }

        internal virtual AnimatableIntegerValue Opacity { get; }

        internal virtual AnimatableFloatValue Width { get; }

        internal virtual IList<AnimatableFloatValue> LineDashPattern { get; }

        internal virtual AnimatableFloatValue DashOffset { get; }

        internal virtual LineCapType CapType { get; }

        internal virtual LineJoinType JoinType { get; }
    }
}