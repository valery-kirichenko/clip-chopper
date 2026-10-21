using System.Drawing;

namespace ClipChopper.WinUIApp.UiTests.Driver;

public sealed record RangeMeasurement(
    Color TrackColor,
    Color RangeColor,
    double RunStartRatio,
    double RunEndRatio,
    double ContrastRatio);
