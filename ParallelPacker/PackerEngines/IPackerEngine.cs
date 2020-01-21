namespace ParallelPacker.PackerEngines {
    public interface IPackerEngine {
        byte[] Pack(byte[] data);
        byte[] Unpack(byte[] data);
    }
}