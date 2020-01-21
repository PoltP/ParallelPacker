using System;
using System.IO;
using Xunit;
using ParallelPacker.Settings;
using ParallelPacker.Loggers;
using System.Threading;

namespace ParallelPacker.Tests {
    public class PackerTests {
        ILoggable logger = new ConsoleLogger(false);
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
        public void RandomDataEvenBytesPackUnpack_Test() {
            RandomDataPackUnpack(1 << 24, Environment.ProcessorCount);
        }

        [Fact]
        public void RandomDataOddBytesPackUnpack_Test() {
            RandomDataPackUnpack((1 << 24) + 1, Environment.ProcessorCount);
        }

        //[Fact]
        //public void GZip_Test() {
        //    byte[] sourceData = PrepareRandomData();
        //    System.Diagnostics.Stopwatch watcher = new System.Diagnostics.Stopwatch();
        //    watcher.Start();
        //    Blocks.IPackable gZipPacker = new Blocks.GZipPacker();
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

        byte[] PrepareRandomData(int length = (1 << 24) + 1) {
            byte[] data = { 102, 107, 124, 6, 0, 53, 35, 56, 43, 67 };
            byte[] sourceData = new byte[length];
            Random rnd = new Random();
            for (int i = 0; i < sourceData.Length; ++i) {
                sourceData[i] = data[rnd.Next(data.Length - 1)];
            }
            return sourceData;
        }
        void SimpleDataPackUnpack(int dataLength, int blockLength, int parallelismDegree) {
            byte[] sourceData = PrepareData(dataLength);
            byte[] sourceDataAfterPackAndUnpack = PackAndUnpack(sourceData, blockLength, parallelismDegree);
            Assert.Equal(sourceData.Length, sourceDataAfterPackAndUnpack.Length);
            Assert.Equal(sourceData, sourceDataAfterPackAndUnpack);
        }
        void RandomDataPackUnpack(int dataLength, int parallelismDegree) {
            byte[] sourceData = PrepareRandomData(dataLength);
            byte[] sourceDataAfterPackAndUnpack = PackAndUnpack(sourceData, 1 << 16, parallelismDegree);

            Assert.Equal(sourceData.Length, sourceDataAfterPackAndUnpack.Length);
            Assert.Equal(sourceData, sourceDataAfterPackAndUnpack);
        }
        byte[] PackAndUnpack(byte[] sourceData, int blockLength = 1 << 16, int parallelismDegree = 1) {
            using (var sourceStream = new MemoryStream(sourceData))
            using (var destinationStream = new MemoryStream())
            using (var stream = new MemoryStream())
            using (var sourceReader = new BinaryReader(sourceStream))
            using (var tempDestinationWriter = new BinaryWriter(stream))
            using (var tempReader = new BinaryReader(stream))
            using (var destinationWriter = new BinaryWriter(destinationStream)) {
                Packer.Run(sourceReader, tempDestinationWriter, PackerMode.Pack, blockLength, parallelismDegree, token, logger);

                tempDestinationWriter.Flush();
                tempDestinationWriter.Seek(0, SeekOrigin.Begin);
                Packer.Run(tempReader, destinationWriter, PackerMode.Unpack, blockLength, parallelismDegree, token, logger);
                return destinationStream.ToArray();
            }
        }
    }
}
