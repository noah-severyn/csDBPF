using System;


namespace csDBPF {
    /// <summary>
    /// An Implementation of the QFS/RefPack/LZ77 compression format used in SC4 DBPF Files.
    /// </summary>
    /// <remarks>
    /// This implementation contains control characters and other changes specific to SimCity 4. For More information on file specification refer to: <br></br>
    /// - <see href="https://www.wiki.sc4devotion.com/index.php?title=DBPF_Compression"/><br></br>
    /// - <see href="http://wiki.niotso.org/RefPack"/> 
    /// </remarks>
    public static partial class QFS {
		/// <summary>
		/// All QFS compressed items will contain this signature in the file header.
		/// </summary>
		private const ushort QFS_Signature = 0xFB10;



		/// <summary>
		/// Check if the data is compressed.
		/// </summary>
		/// <param name="entryData">Data to check</param>
		/// <returns>TRUE if data is compressed; FALSE otherwise</returns>
		public static bool IsCompressed(byte[] entryData) {
			if (entryData.Length > 6) {
				if (BitConverter.ToUInt16(entryData, 4) == QFS_Signature) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns data's decompressed length in bytes.
		/// </summary>
		/// <param name="cData">Data to check</param>
		/// <returns>Size of decompressed data. If data is not compressed, the raw size is returned.</returns>
		public static uint GetDecompressedSize(byte[] cData) {
			if (IsCompressed(cData)) {
				//First 4 bytes is always the size of header + compressed data

				// Read 5 byte header
				byte[] header = new byte[5];
				for (int idx = 0; idx < 5; idx++) {
					header[idx] = cData[idx + 4];
				}

				// After QFS identifier, next 3 bytes are the decompressed size ... byte shift most significant byte to least
				return Convert.ToUInt32((header[2] << 16) + (header[3] << 8) + header[4]);
			} else {
				return (uint) cData.Length;
			}
		}

        /// <summary>
        /// Decompress data compressed with QFS/RefPack compression.
        /// </summary>
        /// <param name="sourceBytes">Compressed data array</param>
        /// <returns>Decompressed data array</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown when the compression algorithm tries to access an element that is out of bounds in the array
        /// </exception>
        public static byte[] Decompress(byte[] sourceBytes) {
			if (!IsCompressed(sourceBytes)) {
				return sourceBytes;
			}

			byte[] destinationBytes;
			int destinationPosition = 0;

			// Check first 4 bytes (size of header + compressed data)
			//uint compressedSize = BitConverter.ToUInt32(sourceBytes, 0);

			// Next read the 5 byte header
			byte[] header = new byte[5];
			for (int i = 0; i < 5; i++) {
				header[i] = sourceBytes[i + 4];
			}

			// First 2 bytes should be the QFS identifier
			// Next 3 bytes should be the uncompressed size of file
			// (we do this by byte shifting (from most significant byte to least))
			// the last 3 bytes of the header to make a number)
			uint uncompressedSize = Convert.ToUInt32((long) (header[2] << 16) + (header[3] << 8) + header[4]); ;

			// Create our destination array
			destinationBytes = new byte[uncompressedSize];

			// Next set our position in the file
			// (The if checks if the first 4 bytes are the size of the file
			// if so our start position is 4 bytes + 5 byte header if not then our
			// offset is just the header (5 bytes))
			//if ((sourceBytes[0] & 0x01) != 0)
			//{
			//    sourcePosition = 9;//8;
			//}
			//else
			//{
			//    sourcePosition = 5;
			//}

			// Above code is redundant for SimCity 4 saves as the QFS compressed files all have the same header length
			// (Check was throwing off start position and caused decompression to get buggered)
			int sourcePosition = 9;

			// In QFS the control character tells us what type of decompression operation we are going to perform (there are 4)
			// Most involve using the bytes proceeding the control byte to determine the amount of data that should be copied from what
			// offset. These bytes are labeled a, b and c. Some operations only use 1 proceeding byte, others can use 3
			byte ctrlByte1;
			byte ctrlByte2;
			byte ctrlByte3;
			byte ctrlByte4;
			int length;
			int offset;

			// Main decoding loop. Keep decoding while sourcePosition is in source array and position isn't 0xFC
			while ((sourcePosition < sourceBytes.Length) && (sourceBytes[sourcePosition] < 0xFC)) {
				// Read our packcode/control character
				ctrlByte1 = sourceBytes[sourcePosition];

				// Read bytes proceeding packcode
				ctrlByte2 = sourceBytes[sourcePosition + 1];
				ctrlByte3 = sourceBytes[sourcePosition + 2];

				// Check which packcode type we are dealing with
				// Control Characters 0 to 127 (2 byte length CC)
				if ((ctrlByte1 & 0x80) == 0) {
					// First we copy from the source array to the destination array
					length = ctrlByte1 & 3;
					LZCompliantCopy(ref sourceBytes, sourcePosition + 2, ref destinationBytes, destinationPosition, length);

					// Then we copy characters already in the destination array to our current position in the destination array
					sourcePosition += length + 2;
					destinationPosition += length;
					length = ((ctrlByte1 & 0x1C) >> 2) + 3;
					offset = ((ctrlByte1 >> 5) << 8) + ctrlByte2 + 1;
					LZCompliantCopy(ref destinationBytes, destinationPosition - offset, ref destinationBytes, destinationPosition, length);

					destinationPosition += length;
				}

				// Control Characters 128 to 191 (3 byte length CC)
				else if ((ctrlByte1 & 0x40) == 0) {
					length = (ctrlByte2 >> 6) & 3;
					LZCompliantCopy(ref sourceBytes, sourcePosition + 3, ref destinationBytes, destinationPosition, length);

					sourcePosition += length + 3;
					destinationPosition += length;
					length = (ctrlByte1 & 0x3F) + 4;
					offset = (ctrlByte2 & 0x3F) * 256 + ctrlByte3 + 1;
					LZCompliantCopy(ref destinationBytes, destinationPosition - offset, ref destinationBytes, destinationPosition, length);

					destinationPosition += length;
				}

				// Control Characters 192 to 223 (4 byte length CC)
				else if ((ctrlByte1 & 0x20) == 0) {
					ctrlByte4 = sourceBytes[sourcePosition + 3];

					length = ctrlByte1 & 3;
					LZCompliantCopy(ref sourceBytes, sourcePosition + 4, ref destinationBytes, destinationPosition, length);

					sourcePosition += length + 4;
					destinationPosition += length;
					length = ((ctrlByte1 >> 2) & 3) * 256 + ctrlByte4 + 5;
					offset = ((ctrlByte1 & 0x10) << 12) + 256 * ctrlByte2 + ctrlByte3 + 1;
					LZCompliantCopy(ref destinationBytes, destinationPosition - offset, ref destinationBytes, destinationPosition, length);

					destinationPosition += length;
				}

				// Control Characters 224 to 251 (1 byte length CC)
				else {
					length = (ctrlByte1 & 0x1F) * 4 + 4;
					LZCompliantCopy(ref sourceBytes, sourcePosition + 1, ref destinationBytes, destinationPosition, length);

					sourcePosition += length + 1;
					destinationPosition += length;
				}
			}

			// Add trailing bytes
			// Control Characters 252 to 255 (1 byte length CC)
			if ((sourcePosition < sourceBytes.Length) && (destinationPosition < destinationBytes.Length)) {
				LZCompliantCopy(ref sourceBytes, sourcePosition + 1, ref destinationBytes, destinationPosition, sourceBytes[sourcePosition] & 3);
				destinationPosition += sourceBytes[sourcePosition] & 3;
			}

			if (destinationPosition != destinationBytes.Length) {
				//Logger.Log(LogLevel.Warning, "QFS bad length, {0} instead of {1}", destinationPosition, destinationBytes.Length);
			}

			return destinationBytes;
		}



		/// <summary>
		/// Method that implements LZ compliant copying of data between arrays
		/// </summary>
		/// <param name="source">Array to copy from</param>
		/// <param name="sourceOffset">Position in array to copy from</param>
		/// <param name="destination">Array to copy to</param>
		/// <param name="destinationOffset">Position in array to copy to</param>
		/// <param name="length">Amount of data to copy</param>
		/// <remarks>
		/// With QFS (LZ77) we require an LZ compatible copy method between arrays, meaning we copy stuff one byte at a time between arrays. With LZ compatible algorithms, it is completely legal to copy over data that overruns the currently filled position in the destination array. In other words it is more than likely the we will be asked to copy over data that hasn't been copied yet. It's confusing, so we copy things one byte at a time.
		/// </remarks>
		/// <exception cref="System.IndexOutOfRangeException">
		/// Thrown when the copy method tries to access an element that is out of bounds in the array
		/// </exception>
		private static void LZCompliantCopy(ref byte[] source, int sourceOffset, ref byte[] destination, int destinationOffset, int length) {
			if (length != 0) {
				for (int i = 0; i < length; i++) {
					Buffer.BlockCopy(source, sourceOffset, destination, destinationOffset, 1);
					sourceOffset++;
					destinationOffset++;
				}
			}
		}
	}
}