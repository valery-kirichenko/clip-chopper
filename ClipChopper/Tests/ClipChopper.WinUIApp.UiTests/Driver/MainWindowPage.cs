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

    public void WaitUntilOpened(TimeSpan timeout)
    {
        var opened = Retry.WhileFalse(
            () => PlayButton.IsEnabled,
            timeout,
            TimeSpan.FromMilliseconds(100),
            throwOnTimeout: false,
            ignoreException: true
        );
        if (!opened.Result)
        {
            throw new TimeoutException("The video did not open: the play button stayed disabled.");
        }
    }

    public void SeekTo(int ms)
    {
        var range = SeekSlider.Patterns.RangeValue.Pattern;
        range.SetValue(ms);
        var expectedTime = FormatTime(ms);
        var reached = Retry.WhileFalse(
            () => Math.Abs(range.Value.Value - ms) <= FrameToleranceMs && CurrentTimeText.Name == expectedTime,
            TimeSpan.FromSeconds(10),
            TimeSpan.FromMilliseconds(50),
            throwOnTimeout: false,
            ignoreException: true
        );
        if (!reached.Result)
        {
            throw new TimeoutException($"The player did not reach {expectedTime}. Time text: {CurrentTimeText.Name}.");
        }
    }

    public void MarkStart()
    {
        StartFragmentButton.Patterns.Invoke.Pattern.Invoke();
    }

    public void MarkEnd()
    {
        EndFragmentButton.Patterns.Invoke.Pattern.Invoke();
    }

    public void MarkRange(int startMs, int endMs)
    {
        SeekTo(startMs);
        MarkStart();
        SeekTo(endMs);
        MarkEnd();
    }

    public MainWindowState ReadState()
    {
        var range = SeekSlider.Patterns.RangeValue.Pattern;
        var status = SeekSlider.Properties.ItemStatus.ValueOrDefault;
        var (start, stop) = ParseStatus(status);
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
            start,
            stop
        );
    }

    /// <summary>
    /// Reads the state until the condition holds or the timeout ends. Returns the last state, so the caller asserts it.
    /// </summary>
    public MainWindowState ReadStateWhen(Func<MainWindowState, bool> condition, TimeSpan timeout)
    {
        MainWindowState? state = null;
        var met = Retry.WhileFalse(
            () =>
            {
                state = ReadState();
                return condition(state);
            },
            timeout,
            TimeSpan.FromMilliseconds(50),
            throwOnTimeout: false,
            ignoreException: true
        );
        return met.Result ? state! : ReadState();
    }

    private static (int? Start, int? Stop) ParseStatus(string? status)
    {
        if (string.IsNullOrEmpty(status))
        {
            return (null, null);
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

        return (start, stop);
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
