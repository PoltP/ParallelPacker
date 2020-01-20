using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ParallelPacker.Blocks;
using ParallelPacker.Conveyers;
using ParallelPacker.Loggers;
using ParallelPacker.Settings;
using ParallelPacker.Workers;

namespace ParallelPacker {
    public static class Packer {
        public static void Run(Parameters parameters, CancellationTokenSource token, ILoggable logger) {
            logger?.Write("Parallel Packer process has been started");

            FileInfo sourceFileInfo = new FileInfo(parameters.SourcePath);
            if (!sourceFileInfo.Exists)
                throw new ArgumentException($"\"{parameters.SourcePath}\" source file does not exist");
            FileInfo destinationFileInfo = new FileInfo(parameters.DestinationPath);
            //if (destinationFileInfo.Exists) {// TODO: ask to rewrite destination file

            using (BinaryReader sourceReader = new BinaryReader(sourceFileInfo.OpenRead())) {
                using (BinaryWriter destinationWriter = new BinaryWriter(destinationFileInfo.OpenWrite())) {
                    Run(sourceReader, destinationWriter, parameters.PackerMode, parameters.BlockLength,
                        parameters.ParallelismDegree, token, logger);
                }
            }
        }
        public static void Run(BinaryReader sourceReader, BinaryWriter destinationWriter,
                PackerMode packerMode, int blockLength, int parallelismDegree, CancellationTokenSource token, ILoggable logger) {

            Stopwatch watcher = new Stopwatch();
            watcher.Start();
            try {
                int blocksNumber = (int)Math.Ceiling((double)sourceReader.BaseStream.Length / blockLength);
                if (packerMode == PackerMode.Unpack) {// we should save blockLength into the packed file because of blockLength parameter can be shanged
                    (blocksNumber, blockLength) = BinaryBlockReader.ReadBlockInfo(sourceReader);
                }

                var sourceConveyer = new LockableConveyer<Block>();
                var destinationConveyer = new LockableConveyer<Block>();

                var workers = new List<Worker<Block, Block>> {
                    WorkerFactory.CreateSourceWorker(sourceReader, blocksNumber, blockLength, packerMode, sourceConveyer, logger),
                    WorkerFactory.CreateDestinationWorker(destinationWriter, blocksNumber, blockLength, packerMode, destinationConveyer, logger)
                };
                for (int index = 1; index <= parallelismDegree; ++index) {
                    workers.Add(WorkerFactory.CreatePackerWorker(index, packerMode, sourceConveyer, destinationConveyer, logger));
                }

                Worker<Block, Block>.DoWork(workers, token);

            } finally {
                watcher.Stop();
                logger?.Write($"{packerMode}ing finished in {watcher.Elapsed}");
            }
        }
    }
}
