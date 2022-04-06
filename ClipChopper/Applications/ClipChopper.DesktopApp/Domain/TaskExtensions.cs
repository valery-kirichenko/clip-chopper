using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ClipChopper.Domain.Errors;

namespace ClipChopper.Domain
{
    internal static class TaskExtension
    {
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
                Debug.WriteLine($"Exception occurred during sync execution.{ex}");
                handler.HandleError(ex);
            }
        }
    }
}
