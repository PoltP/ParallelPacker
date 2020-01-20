using System;
using System.Threading;
using ParallelPacker.Loggers;
using ParallelPacker.Settings;

namespace ParallelPacker {
    class Program {
        static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        static void Main(string[] args) {
            Console.CancelKeyPress += (s, e) => {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
            };

            Parameters parameters = new Parameters(args);
            ConsoleLogger logger = new ConsoleLogger();
            Packer.Run(parameters, cancellationTokenSource, logger);
        }
    }
}
