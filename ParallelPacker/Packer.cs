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
        public static int Run(Parameters parameters, CancellationTokenSource token, ILoggable logger) {
            using (BinaryReader sourceReader = new BinaryReader(parameters.SourceFileInfo.OpenRead())) {
                using (BinaryWriter destinationWriter = new BinaryWriter(parameters.DestinationFileInfo.OpenWrite())) {
                    return Run(sourceReader, destinationWriter, parameters.PackerMode, parameters.BlockLength,
                        parameters.ParallelismDegree, token, logger);
                }
            }
        }
        public static int Run(BinaryReader sourceReader, BinaryWriter destinationWriter,
                PackerMode packerMode, int blockLength, int parallelismDegree, CancellationTokenSource token, ILoggable logger) {

            int blocksNumber = (int)Math.Ceiling((double)sourceReader.BaseStream.Length / blockLength);
            Stopwatch watcher = new Stopwatch();
            watcher.Start();
            logger?.LogMessage("Parallel Packer started:");
            logger?.LogMessage($"    {packerMode.ToString().ToLower()}ing...");
            try {
                if (packerMode == PackerMode.Unpack) {// we should save blockLength into the packed file because of blockLength parameter can be shanged
                    (blocksNumber, blockLength) = BinaryBlockReader.ReadBlockInfo(sourceReader);
                }

                var commonSourceConveyer = new LockableConveyer<Block>();
                var commonDestinationConveyer = new LockableConveyer<Block>();

                var workers = new List<Worker<Block, Block>> {
                    WorkerFactory.CreateSourceWorker(sourceReader, blocksNumber, blockLength, packerMode, commonSourceConveyer, logger),
                    WorkerFactory.CreateDestinationWorker(destinationWriter, blocksNumber, blockLength, packerMode, commonDestinationConveyer, logger)
                };
                for (int index = 1; index <= parallelismDegree; ++index) {
                    workers.Add(WorkerFactory.CreatePackerWorker(index, packerMode, new GZipPacker(), commonSourceConveyer, commonDestinationConveyer, logger));
                }

                Worker<Block, Block>.DoWork(workers, token);

                watcher.Stop();
                if(token.IsCancellationRequested) {
                    logger?.LogMessage($"{packerMode}ing has been cancelled by user '{Environment.UserName}' after {watcher.Elapsed}");
                } else {
                    logger?.LogMessage($"{packerMode}ing has been finished successfully in {watcher.Elapsed}:");
                    logger?.LogMessage($"    total blocks number: {blocksNumber}");
                    logger?.LogMessage($"    raw block length: {blockLength}");
                }
                
                return 0;

            } catch(Exception e) {
                watcher.Stop();
                logger?.LogMessage($"{packerMode}ing finished with ERRORS in {watcher.Elapsed}:");
                logger?.LogMessage($"{e.Message}");
                return 1;
            }
        }
    }
}
