namespace ParallelPacker.Conveyers {
    public delegate T Get<T>(out bool stopped);

    public interface IGettableConveyer<T> {
        T Get(out bool stopped);
    }
}
