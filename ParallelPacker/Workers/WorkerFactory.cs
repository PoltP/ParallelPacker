using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ParallelPacker.Blocks;
using ParallelPacker.Conveyers;
using ParallelPacker.Exceptions;
using ParallelPacker.Loggers;
using ParallelPacker.PackerEngines;
using ParallelPacker.Settings;

namespace ParallelPacker.Workers {
    public static class WorkerFactory {
        public static void DoWork(IEnumerable<IWorkable> workers, CancellationTokenSource token) {
            Thread[] workerThreads = workers.Select(worker => worker.Start(token)).ToArray();
            foreach (var thread in workerThreads) {
                thread.Join();
            }
            var errors = workers
                .Select(worker => worker.InternalError)
                .Where(error => error != null)
                .ToArray();
            if (errors.Length > 0) {
                throw new WorkersAggregateException(errors);
            }
        }

        public static Worker<Block, Block> CreateSourceWorker(BinaryReader reader, int blocksNumber, int blockLength,
                PackerMode packerMode, IPuttableConveyer<Block> puttableConveyer, ILoggable logger) {

            IEnumerator<Block> enumerator;
            if (packerMode == PackerMode.Pack) {
                enumerator = BinaryBlockReader.CreateRawBlocksEnumerator(reader, blockLength);
            } else {
                enumerator = BinaryBlockReader.CreatePackedBlocksEnumerator(reader, blocksNumber);
            }         
            IGettableConveyer<Block> gettableConveyer = new GetOnlyConveyer<Block>((out bool stopped) => {
                stopped = !enumerator.MoveNext();
                return stopped ? null : enumerator.Current;
            });
            return new Worker<Block, Block>("Source", gettableConveyer, puttableConveyer, logger, block => block);
        }

        public static Worker<Block, Block> CreatePackerWorker(int index, PackerMode packerMode, IPackerEngine packerEngine,
                IGettableConveyer<Block> gettableConveyer, IPuttableConveyer<Block> puttableConveyer, ILoggable logger) {

            Convert<Block, Block> convert;
            if (packerMode == PackerMode.Pack) {
                convert = sourceBlock => new Block(sourceBlock.Index, packerEngine.Pack(sourceBlock.Data));
            } else {
                convert = sourceBlock => new Block(sourceBlock.Index, packerEngine.Unpack(sourceBlock.Data));
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
