using System.Collections.Generic;
using Windows.Data.Json;

namespace LottieUWP
{
    internal class PolystarShape
    {
        internal sealed class Type
        {
            public static readonly Type Star = new Type("Star", InnerEnum.Star, 1);
            public static readonly Type Polygon = new Type("Polygon", InnerEnum.Polygon, 2);

            private static readonly IList<Type> ValueList = new List<Type>();

            static Type()
            {
                ValueList.Add(Star);
                ValueList.Add(Polygon);
            }

            public enum InnerEnum
            {
                Star,
                Polygon
            }

            public readonly InnerEnum InnerEnumValue;
            private readonly string _nameValue;
            private readonly int _ordinalValue;
            private static int _nextOrdinal;

            internal readonly int Value;

            internal Type(string name, InnerEnum innerEnum, int value)
            {
                Value = value;

                _nameValue = name;
                _ordinalValue = _nextOrdinal++;
                InnerEnumValue = innerEnum;
            }

            internal static Type ForValue(int value)
            {
                foreach (var type in Values())
                {
                    if (type.Value == value)
                    {
                        return type;
                    }
                }
                return null;
            }

            public static IList<Type> Values()
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

            public static Type ValueOf(string name)
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

        private readonly Type _type;

        private PolystarShape(string name, Type type, AnimatableFloatValue points, IAnimatableValue<PointF> position, AnimatableFloatValue rotation, AnimatableFloatValue innerRadius, AnimatableFloatValue outerRadius, AnimatableFloatValue innerRoundedness, AnimatableFloatValue outerRoundedness)
        {
            Name = name;
            _type = type;
            Points = points;
            Position = position;
            Rotation = rotation;
            InnerRadius = innerRadius;
            OuterRadius = outerRadius;
            InnerRoundedness = innerRoundedness;
            OuterRoundedness = outerRoundedness;
        }

        internal static class Factory
        {
            internal static PolystarShape NewInstance(JsonObject json, LottieComposition composition)
            {
                var name = json.GetNamedString("nm");
                var type = Type.ForValue((int)json.GetNamedNumber("sy"));
                var points = AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("pt"), composition, false);
                var position = AnimatablePathValue.CreateAnimatablePathOrSplitDimensionPath(json.GetNamedObject("p"), composition);
                var rotation = AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("r"), composition, false);
                var outerRadius = AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("or"), composition);
                var outerRoundedness = AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("os"), composition, false);
                AnimatableFloatValue innerRadius;
                AnimatableFloatValue innerRoundedness;

                if (type == Type.Star)
                {
                    innerRadius = AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("ir"), composition);
                    innerRoundedness = AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("is"), composition, false);
                }
                else
                {
                    innerRadius = null;
                    innerRoundedness = null;
                }
                return new PolystarShape(name, type, points, position, rotation, innerRadius, outerRadius, innerRoundedness, outerRoundedness);
            }
        }

        internal virtual string Name { get; }

        internal new virtual Type GetType()
        {
            return _type;
        }

        internal virtual AnimatableFloatValue Points { get; }

        internal virtual IAnimatableValue<PointF> Position { get; }

        internal virtual AnimatableFloatValue Rotation { get; }

        internal virtual AnimatableFloatValue InnerRadius { get; }

        internal virtual AnimatableFloatValue OuterRadius { get; }

        internal virtual AnimatableFloatValue InnerRoundedness { get; }

        internal virtual AnimatableFloatValue OuterRoundedness { get; }
    }
}