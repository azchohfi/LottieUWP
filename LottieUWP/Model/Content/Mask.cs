using LottieUWP.Model.Animatable;

namespace LottieUWP.Model.Content
{
    internal class Mask
    {
        internal enum MaskMode
        {
            MaskModeAdd,
            MaskModeSubtract,
            MaskModeIntersect,
            MaskModeUnknown
        }

        private readonly MaskMode _maskMode;

        private Mask(MaskMode maskMode, AnimatableShapeValue maskPath, AnimatableIntegerValue opacity)
        {
            _maskMode = maskMode;
            MaskPath = maskPath;
            Opacity = opacity;
        }

        internal static class Factory
        {
            internal static Mask NewMask(JsonReader reader, LottieComposition composition)
            {
                MaskMode maskMode = MaskMode.MaskModeUnknown;
                AnimatableShapeValue maskPath = null;
                AnimatableIntegerValue opacity = null;

                reader.BeginObject();
                while (reader.HasNext())
                {
                    switch (reader.NextName())
                    {
                        case "mode":
                            switch (reader.NextString())
                            {
                                case "a":
                                    maskMode = MaskMode.MaskModeAdd;
                                    break;
                                case "s":
                                    maskMode = MaskMode.MaskModeSubtract;
                                    break;
                                case "i":
                                    maskMode = MaskMode.MaskModeIntersect;
                                    break;
                                default:
                                    maskMode = MaskMode.MaskModeUnknown;
                                    break;
                            }

                            break;
                        case "pt":
                            maskPath = AnimatableShapeValue.Factory.NewInstance(reader, composition);
                            break;
                        case "o":
                            opacity = AnimatableIntegerValue.Factory.NewInstance(reader, composition);
                            break;
                        default:
                            reader.SkipValue();
                            break;
                    }
                }

                reader.EndObject();

                return new Mask(maskMode, maskPath, opacity);
            }
        }

        internal virtual MaskMode GetMaskMode()
        {
            return _maskMode;
        }

        internal virtual AnimatableShapeValue MaskPath { get; }
        internal virtual AnimatableIntegerValue Opacity { get; }
    }
}