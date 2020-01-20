using System.Collections.Generic;
using System.Threading;

namespace ParallelPacker.Conveyers {
    public delegate TDestination Convert<TSource, TDestination>(TSource source);

    public class ConveyerBase<T> : IGettableConveyer<T>, IPuttableConveyer<T> {
        readonly Queue<T> queue = new Queue<T>();
        int puttableWorkersNumber;

        protected Queue<T> InternalQueue { get { return queue; } }

        protected bool HasItems { get { return queue.Count > 0; } }

        protected bool HasPuttableWorkers { get { return puttableWorkersNumber > 0; } }

        protected bool IsOpenedChanged { get; private set; }


        public void Open() {
            Interlocked.Increment(ref puttableWorkersNumber);
            IsOpenedChanged = true;
        }

        public virtual void Close() {
            Interlocked.Decrement(ref puttableWorkersNumber);
        }

        public virtual void Put(T item) {
            queue.Enqueue(item);
        }

        public virtual T Get() {
            if (queue.Count > 0) {
                return queue.Dequeue();
            }
            return default(T);
        }
    }
}
