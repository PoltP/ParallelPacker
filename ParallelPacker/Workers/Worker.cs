using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ParallelPacker.Conveyers;
using ParallelPacker.Loggers;

namespace ParallelPacker.Workers {
    public class Worker<TSource, TDestination> {
        public static void DoWork(IEnumerable<Worker<TSource, TDestination>> workers, CancellationTokenSource token) {
            foreach (var worker in workers) {
                worker.Start(token);
            }
            foreach (var worker in workers) {
                worker.Await();
            }
            var errors = workers
                .Select(worker => worker.internalError)
                .Where(error => error != null)
                .ToArray();
            if (errors.Length > 0) {
                throw new AggregateException(errors);
            }
        }

        readonly string name;
        readonly IGettableConveyer<TSource> sourceConveyer;
        readonly IPuttableConveyer<TDestination> destinationConveyer;
        readonly ILoggable logger;
        readonly Convert<TSource, TDestination> convert;
        Exception internalError;
        Thread thread;

        public Worker(string name, IGettableConveyer<TSource> sourceConveyer,
                IPuttableConveyer<TDestination> destinationConveyer, ILoggable logger,
                Convert<TSource, TDestination> convert) {
            this.name = name;
            this.sourceConveyer = sourceConveyer;
            this.destinationConveyer = destinationConveyer;
            this.logger = logger;
            this.convert = convert;
        }

        protected void Start(CancellationTokenSource token) {
            thread = new Thread(GetThreadAction(token)) { Name = name, IsBackground = true };
            thread.Start();
        }

        protected void Await() {
            thread.Join();
        }

        ThreadStart GetThreadAction(CancellationTokenSource token) {
            return () => {
                destinationConveyer.Open();
                try {
                    while (!token.IsCancellationRequested) {
                        TSource sourceData = sourceConveyer.Get();
                        if (Object.Equals(sourceData, default(TSource))) {
                            logger?.Debug($"{name} : worker completed");
                            break;
                        }

                        TDestination convertedData = convert(sourceData);
                        destinationConveyer.Put(convertedData);
                        logger?.Debug($"{name} : {sourceData.ToString()} has been converted to {convertedData.ToString()}");
                    }
                } catch (Exception e) {
                    token.Cancel();
                    internalError = e;
                    logger?.DebugError($"{name} : failed", e);
                } finally {
                    destinationConveyer.Close();
                }
            };
        }
    }
}