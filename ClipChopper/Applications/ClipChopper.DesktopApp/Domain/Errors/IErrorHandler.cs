using System;

namespace ClipChopper.Domain.Errors
{
    internal interface IErrorHandler
    {
        void HandleError(Exception ex);
    }
}
