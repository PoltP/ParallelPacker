namespace ParallelPacker.Blocks {
    public interface IPackable {
        byte[] Pack(byte[] data);
        byte[] Unpack(byte[] data);
    }
}