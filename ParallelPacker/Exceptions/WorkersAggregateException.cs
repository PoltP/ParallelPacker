using System;
using System.Collections.Generic;

namespace ParallelPacker.Exceptions {
    public class WorkersAggregateException : AggregateException {
        public WorkersAggregateException(IEnumerable<Exception> exceptions) : base("One or more errors occurred during the process.\n\r", exceptions) {
        }
    }
}
