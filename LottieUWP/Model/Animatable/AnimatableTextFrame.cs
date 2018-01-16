using System.Collections.Generic;
using LottieUWP.Animation;
using LottieUWP.Animation.Keyframe;

namespace LottieUWP.Model.Animatable
{
    public class AnimatableTextFrame : BaseAnimatableValue<DocumentData, DocumentData>
    {
        internal AnimatableTextFrame(List<Keyframe<DocumentData>> keyframes) : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<DocumentData, DocumentData> CreateAnimation()
        {
            return new TextKeyframeAnimation(Keyframes);
        }

        internal static class Factory
        {
            internal static AnimatableTextFrame NewInstance(JsonReader reader, LottieComposition composition)
            {
                return new AnimatableTextFrame(AnimatableValueParser<DocumentData>.NewInstance(reader, 1, composition, ValueFactory.Instance));
            }
        }

        private class ValueFactory : IAnimatableValueFactory<DocumentData>
        {
            internal static readonly ValueFactory Instance = new ValueFactory();

            public DocumentData ValueFromObject(JsonReader reader, float scale)
            {
                return DocumentData.Factory.NewInstance(reader);
            }
        }
    }
}
