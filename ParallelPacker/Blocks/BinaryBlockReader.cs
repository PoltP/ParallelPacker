using System.Collections.Generic;
using System.IO;
using ParallelPacker.Exceptions;

namespace ParallelPacker.Blocks {
    public static class BinaryBlockReader {
        public static IEnumerator<Block> CreateRawBlocksEnumerator(BinaryReader reader, int blockLength) {
            return ReadRawBlocks(reader, blockLength).GetEnumerator();
        }
        public static IEnumerator<Block> CreatePackedBlocksEnumerator(BinaryReader reader, int blocksNumber) {
            return ReadPackedBlocks(reader, blocksNumber).GetEnumerator();
        }
        public static (int, int) ReadBlockInfo(BinaryReader reader) {
            int blocksNumber = reader.ReadInt32();
            int blockLength = reader.ReadInt32();
            if (blocksNumber <= 0 || blockLength < 1) {
                throw new ReadPackedDataException("Packed file has incorrect block info.");
            }
            return (blocksNumber, blockLength);
        }

        static IEnumerable<Block> ReadRawBlocks(BinaryReader reader, int blockLength) {
            int blockIndex = 0;
            bool isFinished = false;
            while (!isFinished) {
                long totalLength = reader.BaseStream.Length - reader.BaseStream.Position;
                isFinished = totalLength <= blockLength;

                byte[] data = reader.ReadBytes(isFinished ? (int)totalLength : blockLength);
                yield return new Block(blockIndex++, data);
            }
        }

        static IEnumerable<Block> ReadPackedBlocks(BinaryReader reader, int blocksNumber) {
            for (int i = 0; i < blocksNumber; ++i) {
                int blockIndex = reader.ReadInt32();
                int blockDataLength = reader.ReadInt32();
                long totalLength = reader.BaseStream.Length - reader.BaseStream.Position;
                if (blockIndex < 0 || blockDataLength <= 0 || blockDataLength > totalLength) {
                    throw new ReadPackedDataException();
                }

                byte[] data = reader.ReadBytes(blockDataLength);
                yield return new Block(blockIndex, data);
            }
        }
    }
}
