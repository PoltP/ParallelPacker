using System;
using System.IO;

namespace ParallelPacker.Settings {
    public class Parameters {
        public static int DefaultBlockLength = 1 << 20;// 1Mb
        public static readonly int DefaultParallelismDegree = Environment.ProcessorCount;

        public static Parameters Parse(string[] args) {
            try {
                if (args.Length < 2) {
                    throw new ArgumentOutOfRangeException("Incorrect arguments number");
                }

                Parameters parameters = new Parameters();
                string sourcePath = args[1];
                string destinationPath = args.Length > 2 ? args[2] : $"{sourcePath}-{parameters.PackerMode}ed";

                parameters.SourceFileInfo = new FileInfo(sourcePath);
                if (!parameters.SourceFileInfo.Exists)
                    throw new ArgumentException($"\"{sourcePath}\" source file does not exist");

                parameters.DestinationFileInfo = new FileInfo(destinationPath);
                //if (destinationFileInfo.Exists) {// TODO: ask to rewrite destination file

                parameters.PackerMode = (PackerMode)Enum.Parse(typeof(PackerMode), args[0].ToLower(), true);

                return parameters;
            } catch {
                return null;
            }
        }

        public FileInfo SourceFileInfo { get; protected set; }
        public FileInfo DestinationFileInfo { get; protected set; }
        public PackerMode PackerMode { get; protected set; }
        public int BlockLength { get { return DefaultBlockLength; } }
        public int ParallelismDegree { get { return DefaultParallelismDegree; } }

        private Parameters() { }
    }
}
