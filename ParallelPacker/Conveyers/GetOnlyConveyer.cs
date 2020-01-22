namespace ParallelPacker.Conveyers {
    public class GetOnlyConveyer<T> : IGettableConveyer<T> {
        readonly Get<T> get;

        public GetOnlyConveyer(Get<T> get) {
            this.get = get;
        }

        T IGettableConveyer<T>.Get(out bool stopped) {
            return get(out stopped);
        }
    }
}
