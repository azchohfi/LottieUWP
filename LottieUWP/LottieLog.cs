using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LottieUWP
{
    public static class LottieLog
    {
        internal const string Tag = "LOTTIE";

        private const int MaxDepth = 20;
        private static bool _traceEnabled;
        private static string[] _sections;
        private static long[] _startTimeNs;
        private static int _traceDepth;

        private static readonly Queue<string> Msgs = new Queue<string>();

        public static bool TraceEnabled
        {
            set
            {
                if (_traceEnabled == value)
                {
                    return;
                }
                _traceEnabled = value;
                if (_traceEnabled)
                {
                    _sections = new string[MaxDepth];
                    _startTimeNs = new long[MaxDepth];
                }
            }
        }

        internal static void BeginSection(string section)
        {
            if (!_traceEnabled || _traceDepth >= MaxDepth)
            {
                return;
            }
            _sections[_traceDepth] = section;
            _startTimeNs[_traceDepth] = CurrentUnixTime();
            BatchedDebugWriteLine($"Begin Section: {section}");
            _traceDepth++;
        }

        internal static float EndSection(string section)
        {
            if (!_traceEnabled)
            {
                return 0;
            }
            _traceDepth--;
            if (_traceDepth == -1)
            {
                throw new System.InvalidOperationException("Can't end trace section. There are none.");
            }
            if (!section.Equals(_sections[_traceDepth]))
            {
                throw new System.InvalidOperationException("Unbalanced trace call " + section + ". Expected " + _sections[_traceDepth] + ".");
            }
            var duration = (CurrentUnixTime() - _startTimeNs[_traceDepth + 1]) / 1000000f;
            BatchedDebugWriteLine($"End Section - {duration}ms");
            return duration;
        }

        private static void BatchedDebugWriteLine(string message)
        {
            Msgs.Enqueue($"{new string(' ', _traceDepth)}{message}");
            if (_traceDepth == 0 && Msgs.Count >= 20)
            {
                var sb = new StringBuilder();
                while (Msgs.Count > 0)
                {
                    sb.AppendLine(Msgs.Dequeue());
                }
                Debug.WriteLine(sb.ToString());
            }
        }

        private static readonly System.DateTime Epoc = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);

        private static long CurrentUnixTime()
        {
            return (long)(System.DateTime.UtcNow - Epoc).TotalMilliseconds;
        }
    }
}
