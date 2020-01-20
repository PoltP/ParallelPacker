using System.IO;

namespace ParallelPacker.Blocks {
    public static class BinaryBlockWriter {
        public static void WriteInfo(BinaryWriter writer, int blocksNumber, int blockLength) {
            writer.Write(blocksNumber);
            writer.Write(blockLength);
        }

        public static void WriteBlock(BinaryWriter writer, Block block, int blockLength) {
            writer.BaseStream.Seek(block.Index * (long)blockLength, SeekOrigin.Begin);
            writer.Write(block.Data);
        }

        public static void WritePackedBlock(BinaryWriter writer, Block block) {
            writer.Write(block.Index);
            writer.Write(block.Data.Length);
            writer.Write(block.Data);
        }
    }
}
