using System;
using Xunit;
using System.Threading;
using ParallelPacker.Conveyers;
using ParallelPacker.Workers;
using System.Collections.Generic;

namespace ParallelPacker.Tests {
    public class ConveyerTests {
        class SourceItem {
            public int Data { get; set; }
        }
        class DestinationItem {
            public int Data { get; set; }
        }

        CancellationTokenSource token = new CancellationTokenSource();

        [Fact]
        public void CommonConveyerWorkersLogic_Test() {
            var commonSourceConveyer = new LockableConveyer<SourceItem>();
            var commonDestinationConveyer = new LockableConveyer<DestinationItem>();
            byte[] sourceRawData = { 1, 2, 3, 4, 5 };

            int sourceRawDataIndex = 0;
            List<SourceItem> source = new List<SourceItem>();
            List<DestinationItem> destination = new List<DestinationItem>();

            IGettableConveyer<SourceItem> gettableConveyer = new GetOnlyConveyer<SourceItem>((out bool stopped) => {
                stopped = sourceRawDataIndex == sourceRawData.Length;
                return stopped ? null : new SourceItem() { Data = sourceRawData[sourceRawDataIndex++] };
            });
            IPuttableConveyer<DestinationItem> puttableConveyer = new PutOnlyConveyer<DestinationItem>((DestinationItem item) => {
                destination.Add(item);
            });
            var workers = new List<IWorkable> {
                new Worker<SourceItem, SourceItem>("Source", gettableConveyer, commonSourceConveyer, null, item => item),
                new Worker<DestinationItem, DestinationItem>("Destination", commonDestinationConveyer, puttableConveyer, null, item => item)
            };
            for (int index = 1; index <= Environment.ProcessorCount; ++index) {
                workers.Add(new Worker<SourceItem, DestinationItem>($"Worker #{index}", commonSourceConveyer, commonDestinationConveyer, null, (SourceItem item) => {
                    return new DestinationItem() { Data = item.Data };
                }));
            }

            WorkerFactory.DoWork(workers, token);

            Assert.Equal(sourceRawData.Length, destination.Count);
        }
    }
}
