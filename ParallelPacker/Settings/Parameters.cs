using System;
namespace ParallelPacker.Settings {
    public class Parameters {
        public static int DefaultBlockLength = 1 << 20;// 1Mb
        public static readonly int DefaultParallelismDegree = Environment.ProcessorCount;

        public string SourcePath { get; private set; }
        public string DestinationPath { get; private set; }
        public PackerMode PackerMode { get; private set; }
        public int BlockLength { get { return DefaultBlockLength; } }
        public int ParallelismDegree { get { return DefaultParallelismDegree; } }

        public Parameters(string[] args) {
            if (args.Length < 2) {
                throw new ArgumentOutOfRangeException("Incorrect arguments number");
            }
            PackerMode = (PackerMode)Enum.Parse(typeof(PackerMode), args[0].ToLower(), true);
            SourcePath = args[1];
            DestinationPath = args[2] ?? $"{SourcePath}-packed.gz";
        }
    }
}
