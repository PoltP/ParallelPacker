namespace ParallelPacker.Conveyers {
    public delegate void Put<T>(T item);

    public interface IPuttableConveyer<T> {
        void Open();
        void Close();
        void Put(T item);
    }
}
