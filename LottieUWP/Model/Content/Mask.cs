using System.Diagnostics;
using LottieUWP.Model.Animatable;

namespace LottieUWP.Model.Content
{
    public class Mask
    {
        public enum MaskMode
        {
            MaskModeAdd,
            MaskModeSubtract,
            MaskModeIntersect
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
                MaskMode maskMode = MaskMode.MaskModeAdd;
                AnimatableShapeValue maskPath = null;
                AnimatableIntegerValue opacity = null;

                reader.BeginObject();
                while (reader.HasNext())
                {
                    string mode = reader.NextName();
                    switch (mode)
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
                                    Debug.WriteLine($"Unknown mask mode {mode}. Defaulting to Add.", LottieLog.Tag);
                                    maskMode = MaskMode.MaskModeAdd;
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