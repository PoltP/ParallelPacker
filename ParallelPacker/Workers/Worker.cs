using System;
using System.Threading;
using ParallelPacker.Conveyers;
using ParallelPacker.Loggers;

namespace ParallelPacker.Workers {
    public class Worker<TSource, TDestination> : IWorkable {
        readonly string name;
        readonly IGettableConveyer<TSource> sourceConveyer;
        readonly IPuttableConveyer<TDestination> destinationConveyer;
        readonly ILoggable logger;
        readonly Convert<TSource, TDestination> convert;
        Exception internalError;

        Exception IWorkable.InternalError {
            get { return internalError; }
        }

        Thread IWorkable.Start(CancellationTokenSource token) {
            internalError = null;
            Thread thread = new Thread(GetThreadAction(token)) { Name = name, IsBackground = true };
            thread.Start();
            return thread;
        }

        public Worker(string name, IGettableConveyer<TSource> sourceConveyer,
                IPuttableConveyer<TDestination> destinationConveyer, ILoggable logger,
                Convert<TSource, TDestination> convert) {
            this.name = name;
            this.sourceConveyer = sourceConveyer;
            this.destinationConveyer = destinationConveyer;
            this.logger = logger;
            this.convert = convert;
        }

        ThreadStart GetThreadAction(CancellationTokenSource token) {
            return () => {
                destinationConveyer.Open();
                try {
                    while (!token.IsCancellationRequested) {
                        TSource sourceData = sourceConveyer.Get(out bool stopped);
                        if (stopped) {
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