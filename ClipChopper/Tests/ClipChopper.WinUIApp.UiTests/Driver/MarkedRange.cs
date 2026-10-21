using ClipChopper.WinUIApp.CommandLine;

namespace ClipChopper.WinUIApp.UiTests.Driver;

/// <summary>
/// The marked fragment that the app shows in the ItemStatus of the seek slider.
/// RangeStatusFormat defines the form of the text.
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
    /// Throws FormatException when the status is not valid.
    /// </summary>
    public static MarkedRange? Parse(string? status)
    {
        if (string.IsNullOrEmpty(status))
        {
            return null;
        }

        if (!RangeStatusFormat.TryParse(status, out var startMs, out var stopMs))
        {
            throw new FormatException($"Unexpected ItemStatus of the seek slider: '{status}'.");
        }

        return new MarkedRange(startMs, stopMs);
    }
}
