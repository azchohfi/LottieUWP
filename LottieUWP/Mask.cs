using Windows.Data.Json;

namespace LottieUWP
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

        private Mask(MaskMode maskMode, AnimatableShapeValue maskPath)
        {
            _maskMode = maskMode;
            MaskPath = maskPath;
        }

        internal static class Factory
        {
            internal static Mask NewMask(JsonObject json, LottieComposition composition)
            {
                MaskMode maskMode;
                switch (json.GetNamedString("mode"))
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

                var maskPath = AnimatableShapeValue.Factory.NewInstance(json.GetNamedObject("pt"), composition);
                // TODO: use this
                // JSONObject opacityJson = json.GetNamedObject("o");
                // if (opacityJson != null) {
                //   AnimatableIntegerValue opacity =
                //       new AnimatableIntegerValue(opacityJson, composition, false, true);
                // }

                return new Mask(maskMode, maskPath);
            }
        }

        internal virtual MaskMode GetMaskMode()
        {
            return _maskMode;
        }

        internal virtual AnimatableShapeValue MaskPath { get; }
    }
}