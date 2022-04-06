using System;
using System.Diagnostics;

namespace ClipChopper.Domain.Errors
{
    internal sealed class CommonErrorHandler : IErrorHandler
    {
        public CommonErrorHandler()
        {
        }

        #region IErrorHandler Implementation

        public void HandleError(Exception ex)
        {
            Debug.WriteLine($"Exception occurred during task execution.{Environment.NewLine}{ex}");
        }

        #endregion
    }
}
