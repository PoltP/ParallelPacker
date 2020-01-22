using System;
using System.Threading;

namespace ParallelPacker.Workers {
    public interface IWorkable {
        Thread Start(CancellationTokenSource token);

        Exception InternalError { get; }
    }
}
