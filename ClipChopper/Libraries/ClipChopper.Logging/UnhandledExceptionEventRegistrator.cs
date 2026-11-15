using System;
using Acolyte.Assertions;

namespace ClipChopper.Logging
{
    public static class UnhandledExceptionEventRegistrator
    {
        public static void Register(UnhandledExceptionEventHandler eventHandler)
        {
            eventHandler.ThrowIfNull(nameof(eventHandler));

            AppDomain.CurrentDomain.UnhandledException += eventHandler;
        }
    }
}
