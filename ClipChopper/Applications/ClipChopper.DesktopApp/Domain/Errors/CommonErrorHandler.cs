using System;
using ClipChopper.Logging;

namespace ClipChopper.Domain.Errors
{
    internal sealed class CommonErrorHandler : IErrorHandler
    {
        /// <summary>
        /// Logger instance for current class.
        /// </summary>
        private static readonly ILogger _logger =
            LoggerFactory.CreateLoggerFor<CommonErrorHandler>();


        public CommonErrorHandler()
        {
        }

        #region IErrorHandler Implementation

        public void HandleError(Exception ex)
        {
            _logger.Error(ex, $"Exception occurred during task execution.");
        }

        #endregion
    }
}
