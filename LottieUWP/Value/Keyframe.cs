using System.Numerics;

namespace LottieUWP.Value
{
    public class Keyframe<T>
    {
        private readonly LottieComposition _composition;
        public T StartValue { get; }
        public T EndValue { get; }
        public IInterpolator Interpolator { get; }
        public float? StartFrame { get; }
        public float? EndFrame { get; internal set; }

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

        public float StartProgress
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

        public float EndProgress
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

        public bool Static => Interpolator == null;

        public bool ContainsProgress(float progress)
        {
            return progress >= StartProgress && progress < EndProgress;
        }

        public override string ToString()
        {
            return "Keyframe{" + "startValue=" + StartValue + ", endValue=" + EndValue + ", startFrame=" + StartFrame + ", endFrame=" + EndFrame + ", interpolator=" + Interpolator + '}';
        }
    }
}