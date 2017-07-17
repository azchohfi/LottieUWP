using System.Collections.Generic;
using Windows.Data.Json;

namespace LottieUWP
{
    internal class AnimatableTextFrame : BaseAnimatableValue<DocumentData, DocumentData>
    {
        internal AnimatableTextFrame(List<IKeyframe<DocumentData>> keyframes, DocumentData initialValue) : base(keyframes, initialValue)
        {
        }

        protected override DocumentData ConvertType(DocumentData value)
        {
            return value;
        }

        public override IBaseKeyframeAnimation<DocumentData> CreateAnimation()
        {
            return new TextKeyframeAnimation(Keyframes);
        }

        internal static class Factory
        {
            internal static AnimatableTextFrame NewInstance(JsonObject json, LottieComposition composition)
            {
                if (json != null && json.ContainsKey("x"))
                {
                    composition.AddWarning("Lottie doesn't support expressions.");
                }
                var result = AnimatableValueParser<DocumentData>.NewInstance(json, 1, composition, ValueFactory.Instance).ParseJson();
                return new AnimatableTextFrame(result.Keyframes, result.InitialValue);
            }
        }

        private class ValueFactory : IAnimatableValueFactory<DocumentData>
        {
            internal static readonly ValueFactory Instance = new ValueFactory();

            public DocumentData ValueFromObject(IJsonValue @object, float scale)
            {
                return DocumentData.Factory.NewInstance(@object.GetObject());
            }
        }
    }
}
