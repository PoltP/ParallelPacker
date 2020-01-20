namespace ParallelPacker.Conveyers {
    public delegate T Get<T>();

    public interface IGettableConveyer<T> {
        T Get();
    }
}
