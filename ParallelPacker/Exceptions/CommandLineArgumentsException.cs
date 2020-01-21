using System;

namespace ParallelPacker.Exceptions {
    public class CommandLineArgumentsException : ArgumentException {
        public static Exception CreateArgumentsNumberException(int argumentsNumber) {
            return new CommandLineArgumentsException($"Arguments number '{argumentsNumber}' is incorrect, use one of the following syntaxes" +
                "\n   ParallelPacker [compress|decompress] [source file path] [result file path]\n   ParallelPacker [pack|unpack] [source file path] [result file path]");
        }

        public static  Exception CreatePackerModeException(string packerMode, Exception innerException) {
            return new CommandLineArgumentsException($"Unknown packer mode '{packerMode}', use 'compress' ('pack') or 'decompress' ('unpack')", innerException);
        }

        public static Exception CreateFileException(string filePath, bool isSource, Exception innerException) {
            string name = isSource ? "Source" : "Destination";
            return new CommandLineArgumentsException($"{name} file \"{filePath}\" access failed:\n\r{innerException.Message}", innerException);
        }

        CommandLineArgumentsException(string message, Exception innerException) : base(message, innerException) {
        }
        CommandLineArgumentsException(string message) : this(message, null) {
        }
    }
}
