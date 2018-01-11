using System;
using System.Collections.Generic;
using System.Numerics;
using LottieUWP.Model.Animatable;
using LottieUWP.Utils;
using Newtonsoft.Json;

namespace LottieUWP.Animation
{
    public class Keyframe<T>
    {
        /// <summary>
        /// Some animations get exported with insane cp values in the tens of thousands.
        /// PathInterpolator fails to create the interpolator in those cases and hangs.
        /// Clamping the cp helps prevent that.
        /// </summary>
        private const float MaxCpValue = 100;
        private static readonly IInterpolator LinearInterpolator = new LinearInterpolator();

        /// <summary>
        /// The json doesn't include end frames. The data can be taken from the start frame of the next
        /// keyframe though.
        /// </summary>
        internal static void SetEndFrames<TU, TV>(List<TU> keyframes) where TU : Keyframe<TV>
        {
            var size = keyframes.Count;
            for (var i = 0; i < size - 1; i++)
            {
                // In the json, the value only contain their starting frame.
                keyframes[i].EndFrame = keyframes[i + 1].StartFrame;
            }
            var lastKeyframe = keyframes[size - 1];
            if (lastKeyframe.StartValue == null)
            {
                // The only purpose the last keyframe has is to provide the end frame of the previous
                // keyframe.
                keyframes.Remove(lastKeyframe);
            }
        }

        private readonly LottieComposition _composition;
        public T StartValue { get; }
        public T EndValue { get; }
        public IInterpolator Interpolator { get; }
        public float? StartFrame { get; }
        public float? EndFrame { get; set; }

        private float _startProgress = float.MinValue;
        private float _endProgress = float.MinValue;

        // Used by PathKeyframe but it has to be parsed by KeyFrame because we use a JsonReader to 
        // deserialzie the data so we have to parse everything in order 
        public Vector2? PathCp1 { get; set; }
        public Vector2? PathCp2 { get; set; }

        public Keyframe(LottieComposition composition, T startValue, T endValue, IInterpolator interpolator, float? startFrame, float? endFrame)
        {
            _composition = composition;
            StartValue = startValue;
            EndValue = endValue;
            Interpolator = interpolator;
            StartFrame = startFrame;
            EndFrame = endFrame;
        }

        /// <summary>
        /// Non-animated value.
        /// </summary>
        /// <param name="value"></param>
        public Keyframe(T value)
        {
            _composition = null;
            StartValue = value;
            EndValue = value;
            Interpolator = null;
            StartFrame = float.MinValue;
            EndFrame = float.MaxValue;
        }

        public virtual float StartProgress
        {
            get
            {
                if (_composition == null)
                {
                    return 0f;
                }
                if (_startProgress == float.MinValue)
                {
                    _startProgress = (StartFrame.Value - _composition.StartFrame) / _composition.DurationFrames;
                }
                return _startProgress;
            }
        }

        public virtual float EndProgress
        {
            get
            {
                if (_composition == null)
                {
                    return 1f;
                }
                if (_endProgress == float.MinValue)
                {
                    if (EndFrame == null)
                    {
                        _endProgress = 1f;
                    }
                    else
                    {
                        var startProgress = StartProgress;
                        var durationFrames = EndFrame.Value - StartFrame.Value;
                        var durationProgress = durationFrames / _composition.DurationFrames;
                        _endProgress = startProgress + durationProgress;
                    }
                }
                return _endProgress;
            }
        }

        public virtual bool Static => Interpolator == null;

        public virtual bool ContainsProgress(float progress)
        {
            return progress >= StartProgress && progress < EndProgress;
        }

        public override string ToString()
        {
            return "Keyframe{" + "startValue=" + StartValue + ", endValue=" + EndValue + ", startFrame=" + StartFrame + ", endFrame=" + EndFrame + ", interpolator=" + Interpolator + '}';
        }

        internal static class KeyFrameFactory
        {
            private static readonly object _lock = new object();

            private static Dictionary<int, WeakReference<IInterpolator>> _pathInterpolatorCache;

            static Dictionary<int, WeakReference<IInterpolator>> PathInterpolatorCache()
            {
                return _pathInterpolatorCache ?? (_pathInterpolatorCache = new Dictionary<int, WeakReference<IInterpolator>>());
            }

            private static bool GetInterpolator(int hash, out WeakReference<IInterpolator> interpolatorRef)
            {
                // This must be synchronized because get and put isn't thread safe because 
                // SparseArrayCompat has to create new sized arrays sometimes. 
                lock (_lock)
                {
                    return PathInterpolatorCache().TryGetValue(hash, out interpolatorRef);
                }
            }

            private static void PutInterpolator(int hash, WeakReference<IInterpolator> interpolator)
            {
                // This must be synchronized because get and put isn't thread safe because 
                // SparseArrayCompat has to create new sized arrays sometimes. 
                lock (_lock)
                {
                    _pathInterpolatorCache[hash] = interpolator;
                }
            }

            internal static Keyframe<T> NewInstance(JsonReader reader, LottieComposition composition, float scale,
                IAnimatableValueFactory<T> valueFactory, bool animated)
            {
                if (animated)
                    return ParseKeyframe(composition, reader, scale, valueFactory);
                return ParseStaticValue(reader, scale, valueFactory);
            }

            /// <summary>
            /// beginObject will already be called on the keyframe so it can be differentiated with 
            /// a non animated value. 
            /// </summary>
            /// <param name="composition"></param>
            /// <param name="reader"></param>
            /// <param name="scale"></param>
            /// <param name="valueFactory"></param>
            /// <returns></returns>
            private static Keyframe<T> ParseKeyframe(LottieComposition composition, JsonReader reader, float scale, IAnimatableValueFactory<T> valueFactory)
            {
                Vector2? cp1 = null;
                Vector2? cp2 = null;
                float startFrame = 0;
                var startValue = default(T);
                var endValue = default(T);
                var hold = false;
                IInterpolator interpolator;

                // Only used by PathKeyframe 
                Vector2? pathCp1 = null;
                Vector2? pathCp2 = null;

                reader.BeginObject();
                while (reader.HasNext())
                {
                    switch (reader.NextName())
                    {
                        case "t":
                            startFrame = reader.NextDouble();
                            break;
                        case "s":
                            startValue = valueFactory.ValueFromObject(reader, scale);
                            break;
                        case "e":
                            endValue = valueFactory.ValueFromObject(reader, scale);
                            break;
                        case "o":
                            cp1 = JsonUtils.JsonToPoint(reader, scale);
                            break;
                        case "i":
                            cp2 = JsonUtils.JsonToPoint(reader, scale);
                            break;
                        case "h":
                            hold = reader.NextInt() == 1;
                            break;
                        case "to":
                            pathCp1 = JsonUtils.JsonToPoint(reader, scale);
                            break;
                        case "ti":
                            pathCp2 = JsonUtils.JsonToPoint(reader, scale);
                            break;
                        default:
                            reader.SkipValue();
                            break;
                    }
                }

                reader.EndObject();

                if (hold)
                {
                    endValue = startValue;
                    // TODO: create a HoldInterpolator so progress changes don't invalidate.
                    interpolator = LinearInterpolator;
                }
                else if (cp1 != null && cp2 != null)
                {
                    cp1 = new Vector2(MiscUtils.Clamp(cp1.Value.X, -scale, scale),
                        MiscUtils.Clamp(cp1.Value.Y, -MaxCpValue, MaxCpValue));
                    cp2 = new Vector2(MiscUtils.Clamp(cp2.Value.X, -scale, scale),
                        MiscUtils.Clamp(cp2.Value.Y, -MaxCpValue, MaxCpValue));

                    int hash = Utils.Utils.HashFor(cp1.Value.X, cp1.Value.Y, cp2.Value.X, cp2.Value.Y);
                    if (GetInterpolator(hash, out var interpolatorRef) == false ||
                        interpolatorRef.TryGetTarget(out interpolator) == false)
                    {
                        interpolator = new PathInterpolator(cp1.Value.X / scale, cp1.Value.Y / scale,
                            cp2.Value.X / scale, cp2.Value.Y / scale);
                        try
                        {
                            PutInterpolator(hash, new WeakReference<IInterpolator>(interpolator));
                        }
                        catch
                        {
                            // It is not clear why but SparseArrayCompat sometimes fails with this: 
                            //     https://github.com/airbnb/lottie-android/issues/452 
                            // Because this is not a critical operation, we can safely just ignore it. 
                            // I was unable to repro this to attempt a proper fix. 
                        }
                    }
                }
                else
                {
                    interpolator = LinearInterpolator;
                }

                var keyframe = new Keyframe<T>(composition, startValue, endValue, interpolator, startFrame, null)
                {
                    PathCp1 = pathCp1,
                    PathCp2 = pathCp2
                };
                return keyframe;
            }

            private static Keyframe<T> ParseStaticValue(JsonReader reader, float scale, IAnimatableValueFactory<T> valueFactory)
            {
                T value = valueFactory.ValueFromObject(reader, scale);
                return new Keyframe<T>(value);
            }

            internal static List<Keyframe<T>> ParseKeyframes(JsonReader reader, LottieComposition composition, float scale, IAnimatableValueFactory<T> valueFactory)
            {
                var keyframes = new List<Keyframe<T>>();

                if (reader.Peek() == JsonToken.String)
                {
                    composition.AddWarning("Lottie doesn't support expressions.");
                    return keyframes;
                }

                reader.BeginObject();
                while (reader.HasNext())
                {
                    switch (reader.NextName())
                    {
                        case "k":
                            if (reader.Peek() == JsonToken.StartArray)
                            {
                                reader.BeginArray();

                                if (reader.Peek() == JsonToken.Integer || reader.Peek() == JsonToken.Float)
                                {
                                    // For properties in which the static value is an array of numbers. 
                                    keyframes.Add(NewInstance(reader, composition, scale, valueFactory, false));
                                }
                                else
                                {
                                    while (reader.HasNext())
                                    {
                                        keyframes.Add(NewInstance(reader, composition, scale, valueFactory, true));
                                    }
                                }
                                reader.EndArray();
                            }
                            else
                            {
                                keyframes.Add(NewInstance(reader, composition, scale, valueFactory, false));
                            }
                            break;
                        default:
                            reader.SkipValue();
                            break;
                    }
                }
                reader.EndObject();

                SetEndFrames<Keyframe<T>, T>(keyframes);
                return keyframes;
            }
        }
    }
}