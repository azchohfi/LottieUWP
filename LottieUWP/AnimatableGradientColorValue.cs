using System.Collections.Generic;
using System.Diagnostics;
using Windows.Data.Json;
using Windows.UI;

namespace LottieUWP
{
    internal class AnimatableGradientColorValue : BaseAnimatableValue<GradientColor, GradientColor>
    {
        private AnimatableGradientColorValue(IList<IKeyframe<GradientColor>> keyframes, GradientColor initialValue) : base(keyframes, initialValue)
        {
        }

        protected override GradientColor ConvertType(GradientColor value)
        {
            return value;
        }

        public override IBaseKeyframeAnimation<GradientColor> CreateAnimation()
        {
            if (!HasAnimation())
            {
                return new StaticKeyframeAnimation<GradientColor>(_initialValue);
            }
            return new GradientColorKeyframeAnimation(Keyframes);
        }

        internal static class Factory
        {
            internal static AnimatableGradientColorValue NewInstance(JsonObject json, LottieComposition composition)
            {
                var result = AnimatableValueParser<GradientColor>.NewInstance(json, 1, composition, new ValueFactory((int) json.GetNamedNumber("p"))).ParseJson();
                var initialValue = result.InitialValue;
                return new AnimatableGradientColorValue(result.Keyframes, initialValue);
            }
        }

        private class ValueFactory : IAnimatableValueFactory<GradientColor>
        {
            private readonly int _colorPoints;

            internal ValueFactory(int colorPoints)
            {
                _colorPoints = colorPoints;
            }

            /// <summary>
            /// Both the color stops and opacity stops are in the same array.
            /// There are #colorPoints colors sequentially as:
            /// [
            ///     ...,
            ///     position,
            ///     red,
            ///     green,
            ///     blue,
            ///     ...
            /// ]
            /// 
            /// The remainder of the array is the opacity stops sequentially as:
            /// [
            ///     ...,
            ///     position,
            ///     opacity,
            ///     ...
            /// ]
            /// </summary>
            public GradientColor ValueFromObject(IJsonValue @object, float scale)
            {
                var array = @object.GetArray();
                var positions = new float[_colorPoints];
                var colors = new Color[_colorPoints];
                var gradientColor = new GradientColor(positions, colors);
                var r = 0;
                var g = 0;
                if (array.Count != _colorPoints * 4)
                {
                    Debug.WriteLine("Unexpected gradient length: " + array.Count + ". Expected " + _colorPoints * 4 + ". This may affect the appearance of the gradient. " + "Make sure to save your After Effects file before exporting an animation with " + "gradients.", "LOTTIE");
                }
                for (var i = 0; i < _colorPoints * 4; i++)
                {
                    var colorIndex = i / 4;
                    var value = array[i].GetNumber();
                    switch (i % 4)
                    {
                        case 0:
                            // position
                            positions[colorIndex] = (float)value;
                            break;
                        case 1:
                            r = (int)(value * 255);
                            break;
                        case 2:
                            g = (int)(value * 255);
                            break;
                        case 3:
                            var b = (int)(value * 255);
                            colors[colorIndex] = Color.FromArgb(255, (byte)r, (byte)g, (byte)b);
                            break;
                    }
                }

                AddOpacityStopsToGradientIfNeeded(gradientColor, array);
                return gradientColor;
            }

            /// <summary>
            /// This cheats a little bit.
            /// Opacity stops can be at arbitrary intervals independent of color stops.
            /// This uses the existing color stops and modifies the opacity at each existing color stop
            /// based on what the opacity would be.
            /// 
            /// This should be a good approximation is nearly all cases. However, if there are many more
            /// opacity stops than color stops, information will be lost.
            /// </summary>
            private void AddOpacityStopsToGradientIfNeeded(GradientColor gradientColor, JsonArray array)
            {
                var startIndex = _colorPoints * 4;
                if (array.Count <= startIndex)
                {
                    return;
                }

                var opacityStops = (array.Count - startIndex) / 2;
                var positions = new double[opacityStops];
                var opacities = new double[opacityStops];

                for (int i = startIndex, j = 0; i < array.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        positions[j] = array[i].GetNumber();
                    }
                    else
                    {
                        opacities[j] = array[i].GetNumber();
                        j++;
                    }
                }

                for (var i = 0; i < gradientColor.Size; i++)
                {
                    var color = gradientColor.Colors[i];
                    color = Color.FromArgb((byte)GetOpacityAtPosition(gradientColor.Positions[i], positions, opacities), color.R, color.G, color.B);
                    gradientColor.Colors[i] = color;
                }
            }

            private int GetOpacityAtPosition(double position, double[] positions, double[] opacities)
            {
                for (var i = 1; i < positions.Length; i++)
                {
                    var lastPosition = positions[i - 1];
                    var thisPosition = positions[i];
                    if (positions[i] >= position)
                    {
                        var progress = (position - lastPosition) / (thisPosition - lastPosition);
                        return (int)(255 * MiscUtils.Lerp(opacities[i - 1], opacities[i], progress));
                    }
                }
                return (int)(255 * opacities[opacities.Length - 1]);
            }
        }
    }
}