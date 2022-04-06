using System;
using System.Diagnostics;
using System.Windows;

namespace ClipChopper.Domain.Errors
{
    internal sealed class DisplayTaskDialogErrorHandler : IErrorHandler
    {
        private readonly Window _window;


        public DisplayTaskDialogErrorHandler(
            Window window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
        }

        #region IErrorHandler Implementation

        public void HandleError(Exception ex)
        {
            Debug.WriteLine($"Exception occurred during task execution.{Environment.NewLine}{ex}");
            TaskDialogHelper.ShowErrorTaskDialog(_window, ex);
        }

        #endregion
    }
}
