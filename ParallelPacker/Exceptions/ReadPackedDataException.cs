using System;

namespace ParallelPacker.Exceptions {
    public class ReadPackedDataException : SystemException {
        public ReadPackedDataException(string message = "Packed file is corrupted.") : base(message) {
        }
    }
}
