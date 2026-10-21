using System.Globalization;

namespace ClipChopper.WinUIApp.UiTests.Driver;

/// <summary>
/// The marked fragment that the app shows in the ItemStatus of the seek slider.
/// The text has the form "start=&lt;ms&gt;;stop=&lt;ms&gt;".
/// </summary>
public sealed record MarkedRange(int StartMs, int StopMs)
{
    public int this[RangeEdge edge] => edge switch
    {
        RangeEdge.Start => StartMs,
        RangeEdge.Stop => StopMs,
        _ => throw new ArgumentOutOfRangeException(nameof(edge), edge, null),
    };

    /// <summary>
    /// Returns null when the app has not set the status yet.
    /// Throws FormatException when the status has no start or no stop value.
    /// </summary>
    public static MarkedRange? Parse(string? status)
    {
        if (string.IsNullOrEmpty(status))
        {
            return null;
        }

        int? start = null;
        int? stop = null;
        foreach (var part in status.Split(';'))
        {
            var pair = part.Split('=');
            if (pair.Length != 2 || !int.TryParse(pair[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                throw new FormatException($"Unexpected ItemStatus of the seek slider: '{status}'.");
            }

            if (pair[0] == "start")
            {
                start = value;
            }
            else if (pair[0] == "stop")
            {
                stop = value;
            }
        }

        if (start is null || stop is null)
        {
            throw new FormatException($"The ItemStatus of the seek slider has no start or no stop value: '{status}'.");
        }

        return new MarkedRange(start.Value, stop.Value);
    }
}
