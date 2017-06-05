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

        internal override GradientColor ConvertType(GradientColor value)
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

        internal sealed class Factory
        {
            internal static AnimatableGradientColorValue NewInstance(JsonObject json, LottieComposition composition)
            {
                AnimatableValueParser<GradientColor>.Result result = AnimatableValueParser<GradientColor>.NewInstance(json, 1, composition, new ValueFactory((int) json.GetNamedNumber("p"))).ParseJson();
                GradientColor initialValue = result.InitialValue;
                return new AnimatableGradientColorValue(result.Keyframes, initialValue);
            }
        }

        private class ValueFactory : IAnimatableValueFactory<GradientColor>
        {
            internal readonly int ColorPoints;

            internal ValueFactory(int colorPoints)
            {
                ColorPoints = colorPoints;
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
            public virtual GradientColor ValueFromObject(IJsonValue @object, float scale)
            {
                var array = @object.GetArray();
                float[] positions = new float[ColorPoints];
                Color[] colors = new Color[ColorPoints];
                GradientColor gradientColor = new GradientColor(positions, colors);
                int r = 0;
                int g = 0;
                if (array.Count != ColorPoints * 4)
                {
                    Debug.WriteLine("Unexpected gradient length: " + array.Count + ". Expected " + ColorPoints * 4 + ". This may affect the appearance of the gradient. " + "Make sure to save your After Effects file before exporting an animation with " + "gradients.", L.Tag);
                }
                for (int i = 0; i < ColorPoints * 4; i++)
                {
                    int colorIndex = i / 4;
                    double value = array[i].GetNumber();
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
                            int b = (int)(value * 255);
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
            internal virtual void AddOpacityStopsToGradientIfNeeded(GradientColor gradientColor, JsonArray array)
            {
                int startIndex = ColorPoints * 4;
                if (array.Count <= startIndex)
                {
                    return;
                }

                int opacityStops = (array.Count - startIndex) / 2;
                double[] positions = new double[opacityStops];
                double[] opacities = new double[opacityStops];

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

                for (int i = 0; i < gradientColor.Size; i++)
                {
                    var color = gradientColor.Colors[i];
                    color = Color.FromArgb((byte)GetOpacityAtPosition(gradientColor.Positions[i], positions, opacities), color.R, color.G, color.B);
                    gradientColor.Colors[i] = color;
                }
            }

            internal virtual int GetOpacityAtPosition(double position, double[] positions, double[] opacities)
            {
                for (int i = 1; i < positions.Length; i++)
                {
                    double lastPosition = positions[i - 1];
                    double thisPosition = positions[i];
                    if (positions[i] >= position)
                    {
                        double progress = (position - lastPosition) / (thisPosition - lastPosition);
                        return (int)(255 * MiscUtils.Lerp(opacities[i - 1], opacities[i], progress));
                    }
                }
                return (int)(255 * opacities[opacities.Length - 1]);
            }
        }
    }
}