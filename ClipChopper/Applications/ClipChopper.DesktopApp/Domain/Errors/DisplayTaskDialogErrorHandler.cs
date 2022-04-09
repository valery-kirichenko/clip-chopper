using System;
using System.Windows;
using ClipChopper.Logging;

namespace ClipChopper.Domain.Errors
{
    internal sealed class DisplayTaskDialogErrorHandler : IErrorHandler
    {
        /// <summary>
        /// Logger instance for current class.
        /// </summary>
        private static readonly ILogger _logger =
            LoggerFactory.CreateLoggerFor<DisplayTaskDialogErrorHandler>();

        private readonly Window _window;


        public DisplayTaskDialogErrorHandler(
            Window window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
        }

        #region IErrorHandler Implementation

        public void HandleError(Exception ex)
        {
            _logger.Error(ex, $"Exception occurred during task execution.");
            TaskDialogHelper.ShowErrorTaskDialog(_window, ex);
        }

        #endregion
    }
}
