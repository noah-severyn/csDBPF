using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;


namespace csDBPF {
	/// <summary>
	/// Implementation of the QFS/RefPack/LZ77 compression and decompression format.
	/// </summary>
	/// <see cref="http://wiki.niotso.org/RefPack"/>
	/// <seealso cref="https://www.wiki.sc4devotion.com/index.php?title=DBPF_Compression"/>
	public class DBPFCompression {
		//Anything prefixed with "c" refers to compressed (e.g. cData = compressedData), and a "d" prefix refers to decompressed (e.g. dData = decompressedData) 
		private const ushort QFS = 0xFB10;


		/// <summary>
		/// Check if the data is compressed.
		/// </summary>
		/// <param name="entryData">Data to check</param>
		/// <returns>TRUE if data is compressed; FALSE otherwise</returns>
		public static bool IsCompressed(byte[] entryData) {
			if (entryData.Length > 6) {

				ushort signature = BitConverter.ToUInt16(entryData, 4); //ToUint32(entryData,2) would otherwise return 0xFB10 0000, but we're only interested in 0xFB10
				if (signature == DBPFCompression.QFS) {
					//Memo's message: "there is an s3d file in SC1.dat which would otherwise return true on uncompressed data; this workaround is not fail proof"
					//https://github.com/memo33/jDBPFX/blob/fa2535c51de80df48a7f62b79a376e25274998c0/src/jdbpfx/util/DBPFPackager.java#L54
					string fileType = ByteArrayHelper.ToAString(entryData, 0, 4);
					if (fileType.Equals("3DMD")) { //3DMD = 0x3344 4D44 = 860114244
						return false;
					}
					return true;
				}
			}
			return false;
		}


		/// <summary>
		/// Returns the length of the data array in bytes. If data is compressed, the uncompressed size is returned. If data is not compressed, the raw size is returned.
		/// </summary>
		/// <param name="cData">Data to check</param>
		/// <returns>Size of data</returns>
		public static uint GetDecompressedSize(byte[] cData) {
			if (IsCompressed(cData)) {
				uint compressedSize = BitConverter.ToUInt32(cData, 0); //first 4 bytes is always the size of header + compressed data

				//read 5 byte header
				byte[] header = new byte[5];
				for (int idx = 0; idx < 5; idx++) {
					header[idx] = cData[idx + 4];
				}

				//After QFS identifier, next 3 bytes are the decompressed size ... byte shift most significant byte to least
				uint decompressedSize = Convert.ToUInt32((header[2] << 16) + (header[3] << 8) + header[4]);
				return decompressedSize;

			} else {
				return (uint) cData.Length;
			}
		}


		/// <summary>
		/// Decompress the provided data. If data is not compressed, the same data will be returned.
		/// </summary>
		/// <param name="data">Compressed</param>
		/// <returns>Decompressed data</returns>
		public static byte[] Decompress(byte[] cData) {
			//If data is not compressed do not run it through the algorithm otherwise it will return junk data
			if (!IsCompressed(cData)) {
				return cData;
			}

			byte[] dData = new byte[GetDecompressedSize(cData)]; //Set destination array of decompressed data
			int dPos = 0;

			byte ctrlByte1 = 0; //The control character (CC) determines which type of decompression algorithm needs to be performed; overall CC can be 1 to 4 bytes, depending on byte1 of CC
			int cPos = 9; //bytes 0-1 are header identifier, 2-4 are uncompressed size, 5-8 are unused by SC4

			while (cPos < cData.Length && ctrlByte1 < 0xFC) {
				ctrlByte1 = cData[cPos]; //this is byte0 = the first byte of the CC
				cPos++;

				// Control Characters 0 to 127 (2 byte length CC)
				if (ctrlByte1 >= 0x00 && ctrlByte1 <= 0x7F) {
					byte ctrlByte2 = cData[cPos];
					cPos++;

					//Copy from the source array to the destination array
					int numberPlainText = ctrlByte1 & 0x03;
					LZCompliantCopy(ref cData, cPos, ref dData, dPos, numberPlainText);

					//Copy characters already in the destination array to the current position in the destination array
					cPos += numberPlainText;
					dPos += numberPlainText;
					int offset = ((ctrlByte1 & 0x60) << 3) + ctrlByte2 + 1;
					int length = ((ctrlByte1 & 0x1C) >> 2) + 3; //Number of chars that should be copied from somewhere in the already decoded output and added to the end of the output.
					LZCompliantCopy(ref dData, dPos - offset, ref dData, dPos, length);
					dPos += length;
				}

				// Control Characters 128 to 191 (3 byte length CC)
				else if (ctrlByte1 >= 0x80 && ctrlByte1 <= 0xBF) {
					byte ctrlByte2 = cData[cPos];
					cPos++;
					byte ctrlByte3 = cData[cPos];
					cPos++;

					//Copy from the source array to the destination array
					int numberOfPlainText = (ctrlByte2 >> 6) & 0x03;
					LZCompliantCopy(ref cData, cPos, ref dData, dPos, numberOfPlainText);
					cPos += numberOfPlainText;
					dPos += numberOfPlainText;

					//Copy characters already in the destination array to the current position in the destination array
					int offset = ((ctrlByte2 & 0x3F) << 8) + (ctrlByte3) + 1;
					int length = (ctrlByte1 & 0x3F) + 4;
					LZCompliantCopy(ref dData, dPos - offset, ref dData, dPos, length);
					dPos += length;
				}

				// Control Characters 192 to 223 (4 byte length CC)
				else if (ctrlByte1 >= 0xC0 && ctrlByte1 <= 0xDF) {
					byte ctrlByte2 = cData[cPos];
					cPos++;
					byte ctrlByte3 = cData[cPos];
					cPos++;
					byte ctrlByte4 = cData[cPos];
					cPos++;

					//Copy from the source array to the destination array
					int numberOfPlainText = (ctrlByte1 & 0x03);
					LZCompliantCopy(ref cData, cPos, ref dData, dPos, numberOfPlainText);
					cPos += numberOfPlainText;
					dPos += numberOfPlainText;

					//Copy characters already in the destination array to the current position in the destination array
					int offset = ((ctrlByte1 & 0x10) << 12) + (ctrlByte2 << 8) + (ctrlByte3) + 1;
					int length = ((ctrlByte1 & 0x0C) << 6) + (ctrlByte4) + 5;
					LZCompliantCopy(ref dData, dPos - offset, ref dData, dPos, length);
					dPos += length;
				}

				// Control Characters 224 to 251 (1 byte length CC)
				else if (ctrlByte1 >= 0xE0 && ctrlByte1 <= 0xFB) {
					int numberOfPlainText = ((ctrlByte1 & 0x1F) << 2) + 4;
					LZCompliantCopy(ref cData, cPos, ref dData, dPos, numberOfPlainText);
					cPos += numberOfPlainText;
					dPos += numberOfPlainText;
				}

				// Control Characters 252 to 255 (1 byte length CC)
				else {
					int numberOfPlainText = (ctrlByte1 & 0x03);
					LZCompliantCopy(ref cData, cPos, ref dData, dPos, numberOfPlainText);
					cPos += numberOfPlainText;
					dPos += numberOfPlainText;
				}
			}
			return dData;
		}


		/// <summary>
		/// Compress the provided data.
		/// </summary>
		/// <param name="dData">Uncompressed data</param>
		/// <returns>Compressed data</returns>
		/// <see cref="https://github.com/memo33/jDBPFX/blob/master/src/jdbpfx/util/DBPFPackager.java#L170"/>
		public static byte[] Compress(byte[] dData) {
			//check if data is big enough to compress
			if (dData.Length < 6) {
				return dData;
			}
			//check if data is already compressed
			if (IsCompressed(dData)) {
				return dData;
			}

			//Performance calibration constants for compression
			//TODO - what do these values actually do???
			const int MAXOFFSET = 0x20000;
			const int MAXCOPYCOUNT = 0x404;
			const int QFSMAXITER = 0X80; //used to fine tune the lookup: small values increase the compression for big files


			Dictionary<int, ArrayList> cmpmap = new Dictionary<int, ArrayList>(); //contains the latest offset for a combination of two characters
			byte[] cData = new byte[dData.Length + MAXCOPYCOUNT]; //max size = uncompressedSize + MAXCOPYCOUNT


			//Write the header. First 4 bytes are size of header and compressed data. We obviously don't know this yet so skip. Bytes 5-6 are compression code, then bytes 7-9 bytes the decompressed size
			cData[4] = 0x10;
			cData[5] = 0xFB;
			cData[6] = (byte) (dData.Length >> 16);
			cData[7] = (byte) (dData.Length >> 8);
			cData[8] = (byte) dData.Length;

			int cPos = 9; //represents the current position in cData


			int writeIdx = 9; //leave 9 bytes for the header
			int lastReadIdx = 0;
			ArrayList locsOfCurrentIdx; //DO NOT USE ARRAY LIST USE LIST<T> INSTEAD
			ArrayList ret;


			int copyOffset = 0;
			int copyCount = 0;
			int idx = 0;
			bool end = false;

			//main compression loop
			while (idx < dData.Length - 3) {
				//get all compression candidates (list of offsets for all occurrences of the current 3 bytes
				do {
					idx++;
					if (idx >= dData.Length - 2) {
						end = true;
						break;
					}
					int mapindex = dData[idx] | (dData[idx + 1] << 8) | (dData[idx + 2] << 16);
					cmpmap.TryGetValue(mapindex, out locsOfCurrentIdx);
					if (locsOfCurrentIdx == null) {
						locsOfCurrentIdx = new ArrayList();
						cmpmap.Add(mapindex, locsOfCurrentIdx);
					}
					locsOfCurrentIdx.Add(idx);
				} while (idx < lastReadIdx);
				if (end) {
					break;
				}

				//find the longest repeating byte sequence in the index list (for offset copy)
				int offsetCopyCount = 0;
				int loopcount = 1;
				while (loopcount < locsOfCurrentIdx.Count && loopcount < QFSMAXITER) {
					int foundIdx = (int) locsOfCurrentIdx[locsOfCurrentIdx.Count - 1 - loopcount];
					if (idx - foundIdx >= MAXOFFSET) {
						break;
					}
					loopcount++;
					offsetCopyCount = 3;
					while ((dData.Length > idx + copyCount) && (dData[idx + copyCount] == dData[foundIdx + copyCount]) && (copyCount < MAXCOPYCOUNT)) {
						copyCount++;
					}
					if (copyCount > offsetCopyCount) {
						offsetCopyCount = copyCount;
						copyOffset = idx - foundIdx;
					}
				}


				//check if we can compress this
				if (offsetCopyCount > dData.Length - idx) {
					offsetCopyCount = idx - dData.Length;
				}
				if (offsetCopyCount <= 2) {
					offsetCopyCount = 0;
				} else if ((offsetCopyCount == 3) && (copyOffset > 0x400)) { //1024
					offsetCopyCount = 0;
				} else if ((offsetCopyCount == 4) && (copyOffset > 0x4000)) {//16384
					offsetCopyCount = 0;
				}

				//is this offset compressible? if so do the compression
				if (offsetCopyCount > 0) {
					//plain copy
					while (idx - lastReadIdx >= 4) {
						copyCount = (idx - lastReadIdx) / 4 - 1;
						if (copyCount > 0x1B) {
							copyCount = 0x1B;
						}
						cData[cPos++] = (byte) (0xE0 + copyCount);
						copyCount = 4 * copyCount + 4;

						LZCompliantCopy(ref dData, lastReadIdx, ref cData, cPos, copyCount);
						lastReadIdx += copyCount;
						cPos += copyCount;
					}

					//offset copy
					copyCount = idx - lastReadIdx;
					copyOffset--;
					//2 byte control character
					if ((offsetCopyCount <= 0x0A) && (copyOffset < 0x400)) {
						cData[cPos++] = (byte) (((copyOffset >> 8) << 5) + ((offsetCopyCount - 3) << 2) + copyCount);
						cData[cPos++] = (byte) copyOffset;
						//3 byte control character
					} else if ((offsetCopyCount <= 0x43) && (copyOffset < 0x4000)) {
						cData[cPos++] = (byte) (0x80 + (offsetCopyCount - 4));
						cData[cPos++] = (byte) ((copyCount << 6) + (copyOffset >> 8));
						cData[cPos++] = (byte) copyOffset;
						//4 byte control character
					} else if ((offsetCopyCount <= MAXCOPYCOUNT) && (copyOffset < MAXOFFSET)) {
						cData[cPos++] = (byte) (0xc0 + ((copyOffset >> 16) << 4) + (((offsetCopyCount - 5) >> 8) << 2) + copyCount);
						cData[cPos++] = (byte) ((copyOffset >> 8) & 0xff);
						cData[cPos++] = (byte) (copyOffset & 0xff);
						cData[cPos++] = (byte) ((offsetCopyCount - 5) & 0xff);
					}

					//do the offset copy
					LZCompliantCopy(ref dData, lastReadIdx, ref cData, cPos, copyCount);
					cPos += copyCount;
					lastReadIdx += copyCount;
					lastReadIdx += offsetCopyCount;
				}
			}

			//Add end record
			idx = dData.Length;
			while (idx - lastReadIdx >= 4) {
				copyCount = (idx - lastReadIdx) / 4 - 1;
				if (copyCount > 0x1B) {
					copyCount = 0x1B;
				}
				cData[cPos++] = (byte) (0xE0 + copyCount);
				copyCount = 4 * copyCount + 4;

				LZCompliantCopy(ref dData, lastReadIdx, ref cData, cPos, copyCount);
				lastReadIdx += copyCount;
				cPos += copyCount;
			}

			copyCount = idx - lastReadIdx;
			cData[cPos++] = (byte) (0xFC + copyCount);
			LZCompliantCopy(ref dData, lastReadIdx, ref cData, cPos, copyCount);
			cPos += copyCount;
			lastReadIdx += copyCount;

			//write header for the compressed data
			//set compressed size
			//...

			throw new NotImplementedException();
		}


		/// <summary>
		/// Recursive method for LZ compliant copying of data between arrays.
		/// </summary>
		/// <param name="source">Array to copy from</param>
		/// <param name="sourceOffset">Location in array to start copying from</param>
		/// <param name="destination">Array to copy to</param>
		/// <param name="destinationOffset">Position in array to start copying to</param>
		/// <param name="length">Length of data to copy</param>
		/// <remarks>
		/// With QFS (LZ77), a LZ compatible array copy method is required to copy data one byte at a time between arrays. Within the LZ compatible algorithms, it is legal to copy data to the destination array that would overrun the length of the destination array. The solution is to copy one byte at a time.
		/// </remarks>
		private static void LZCompliantCopy(ref byte[] source, int sourceOffset, ref byte[] destination, int destinationOffset, int length) {
			if (length > 0) {
				Array.Copy(source, sourceOffset, destination, destinationOffset, length);

				length -= 1;
				sourceOffset++;
				destinationOffset++;
				LZCompliantCopy(ref source, sourceOffset, ref destination, destinationOffset, length);
			}
		}
	}
}
