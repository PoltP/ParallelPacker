using System;

namespace ParallelPacker.Loggers {
    public interface ILoggable {
        void Write(string message);
        void Error(string message, Exception exception);
    }
}
