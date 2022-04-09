using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ClipChopper.Domain.Errors;
using ClipChopper.Logging;

namespace ClipChopper.Domain
{
    internal static class TaskExtension
    {
        /// <summary>
        /// Logger instance for current class.
        /// </summary>
        private static readonly ILogger _logger =
            LoggerFactory.CreateLoggerFor(typeof(TaskExtension));


        public static async void FireAndForgetSafeAsync(this Task task,
            IErrorHandler? handler = null)
        {
            handler ??= new CommonErrorHandler();

            try
            {
                await task;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Exception occurred during sync execution.");
                handler.HandleError(ex);
            }
        }
    }
}
