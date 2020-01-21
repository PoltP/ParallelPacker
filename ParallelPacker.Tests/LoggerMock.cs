using System;
using System.Collections.Generic;
using ParallelPacker.Loggers;

namespace ParallelPacker.Tests {
    public class LoggerMock : ILoggable {
        readonly List<Exception> loggedErrors = new List<Exception>();

        public List<Exception> LoggedErrors {
            get {
                return loggedErrors;
            }
        }

        void ILoggable.Debug(string message) {
        }

        void ILoggable.DebugError(string message, Exception exception) {
        }

        void ILoggable.LogError(string message, Exception exception) {
            loggedErrors.Add(exception);
        }

        void ILoggable.LogMessage(string message, bool currentLine) {
        }
    }
}
