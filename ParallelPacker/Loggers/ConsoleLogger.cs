using System;

namespace ParallelPacker.Loggers {
    public class ConsoleLogger : ILoggable {
        readonly bool allowDebugging;

        public ConsoleLogger(bool allowDebugging = false) {
            this.allowDebugging = allowDebugging;
        }

        void ILoggable.LogMessage(string logMessage, bool currentLine) {
            string message = $"{logMessage}";
            if (currentLine) {
                Console.Write(message);
            } else {
                Console.WriteLine(message);
            }
        }

        void ILoggable.LogError(string errorMessage, Exception exception) {
            ConsoleColor fgConsoleColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkRed; 
            Console.WriteLine(errorMessage);
            Console.WriteLine($"{exception.Message}");
            Console.ForegroundColor = fgConsoleColor;
        }

        void ILoggable.DebugError(string message, Exception exception) {
            if (!allowDebugging) return;
            Console.WriteLine($"[ERROR] {message} : {exception?.Message}");
        }

        void ILoggable.Debug(string message) {
            if (!allowDebugging) return;
            Console.WriteLine(message);
        }
    }
}
