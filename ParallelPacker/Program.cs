using System;
using System.Threading;
using ParallelPacker.Loggers;
using ParallelPacker.PackerEngines;
using ParallelPacker.Settings;

namespace ParallelPacker {
    class Program {
        static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        static void Main(string[] args) {
            Console.CancelKeyPress += (s, e) => {
                e.Cancel = true;
                cancellationTokenSource.Cancel();
            };
 
            Parameters parameters = Parameters.Setup(args);
            if (parameters != null) {   
                Packer.Run(parameters, cancellationTokenSource, new GZipPacker(), new ConsoleLogger());
            }
        }
    }
}
