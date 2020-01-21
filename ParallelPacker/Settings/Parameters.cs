using System;
using System.IO;

namespace ParallelPacker.Settings {
    public class Parameters {
        public static int DefaultBlockLength = 1 << 20;// 1Mb
        public static readonly int DefaultParallelismDegree = Environment.ProcessorCount;

        public static Parameters Setup(string[] args) {
            Exception argumentsCheckingError = CommandLineArgumentsChecker.CheckErrors(args);

            if(argumentsCheckingError != null) {
                Console.WriteLine("Parallel Packer\n\r");
                ConsoleColor fgConsoleColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Error on parsing input parameters\n\r" + argumentsCheckingError.Message);
                Console.ForegroundColor = fgConsoleColor;
                return null;
            }

            return new Parameters() {
                PackerMode = (PackerMode)Enum.Parse(typeof(PackerMode), args[0].ToLower(), true),
                SourceFileInfo = new FileInfo(args[1]),
                DestinationFileInfo = new FileInfo(args[2])
            };
        }

        public FileInfo SourceFileInfo { get; protected set; }
        public FileInfo DestinationFileInfo { get; protected set; }
        public PackerMode PackerMode { get; protected set; }
        public int BlockLength { get { return DefaultBlockLength; } }
        public int ParallelismDegree { get { return DefaultParallelismDegree; } }

        private Parameters() { }
    }
}