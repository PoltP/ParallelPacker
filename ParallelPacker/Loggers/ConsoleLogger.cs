using System;

namespace ParallelPacker.Loggers {
    public class ConsoleLogger : ILoggable {
        //string threadName = Thread.CurrentThread.Name;
        void ILoggable.Error(string message, Exception exception) {
            Console.WriteLine($"[ERROR] {message} : {exception?.Message}");
        }

        void ILoggable.Write(string message) {
            Console.WriteLine(message);
        }
    }
}
