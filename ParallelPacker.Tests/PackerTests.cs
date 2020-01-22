using System;
using System.IO;
using Xunit;
using ParallelPacker.Settings;
using System.Threading;
using ParallelPacker.PackerEngines;
using ParallelPacker.Exceptions;

namespace ParallelPacker.Tests {
    public class PackerTests {
        LoggerMock logger = new LoggerMock();
        CancellationTokenSource token = new CancellationTokenSource();

        [Fact]
        public void SimpleDataEvenBytesPackUnpack_Test() {
            SimpleDataPackUnpack(1000, 100, Environment.ProcessorCount);
        }

        [Fact]
        public void SimpleDataOddBytesPackUnpack_Test() {
            SimpleDataPackUnpack(1001, 100, Environment.ProcessorCount);
        }

        [Fact]
        public void RandomDataPackUnpack_Test() {
            byte[] sourceData = new byte[1 << 24];
            new Random().NextBytes(sourceData);
            byte[] sourceDataAfterPackAndUnpack = PackAndUnpack(sourceData, 1 << 16, Environment.ProcessorCount);

            Assert.Equal(sourceData.Length, sourceDataAfterPackAndUnpack.Length);
            Assert.Equal(sourceData, sourceDataAfterPackAndUnpack);
        }

        [Fact]
        public void WorkerUserCancel_Test() {
            var bytes = new byte[] { 10, 20, 30, 40 };
            using (var sourceStream = new MemoryStream(bytes))
            using (var destinationStream = new MemoryStream()) {
                PackerEngineMock packer = new PackerEngineMock();
                PackerEngineMock.PackedChangedEventHandler packedChanged = (object sender, PackedChangedEventArgs e) => {
                    token.Cancel();
                };
                packer.PackedChanged += packedChanged;
                Assert.Equal(ExitStatus.CANCEL, Packer.Run(sourceStream, destinationStream, packer, PackerMode.Pack, 1, Environment.ProcessorCount, token, logger));
                packer.PackedChanged -= packedChanged;

                Assert.Empty(logger.LoggedErrors);
            }
        }

        [Fact]
        public void WorkerSingleWorkerError_Test() {
            string errorMessage = "Test";
            var bytes = new byte[] { 10, 20, 30, 40, 50 };
            using (var sourceStream = new MemoryStream(bytes))
            using (var destinationStream = new MemoryStream()) {
                int thrownFlag = 0;
                PackerEngineMock packer = new PackerEngineMock();
                PackerEngineMock.PackedChangedEventHandler packedChanged = (object sender, PackedChangedEventArgs e) => {
                    if (Interlocked.Exchange(ref thrownFlag, 1) == 0) {
                        throw new UnauthorizedAccessException(errorMessage);
                    }
                };
                packer.PackedChanged += packedChanged;
                Assert.Equal(ExitStatus.ERROR, Packer.Run(sourceStream, destinationStream, packer, PackerMode.Pack, 1, Environment.ProcessorCount, token, logger));
                packer.PackedChanged -= packedChanged;

                Assert.Single(logger.LoggedErrors);
                WorkersAggregateException aggregateException = logger.LoggedErrors[0] as WorkersAggregateException;
                Assert.Single(aggregateException.InnerExceptions);
                Assert.Equal(errorMessage, aggregateException.InnerExceptions[0].Message);
            }
        }

        [Fact]
        public void WorkerMultipleErrors_Test() {
            var bytes = new byte[] { 10, 20, 30, 40, 50 };
            using (var sourceStream = new MemoryStream(bytes))
            using (var destinationStream = new MemoryStream()) {
                int thrownCounter = 0;
                PackerEngineMock packer = new PackerEngineMock();
                PackerEngineMock.PackedChangedEventHandler packedChanged = (object sender, PackedChangedEventArgs e) => {
                    Interlocked.Increment(ref thrownCounter);
                    throw new UnauthorizedAccessException();
                };
                packer.PackedChanged += packedChanged;
                Assert.Equal(ExitStatus.ERROR, Packer.Run(sourceStream, destinationStream, packer, PackerMode.Pack, 1, Environment.ProcessorCount, token, logger));
                packer.PackedChanged -= packedChanged;

                Assert.Single(logger.LoggedErrors);
                WorkersAggregateException aggregateException = logger.LoggedErrors[0] as WorkersAggregateException;
                Assert.Equal(thrownCounter, aggregateException.InnerExceptions.Count);
            }
        }

        //[Fact]
        //public void GZip_Test() {
        //    byte[] sourceData = PrepareRandomData();
        //    System.Diagnostics.Stopwatch watcher = new System.Diagnostics.Stopwatch();
        //    watcher.Start();
        //    IPackerEngine gZipPacker = new GZipPacker();
        //    byte[] sourceDataAfterPackAndUnpack = gZipPacker.Unpack(gZipPacker.Pack(sourceData));
        //    watcher.Stop();
        //    Console.Write($"Finished in {watcher.Elapsed}");
        //    Assert.Equal(sourceDataAfterPackAndUnpack, sourceData);
        //}

        byte[] PrepareData(int length) {
            byte[] data = { 10, 20, 30, 40, 50, 0 };
            byte[] sourceData = new byte[length];
            for (int i = 0; i < sourceData.Length; ++i) {
                sourceData[i] = data[i % data.Length];
            }
            return sourceData;
        }

        void SimpleDataPackUnpack(int dataLength, int blockLength, int parallelismDegree) {
            byte[] sourceData = PrepareData(dataLength);
            byte[] sourceDataAfterPackAndUnpack = PackAndUnpack(sourceData, blockLength, parallelismDegree);
            Assert.Equal(sourceData.Length, sourceDataAfterPackAndUnpack.Length);
            Assert.Equal(sourceData, sourceDataAfterPackAndUnpack);
        }
        byte[] PackAndUnpack(byte[] sourceData, int blockLength = 1 << 16, int parallelismDegree = 1) {
            using (var sourceStream = new MemoryStream(sourceData))
            using (var destinationStream = new MemoryStream()) {
                IPackerEngine packer = new GZipPacker();
                Assert.Equal(ExitStatus.SUCCESS, Packer.Run(sourceStream, destinationStream, packer, PackerMode.Pack, blockLength, parallelismDegree, token, logger));

                using (var sourceStream2 = new MemoryStream(destinationStream.ToArray()))
                using (var destinationStream2 = new MemoryStream()) {
                    Assert.Equal(ExitStatus.SUCCESS, Packer.Run(sourceStream2, destinationStream2, packer, PackerMode.Unpack, blockLength, parallelismDegree, token, logger));
                    return destinationStream2.ToArray();
                }
            }
        }
    }
}
