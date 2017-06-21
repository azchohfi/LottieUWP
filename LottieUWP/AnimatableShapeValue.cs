using System.Collections.Generic;
using Windows.Data.Json;

namespace LottieUWP
{
    internal class AnimatableShapeValue : BaseAnimatableValue<ShapeData, Path>
    {
        private readonly Path _convertTypePath = new Path();

        private AnimatableShapeValue(IList<IKeyframe<ShapeData>> keyframes, ShapeData initialValue) : base(keyframes, initialValue)
        {
        }

        public override IBaseKeyframeAnimation<Path> CreateAnimation()
        {
            if (!HasAnimation())
            {
                return new StaticKeyframeAnimation<Path>(ConvertType(_initialValue));
            }
            return new ShapeKeyframeAnimation(Keyframes);
        }

        protected override Path ConvertType(ShapeData shapeData)
        {
            _convertTypePath.Reset();
            MiscUtils.GetPathFromData(shapeData, _convertTypePath);
            return _convertTypePath;
        }

        internal static class Factory
        {
            internal static AnimatableShapeValue NewInstance(JsonObject json, LottieComposition composition)
            {
                var result = AnimatableValueParser<ShapeData>.NewInstance(json, composition.DpScale, composition, ShapeData.Factory.Instance).ParseJson();
                return new AnimatableShapeValue(result.Keyframes, result.InitialValue);
            }
        }
    }
}