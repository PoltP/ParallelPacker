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

            ILoggable logger = new ConsoleLogger();
            Parameters parameters = Parameters.Parse(args);
            if (parameters != null) {   
                Packer.Run(parameters, cancellationTokenSource, logger);
            } else {
                logger.LogMessage("Input parameters are incorrect, use one of the following forms:");
                logger.LogMessage("   ParallelPacker compress/decompress [source file path] [result file path]");
                logger.LogMessage("   ParallelPacker pack/unpack [source file path] [result file path]");
            }
        }
    }
}
