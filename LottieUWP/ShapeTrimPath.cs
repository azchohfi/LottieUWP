using System.Collections.Generic;
using Windows.Data.Json;

namespace LottieUWP
{
    internal class ShapeTrimPath
    {
        internal sealed class Type
        {
            public static readonly Type Simultaneously = new Type("Simultaneously", InnerEnum.Simultaneously);
            public static readonly Type Individually = new Type("Individually", InnerEnum.Individually);

            private static readonly IList<Type> ValueList = new List<Type>();

            static Type()
            {
                ValueList.Add(Simultaneously);
                ValueList.Add(Individually);
            }

            public enum InnerEnum
            {
                Simultaneously,
                Individually
            }

            private readonly InnerEnum _innerEnumValue;
            private readonly string _nameValue;
            private readonly int _ordinalValue;
            private static int _nextOrdinal;

            private Type(string name, InnerEnum innerEnum)
            {
                _nameValue = name;
                _ordinalValue = _nextOrdinal++;
                _innerEnumValue = innerEnum;
            }

            internal static Type ForId(int id)
            {
                switch (id)
                {
                    case 1:
                        return Simultaneously;
                    case 2:
                        return Individually;
                    default:
                        throw new System.ArgumentException("Unknown trim path type " + id);
                }
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
                foreach (Type enumInstance in ValueList)
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
        private readonly AnimatableFloatValue _start;
        private readonly AnimatableFloatValue _end;
        private readonly AnimatableFloatValue _offset;

        private ShapeTrimPath(string name, Type type, AnimatableFloatValue start, AnimatableFloatValue end, AnimatableFloatValue offset)
        {
            Name = name;
            _type = type;
            _start = start;
            _end = end;
            _offset = offset;
        }

        internal class Factory
        {
            internal static ShapeTrimPath NewInstance(JsonObject json, LottieComposition composition)
            {
                return new ShapeTrimPath(json.GetNamedString("nm"), Type.ForId((int)json.GetNamedNumber("m", 1)), AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("s"), composition, false), AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("e"), composition, false), AnimatableFloatValue.Factory.NewInstance(json.GetNamedObject("o"), composition, false));
            }
        }

        internal virtual string Name { get; }

        internal new virtual Type GetType()
        {
            return _type;
        }

        internal virtual AnimatableFloatValue End => _end;

        internal virtual AnimatableFloatValue Start => _start;

        internal virtual AnimatableFloatValue Offset => _offset;

        public override string ToString()
        {
            return "Trim Path: {start: " + _start + ", end: " + _end + ", offset: " + _offset + "}";
        }
    }
}