using ClipChopper.WinUIApp.UiTests.Platform;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;

namespace ClipChopper.WinUIApp.UiTests.Driver;

/// <summary>
/// A file picker dialog that the app shows, and the pending invoke of the button that opened it.
/// </summary>
public sealed class PickerDialog
{
    private const string CancelButtonId = "2";

    private readonly IntPtr _handle;
    private readonly Window _window;
    private readonly Task _invoke;

    public PickerDialog(IntPtr handle, Window window, Task invoke)
    {
        _handle = handle;
        _window = window;
        _invoke = invoke;
    }

    public void Cancel()
    {
        var cancel = _window.FindFirstChild(factory => factory.ByAutomationId(CancelButtonId).And(factory.ByControlType(ControlType.Button)))
            ?? throw new InvalidOperationException("The Cancel button of the picker dialog was not found.");
        cancel.Patterns.Invoke.Pattern.Invoke();
        var closed = Retry.WhileFalse(() => !OwnedDialogFinder.IsOpen(_handle), UiTiming.Timeout, UiTiming.PollInterval, throwOnTimeout: false, ignoreException: true);
        if (!closed.Result)
        {
            throw new TimeoutException("The picker dialog did not close.", closed.LastException);
        }

        if (!_invoke.Wait(UiTiming.Timeout))
        {
            throw new TimeoutException("The button invoke did not return after the picker dialog closed.");
        }
    }
}
