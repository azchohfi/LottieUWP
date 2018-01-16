using System.Collections.Generic;
using LottieUWP.Animation;
using LottieUWP.Model.Animatable;

namespace LottieUWP.Parser
{
    public static class AnimatableValueParser
    {
        public static AnimatableFloatValue ParseFloat(JsonReader reader, LottieComposition composition)
        {
            return ParseFloat(reader, composition, true);
        }

        public static AnimatableFloatValue ParseFloat(JsonReader reader, LottieComposition composition, bool isDp)
        {
            return new AnimatableFloatValue(Parse(reader, isDp ? Utils.Utils.DpScale() : 1f, composition, FloatParser.Instance));
        }

        public static AnimatableIntegerValue ParseInteger(JsonReader reader, LottieComposition composition)
        {
            return new AnimatableIntegerValue(Parse(reader, composition, IntegerParser.Instance));
        }

        public static AnimatablePointValue ParsePoint(JsonReader reader, LottieComposition composition)
        {
            return new AnimatablePointValue(Parse(reader, Utils.Utils.DpScale(), composition, PointFParser.Instance));
        }

        public static AnimatableScaleValue ParseScale(JsonReader reader, LottieComposition composition)
        {
            return new AnimatableScaleValue(Parse(reader, composition, ScaleXyParser.Instance));
        }

        public static AnimatableShapeValue ParseShapeData(JsonReader reader, LottieComposition composition)
        {
            return new AnimatableShapeValue(Parse(reader, Utils.Utils.DpScale(), composition, ShapeDataParser.Instance));
        }

        public static AnimatableTextFrame ParseDocumentData(JsonReader reader, LottieComposition composition)
        {
            return new AnimatableTextFrame(Parse(reader, composition, DocumentDataParser.Instance));
        }

        public static AnimatableColorValue ParseColor(JsonReader reader, LottieComposition composition)
        {
            return new AnimatableColorValue(Parse(reader, composition, ColorParser.Instance));
        }

        public static AnimatableGradientColorValue ParseGradientColor(JsonReader reader, LottieComposition composition, int points)
        {
            return new AnimatableGradientColorValue(Parse(reader, composition, new GradientColorParser(points)));
        }

        /// <summary>
        /// Will return null if the animation can't be played such as if it has expressions.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="composition"></param>
        /// <param name="valueParser"></param>
        /// <returns></returns>
        private static List<Keyframe<T>> Parse<T>(JsonReader reader, LottieComposition composition, IValueParser<T> valueParser)
        {
            return KeyframesParser.Parse(reader, composition, 1, valueParser);
        }

        /// <summary>
        /// Will return null if the animation can't be played such as if it has expressions.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="scale"></param>
        /// <param name="composition"></param>
        /// <param name="valueParser"></param>
        /// <returns></returns>
        private static List<Keyframe<T>> Parse<T>(JsonReader reader, float scale, LottieComposition composition, IValueParser<T> valueParser)
        {
            return KeyframesParser.Parse(reader, composition, scale, valueParser);
        }
    }
}