namespace ParallelPacker.Blocks {
    public class Block {
        public int Index { get; private set; }
        public byte[] Data { get; private set; }

        public Block(int index, byte[] data) {
            Index = index;
            Data = data;
        }

        public override string ToString() {
            return $"[{Index}], {Data.Length} bytes";
        }
    }
}
