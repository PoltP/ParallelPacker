using System;

namespace ParallelPacker.Loggers {
    public interface ILoggable {
        void LogMessage(string message, bool currentLine = false);
        void Debug(string message);
        void DebugError(string message, Exception exception);
    }
}
