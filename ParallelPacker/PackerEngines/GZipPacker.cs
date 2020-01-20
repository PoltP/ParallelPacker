using System.IO;
using System.IO.Compression;

namespace ParallelPacker.Blocks {
    public class GZipPacker : IPackable {
        byte[] IPackable.Pack(byte[] data) {
            using (MemoryStream packedStream = new MemoryStream(data.Length)) {
                using (GZipStream gZip = new GZipStream(packedStream, CompressionMode.Compress)) {
                    gZip.Write(data, 0, data.Length);
                }
                return packedStream.ToArray();
            }
        }

        byte[] IPackable.Unpack(byte[] data) {
            using (MemoryStream packedStream = new MemoryStream(data)) {
                using (MemoryStream unpackedStream = new MemoryStream()) {
                    using (GZipStream gZip = new GZipStream(packedStream, CompressionMode.Decompress)) {
                        gZip.CopyTo(unpackedStream);
                    }
                    return unpackedStream.ToArray();
                }
            }
        }
    }
}
