using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI;
using LottieUWP.Animation;
using LottieUWP.Animation.Keyframe;
using LottieUWP.Model.Content;
using LottieUWP.Utils;
using Newtonsoft.Json;

namespace LottieUWP.Model.Animatable
{
    public class AnimatableGradientColorValue : BaseAnimatableValue<GradientColor, GradientColor>
    {
        private AnimatableGradientColorValue(List<Keyframe<GradientColor>> keyframes) : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<GradientColor, GradientColor> CreateAnimation()
        {
            return new GradientColorKeyframeAnimation(Keyframes);
        }

        internal static class Factory
        {
            internal static AnimatableGradientColorValue NewInstance(JsonReader reader, LottieComposition composition, int points)
            {
                return new AnimatableGradientColorValue(AnimatableValueParser<GradientColor>.NewInstance(reader, 1, composition, new ValueFactory(points)));
            }
        }

        private class ValueFactory : IAnimatableValueFactory<GradientColor>
        {
            /** The number of colors if it exists in the json or -1 if it doesn't (legacy bodymovin) */
            private int _colorPoints;

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
            public GradientColor ValueFromObject(JsonReader reader, float scale)
            {
                List<float> array = new List<float>();
                // The array was started by Keyframe because it thought that this may be an array of keyframes 
                // but peek returned a number so it considered it a static array of numbers. 
                bool isArray = reader.Peek() == JsonToken.StartArray;
                if (isArray)
                {
                    reader.BeginArray();
                }
                while (reader.HasNext())
                {
                    array.Add(reader.NextDouble());
                }
                if (isArray)
                {
                    reader.EndArray();
                }
                if (_colorPoints == -1)
                {
                    _colorPoints = array.Count / 4;
                }

                var positions = new float[_colorPoints];
                var colors = new Color[_colorPoints];
                
                var r = 0;
                var g = 0;
                if (array.Count != _colorPoints * 4)
                {
                    Debug.WriteLine("Unexpected gradient length: " + array.Count + ". Expected " + _colorPoints * 4 + ". This may affect the appearance of the gradient. " + "Make sure to save your After Effects file before exporting an animation with " + "gradients.", LottieLog.Tag);
                }
                for (var i = 0; i < _colorPoints * 4; i++)
                {
                    var colorIndex = i / 4;
                    var value = array[i];
                    switch (i % 4)
                    {
                        case 0:
                            // position
                            positions[colorIndex] = value;
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

                var gradientColor = new GradientColor(positions, colors);
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
            private void AddOpacityStopsToGradientIfNeeded(GradientColor gradientColor, List<float> array)
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
                        positions[j] = array[i];
                    }
                    else
                    {
                        opacities[j] = array[i];
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