using System.Globalization;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;
using FlaUI.UIA3;

namespace ClipChopper.WinUIApp.UiTests.Driver;

public sealed class MainWindowPage
{
    public const int FrameToleranceMs = 34;

    private readonly Window _window;
    private readonly UIA3Automation _automation;

    public MainWindowPage(Window window, UIA3Automation automation)
    {
        _window = window;
        _automation = automation;
    }

    public AutomationElement PlayButton => Find("PlayButton");

    public AutomationElement StartFragmentButton => Find("StartFragmentButton");

    public AutomationElement EndFragmentButton => Find("EndFragmentButton");

    public AutomationElement CropButton => Find("CropButton");

    public AutomationElement DirectoryList => Find("DirectoryList");

    public AutomationElement AudioTrackSelector => Find("AudioTrackSelector");

    public AutomationElement SeekSlider => Find("SeekSlider");

    public AutomationElement CurrentTimeText => Find("CurrentTimeText");

    public AutomationElement DurationText => Find("DurationText");

    /// <summary>
    /// Waits until the play button is enabled and the slider shows the marked range of the new video.
    /// </summary>
    public void WaitUntilOpened(TimeSpan timeout)
    {
        var opened = Retry.WhileFalse(
            IsOpened,
            timeout,
            UiTiming.PollInterval,
            throwOnTimeout: false,
            ignoreException: true
        );
        if (!opened.Result)
        {
            throw new TimeoutException("The video did not open: the play button stayed disabled or the range is not set.", opened.LastException);
        }
    }

    public void SeekTo(int ms)
    {
        SeekSlider.Patterns.RangeValue.Pattern.SetValue(ms);
        var reached = Retry.WhileFalse(
            () => IsAt(ms),
            UiTiming.Timeout,
            UiTiming.PollInterval,
            throwOnTimeout: false,
            ignoreException: true
        );
        if (!reached.Result)
        {
            throw new TimeoutException($"The player did not reach {FormatTime(ms)}. Time text: {CurrentTimeText.Name}.");
        }
    }

    public void MarkStart(int expectedMs)
    {
        Mark(RangeEdge.Start, expectedMs);
    }

    public void MarkEnd(int expectedMs)
    {
        Mark(RangeEdge.Stop, expectedMs);
    }

    public void MarkRange(int startMs, int endMs)
    {
        SeekTo(startMs);
        MarkStart(startMs);
        SeekTo(endMs);
        MarkEnd(endMs);
    }

    public MainWindowState ReadState()
    {
        var range = SeekSlider.Patterns.RangeValue.Pattern;
        var selected = DirectoryList
            .FindAllDescendants(_automation.ConditionFactory.ByControlType(ControlType.ListItem))
            .FirstOrDefault(item => item.Patterns.SelectionItem.Pattern.IsSelected.Value);

        return new MainWindowState(
            _window.Title,
            PlayButton.IsEnabled,
            CurrentTimeText.Name,
            DurationText.Name,
            range.Value.Value,
            range.Maximum.Value,
            CropButton.Patterns.Toggle.Pattern.ToggleState.Value == ToggleState.On,
            selected?.Name,
            AudioTrackSelector.IsEnabled,
            ReadRange()
        );
    }

    public MarkedRange? ReadRange()
    {
        return MarkedRange.Parse(SeekSlider.Properties.ItemStatus.ValueOrDefault);
    }

    private void Mark(RangeEdge edge, int expectedMs)
    {
        var buttonId = edge == RangeEdge.Start ? "StartFragmentButton" : "EndFragmentButton";
        var invoked = Retry.WhileException(
            () => Find(buttonId).Patterns.Invoke.Pattern.Invoke(),
            UiTiming.Timeout,
            UiTiming.PollInterval
        );
        if (!invoked.Success)
        {
            throw new TimeoutException($"Cannot invoke '{buttonId}'.", invoked.LastException);
        }

        var marked = Retry.WhileFalse(
            () => IsMarkedAt(edge, expectedMs),
            UiTiming.Timeout,
            UiTiming.PollInterval,
            throwOnTimeout: false,
            ignoreException: true
        );
        if (!marked.Result)
        {
            throw new TimeoutException($"The {edge} mark did not reach {expectedMs}. Range: {ReadRange()}.", marked.LastException);
        }
    }

    private bool IsOpened()
    {
        return PlayButton.IsEnabled && ReadRange() != null;
    }

    private bool IsAt(int ms)
    {
        return Math.Abs(SeekSlider.Patterns.RangeValue.Pattern.Value.Value - ms) <= FrameToleranceMs && CurrentTimeText.Name == FormatTime(ms);
    }

    private bool IsMarkedAt(RangeEdge edge, int expectedMs)
    {
        return ReadRange() is { } range && Math.Abs(range[edge] - expectedMs) <= FrameToleranceMs;
    }

    private static string FormatTime(int ms)
    {
        return TimeSpan.FromMilliseconds(ms).ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
    }

    private AutomationElement Find(string automationId)
    {
        return _window.FindFirstDescendant(_automation.ConditionFactory.ByAutomationId(automationId))
            ?? throw new InvalidOperationException($"Control '{automationId}' was not found.");
    }
}
