using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csDBPF {
    /// <summary>
    /// A set of extension methods for working with Streams and Readers
    /// </summary>
    public static class StreamExtensions {

        /// <summary>
        /// Reads a fixed-length string from the binary stream using ASCII encoding.
        /// </summary>
        /// <param name="br">The <see cref="BinaryReader"/> instance to read from.</param>
        /// <param name="count">The number of bytes to read. This value must be 0 or a non-negative number or an exception will occur.</param>
        /// <returns>
        /// A string decoded from the read bytes using ASCII encoding.
        /// </returns>
        public static string ReadString(this BinaryReader br, int count) {
            byte[] bytes = br.ReadBytes(count);
            return Encoding.ASCII.GetString(bytes);
        }


        /// <summary>
        /// Creates a new <see cref="MemoryStream"/> starting from a specified offset in the original BinaryReader's base stream.
        /// </summary>
        /// <param name="reader">The BinaryReader whose base stream will be sliced.</param>
        /// <param name="offset">The byte offset at which to start the substream.</param>
        /// <param name="length">Optional length of the substream. If null, uses the remaining bytes.</param>
        /// <returns>A new <see cref="MemoryStream"/> representing the specified segment.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if offset or length exceeds stream bounds.</exception>
        /// <exception cref="InvalidOperationException">Thrown in the base stream is not a MemoryStream</exception>
        public static BinaryReader GetStreamAt(this BinaryReader reader, int offset, int? length = null) {
            if (reader.BaseStream is not MemoryStream baseStream) {
                throw new InvalidOperationException("Substreaming is only supported on MemoryStream.");
            }
            byte[] buffer = baseStream.ToArray();

            if (offset < 0 || offset >= buffer.Length) {
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset is outside the bounds of the stream.");
            }
            int sliceLength = length ?? (buffer.Length - offset);
            if (offset + sliceLength > buffer.Length) {
                throw new ArgumentOutOfRangeException(nameof(length), "Substream length exceeds buffer bounds.");
            }
            var slice = new MemoryStream(buffer, offset, sliceLength, writable: false);
            return new BinaryReader(slice);
        }
    }
}
