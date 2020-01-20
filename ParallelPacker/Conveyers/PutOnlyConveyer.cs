namespace ParallelPacker.Conveyers {
    public class PutOnlyConveyer<T> : IPuttableConveyer<T> {
        readonly Put<T> put;

        public PutOnlyConveyer(Put<T> put) {
            this.put = put;
        }

        void IPuttableConveyer<T>.Close() {
            // do nothing
        }

        void IPuttableConveyer<T>.Open() {
            // do nothing
        }

        void IPuttableConveyer<T>.Put(T item) {
            put(item);
        }
    }
}
