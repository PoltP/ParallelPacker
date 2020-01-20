using System.Threading;

namespace ParallelPacker.Conveyers {
    //
    //  Using Jeffrey Richter's "condition variable pattern"   
    //
    public class LockableConveyer<T> : ConveyerBase<T> {
        readonly object lockableObject = new object();

        public override void Close() {
            Monitor.Enter(lockableObject);
            try {
                base.Close();
                if (!HasPuttableWorkers) {
                    Monitor.PulseAll(lockableObject);
                }
            } finally {
                Monitor.Exit(lockableObject);
            }
        }

        public override T Get() {
            Monitor.Enter(lockableObject);
            try {
                while (!HasItems && (HasPuttableWorkers || !IsOpenedChanged)) {
                    Monitor.Wait(lockableObject);
                }
                return base.Get();
            } finally {
                Monitor.Exit(lockableObject);
            }
        }

        public override void Put(T item) {
            Monitor.Enter(lockableObject);
            try {
                base.Put(item);
                Monitor.Pulse(lockableObject);
            } finally {
                Monitor.Exit(lockableObject);
            }
        }
    }
}
