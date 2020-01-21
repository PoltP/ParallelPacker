using System;
using System.IO;
using Xunit;
using ParallelPacker.Settings;
using ParallelPacker.Exceptions;

namespace ParallelPacker.Tests {
    public class CommandLineArgumentsTests {
        string sourceFileName = "src.pkr";
        string destinationFileName = "dest.pkr";

        public CommandLineArgumentsTests() {
            File.Delete(destinationFileName);// if it is exist
            using (FileStream sourceFile = File.Open(sourceFileName, FileMode.OpenOrCreate)) {
                sourceFile.Close();
            }
        }

        [Fact]
        public void CorrectPackerModePack_Test() {
            Assert.Null(CommandLineArgumentsChecker.CheckErrors(new string[] { "pack", sourceFileName, destinationFileName }));
        }
        [Fact]
        public void CorrectPackerModeUnpack_Test() {
            Assert.Null(CommandLineArgumentsChecker.CheckErrors(new string[] { "unpack", sourceFileName, destinationFileName }));
        }
        [Fact]
        public void CorrectPackerModeCompress_Test() {
            Assert.Null(CommandLineArgumentsChecker.CheckErrors(new string[] { "compress", sourceFileName, destinationFileName }));
        }
        [Fact]
        public void CorrectPackerModeDecompress_Test() {
            Assert.Null(CommandLineArgumentsChecker.CheckErrors(new string[] { "decompress", sourceFileName, destinationFileName }));
        }
        [Fact]
        public void LessNumberOfArguments_Test() {
            Exception e = CommandLineArgumentsChecker.CheckErrors(new string[] { "pack", sourceFileName });
            Assert.IsType<CommandLineArgumentsException>(e);
            Assert.Null(e.InnerException);
        }
        [Fact]
        public void MoreNumberOfArguments_Test() {
            Exception e = CommandLineArgumentsChecker.CheckErrors(new string[] { "pack", sourceFileName, destinationFileName, "test" });
            Assert.IsType<CommandLineArgumentsException>(e);
            Assert.Null(e.InnerException);
        }
        [Fact]
        public void IncorrectPackerMode_Test() {
            Exception e = CommandLineArgumentsChecker.CheckErrors(new string[] { "pak", sourceFileName, destinationFileName });
            Assert.IsType<CommandLineArgumentsException>(e);
            Assert.IsType<ArgumentException>(e.InnerException);
        }
        [Fact]
        public void SourceNotFound_Test() {
            Exception e = CommandLineArgumentsChecker.CheckErrors(new string[] { "pack", "I_believe_there_is_no_file_with_this_name", destinationFileName });
            Assert.IsType<CommandLineArgumentsException>(e);
            Assert.IsType<FileNotFoundException>(e.InnerException);
        }
        [Fact]
        public void DestinationIllegalCharactersName_Test() {
            Exception e = CommandLineArgumentsChecker.CheckErrors(new string[] { "pack", sourceFileName, "\0" + destinationFileName });
            Assert.IsType<CommandLineArgumentsException>(e);
            Assert.IsType<ArgumentException>(e.InnerException);
        }
        [Fact]
        public void DestinationAlreadyExist_Test() {
            try {
                using (FileStream destinationFile = File.Open(destinationFileName, FileMode.OpenOrCreate)) {
                    Exception e = CommandLineArgumentsChecker.CheckErrors(new string[] { "pack", sourceFileName, destinationFileName });
                    Assert.IsType<CommandLineArgumentsException>(e);
                    Assert.IsType<IOException>(e.InnerException);
                    destinationFile.Close();
                }
            } finally {
                File.Delete(destinationFileName);
            }
        }
    }
}
