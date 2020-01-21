using System.IO.Compression;

namespace ParallelPacker.Settings {
    public enum PackerMode {
        Unpack = 0x0,
        Pack = 0x1,
        Decompress = 0x0,
        Compress = 0x1  
    }

    public static class PackerModeExtension {
        public static CompressionMode ToCompressionMode(this PackerMode engineMode) {
            return engineMode == PackerMode.Compress ? CompressionMode.Compress : CompressionMode.Decompress;
        }
    }
}