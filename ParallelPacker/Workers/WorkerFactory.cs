using System.Collections.Generic;
using System.IO;
using ParallelPacker.Blocks;
using ParallelPacker.Conveyers;
using ParallelPacker.Loggers;
using ParallelPacker.Settings;

namespace ParallelPacker.Workers {
    public static class WorkerFactory {
        public static Worker<Block, Block> CreateSourceWorker(BinaryReader reader, int blocksNumber, int blockLength,
                PackerMode packerMode, IPuttableConveyer<Block> puttableConveyer, ILoggable logger) {

            IEnumerator<Block> enumerator;
            if (packerMode == PackerMode.Pack) {
                enumerator = BinaryBlockReader.CreateRawBlocksEnumerator(reader, blockLength);
            } else {
                enumerator = BinaryBlockReader.CreatePackedBlocksEnumerator(reader, blocksNumber);
            }         
            IGettableConveyer<Block> srcConveyer = new GetOnlyConveyer<Block>(() =>  enumerator.MoveNext() ? enumerator.Current : null);
            return new Worker<Block, Block>("Source", srcConveyer, puttableConveyer, logger, block => block);
        }

        public static Worker<Block, Block> CreatePackerWorker(int index, PackerMode packerMode, IPackable packable,
                IGettableConveyer<Block> gettableConveyer, IPuttableConveyer<Block> puttableConveyer, ILoggable logger) {

            Convert<Block, Block> convert;
            if (packerMode == PackerMode.Pack) {
                convert = sourceBlock => new Block(sourceBlock.Index, packable.Pack(sourceBlock.Data));
            } else {
                convert = sourceBlock => new Block(sourceBlock.Index, packable.Unpack(sourceBlock.Data));
            }
            return new Worker<Block, Block>($"{packerMode.ToString()}ing #{index}", gettableConveyer, puttableConveyer, logger, convert);
        }

        public static Worker<Block, Block> CreateDestinationWorker(BinaryWriter writer, int blocksNumber, int blockLength,
                PackerMode packerMode, IGettableConveyer<Block> gettableConveyer, ILoggable logger) {

            IPuttableConveyer<Block> puttableConveyer;
            if (packerMode == PackerMode.Pack) {
                BinaryBlockWriter.WriteInfo(writer, blocksNumber, blockLength);
                puttableConveyer = new PutOnlyConveyer<Block>((Block block) => {
                    BinaryBlockWriter.WritePackedBlock(writer, block);
                });
            } else {
                puttableConveyer = new PutOnlyConveyer<Block>((Block block) => {
                    BinaryBlockWriter.WriteBlock(writer, block, blockLength);
                });
            }
            return new Worker<Block, Block>("Destination", gettableConveyer, puttableConveyer, logger, block => block);
        }
    }
}
