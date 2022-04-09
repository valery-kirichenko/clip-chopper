using System;

namespace ClipChopper.Logging
{
    public interface ILogger
    {
        void PrintHeader(string message);
        void PrintFooter(string message);
        void Debug(string message);
        void Info(string message);
        void Warn(string message);
        void Warn(Exception ex, string message);
        void Error(string message);
        void Error(Exception ex, string message);
    }
}
