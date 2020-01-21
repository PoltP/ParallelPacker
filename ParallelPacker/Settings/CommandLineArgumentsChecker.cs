using System;
using System.IO;
using ParallelPacker.Exceptions;

namespace ParallelPacker.Settings {
    public class CommandLineArgumentsChecker {
        public static Exception CheckErrors(string[] args) {
            try {
                if (args == null || args.Length != 3) {
                    throw CommandLineArgumentsException.CreateArgumentsNumberException(args != null ? args.Length : -1);
                }
                try {
                    Enum.Parse(typeof(PackerMode), args[0].ToLower(), true);
                } catch(Exception e) {
                    throw CommandLineArgumentsException.CreatePackerModeException(args[0], e);
                }

                CheckFile(args[1], true);
                CheckFile(args[2], false);

                return null;

            } catch (Exception e) {
                return e;
            }
        }

        static void CheckFile(string filePath, bool isSource) {
            try {
                string absolutePath = Path.GetFullPath(filePath);
                if (isSource) {
                    using (File.Open(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    }
                } else {
                    using (File.Open(absolutePath, FileMode.CreateNew, FileAccess.Write)) {
                        File.Delete(absolutePath);
                    }
                }
 
            } catch (Exception e) {
                throw CommandLineArgumentsException.CreateFileException(filePath, isSource, e);
            }
        }
    }
}
