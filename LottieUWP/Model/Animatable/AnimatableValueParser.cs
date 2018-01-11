using System.Collections.Generic;
using LottieUWP.Animation;

namespace LottieUWP.Model.Animatable
{
    internal class AnimatableValueParser<T>
    {
        private readonly JsonReader _reader;
        private readonly float _scale;
        private readonly LottieComposition _composition;
        private readonly IAnimatableValueFactory<T> _valueFactory;

        private AnimatableValueParser(JsonReader reader, float scale, LottieComposition composition, IAnimatableValueFactory<T> valueFactory)
        {
            _reader = reader;
            _scale = scale;
            _composition = composition;
            _valueFactory = valueFactory;
        }

        /// <summary>
        /// Will return null if the animation can't be played such as if it has expressions. 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="scale"></param>
        /// <param name="composition"></param>
        /// <param name="valueFactory"></param>
        /// <returns></returns>
        internal static List<Keyframe<T>> NewInstance(JsonReader reader, float scale, LottieComposition composition, IAnimatableValueFactory<T> valueFactory)
        {
            var parser = new AnimatableValueParser<T>(reader, scale, composition, valueFactory);
            return parser.ParseKeyframes();
        }

        /// <summary>
        /// Will return null if the animation can't be played such as if it has expressions. 
        /// </summary>
        /// <returns></returns>
        private List<Keyframe<T>> ParseKeyframes()
        {
            return Keyframe<T>.KeyFrameFactory.ParseKeyframes(_reader, _composition, _scale, _valueFactory);
        }
    }
}