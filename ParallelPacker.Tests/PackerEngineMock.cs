using System;
using ParallelPacker.PackerEngines;
using ParallelPacker.Settings;

namespace ParallelPacker.Tests {
    public class PackedChangedEventArgs {
        public PackedChangedEventArgs(PackerMode mode, int count) {
            Mode = mode;
            Count = count;
        }
        public PackerMode Mode { get; private set; }
        public int Count { get; private set; }
    }

    public class PackerEngineMock : IPackerEngine {
        public delegate void PackedChangedEventHandler(object sender, PackedChangedEventArgs e);

        public event PackedChangedEventHandler PackedChanged;

        public int PackedCount {
            get; private set;
        }

        public int UnpackedCount {
            get; private set;
        }

        protected virtual void RaisePackedChangedEvent(PackerMode mode, int count) {
            PackedChanged?.Invoke(this, new PackedChangedEventArgs(mode, count));
        }

        public PackerEngineMock() {
        }

        byte[] IPackerEngine.Pack(byte[] data) {
            PackedCount++;
            RaisePackedChangedEvent(PackerMode.Pack, PackedCount);
            return data;
        }

        byte[] IPackerEngine.Unpack(byte[] data) {
            UnpackedCount++;
            RaisePackedChangedEvent(PackerMode.Unpack, PackedCount);
            return data;
        }
    }
}
