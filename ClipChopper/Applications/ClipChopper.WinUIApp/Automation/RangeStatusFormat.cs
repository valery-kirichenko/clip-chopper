using System;
using System.Globalization;

namespace ClipChopper.WinUIApp.Automation
{
    /// <summary>
    /// The text that the app shows in the ItemStatus of the seek slider when "--expose-range" is set.
    /// The text has the form "start=[ms];stop=[ms]".
    /// </summary>
    public static class RangeStatusFormat
    {
        private const string StartKey = "start";

        private const string StopKey = "stop";

        public static string Format(int startMs, int stopMs)
        {
            return FormattableString.Invariant($"{StartKey}={startMs};{StopKey}={stopMs}");
        }

        public static bool TryParse(string status, out int startMs, out int stopMs)
        {
            int? start = null;
            int? stop = null;
            foreach (var part in status.Split(';'))
            {
                var pair = part.Split('=');
                if (pair.Length != 2 || !int.TryParse(pair[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                {
                    startMs = 0;
                    stopMs = 0;
                    return false;
                }

                if (pair[0] == StartKey)
                {
                    start = value;
                }
                else if (pair[0] == StopKey)
                {
                    stop = value;
                }
            }

            startMs = start ?? 0;
            stopMs = stop ?? 0;
            return start is not null && stop is not null;
        }
    }
}
