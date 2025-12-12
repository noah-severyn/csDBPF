using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static csDBPF.DBPF;

namespace csDBPF {
	/// <summary>
	/// Helper methods to parse a byte array into an array of one of the DBPF data types. 
	/// </summary>
	public static class ByteArrayHelper {
		//TODO - replace all of these as MemoryMarshall.Case<To,From> https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.memorymarshal.cast?view=net-7.0
		//TODO - replace some of these with Encoding.ASCII.GetString() ???

		//Convert from a byte[] to the specific data type
		#region FromByteArrayToArray
		/// <summary>
		/// Convert byte array to boolean array.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <returns>Array of boolean values</returns>
		public static bool[] ToBoolArray(byte[] data) {
			bool[] result = new bool[data.Length];
			for (int idx = 0; idx < data.Length; idx++) {
				if (data[idx] == 0) {
					result[idx] = false;
				} else {
					result[idx] = true;
				}
			}
			return result;
		}
		/// <summary>
		/// Convert byte array to UInt8 array. A Uint8 is the same as a byte, so just return the byte array.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <returns>Array of byte values</returns>
		public static byte[] ToUint8Array(byte[] data) {
			return data;
		}
		/// <summary>
		/// Convert byte array to UInt16 array.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <returns>Array of ushort values</returns>
		public static ushort[] ToUInt16Array(byte[] data) {
			if (data.Length % 2 != 0) {
				throw new ArgumentException("Length of data array cannot be odd!");
			}

			ushort[] result = new ushort[data.Length / 2];
			for (int idx = 0; idx < data.Length / 2; idx++) {
				//result[idx] = (ushort) (data[pos+1] << 8 | data[pos]);
				result[idx] = BitConverter.ToUInt16(data, idx * 2);
			}
			return result;
		}
		/// <summary>
		/// Convert byte array to UInt32 array.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <returns>Array of uint values</returns>
		public static uint[] ToUInt32Array(byte[] data) {
			if (data.Length % 2 != 0) {
				throw new ArgumentException("Length of data array cannot be odd!");
			} else if (data.Length % 4 != 0) {
				throw new ArgumentException("Length of data array must be a multiple of 4!");
			}

			uint[] result = new uint[data.Length / 4];
			for (int idx = 0; idx < data.Length / 4; idx++) {
				//result[idx] = (uint) ((data[pos+3] << 24) | (data[pos + 2] << 16) | (data[pos + 1] << 8) | data[pos]);
				result[idx] = BitConverter.ToUInt32(data, idx * 4);
			}
			return result;
		}
		/// <summary>
		/// Convert byte array to SInt32 array.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <returns>Array of int values</returns>
		public static int[] ToSInt32Array(byte[] data) {
			if (data.Length % 2 != 0) {
				throw new ArgumentException("Length of data array cannot be odd!");
			} else if (data.Length % 4 != 0) {
				throw new ArgumentException("Length of data array must be a multiple of 4!");
			}

			int[] result = new int[data.Length / 4];
			for (int idx = 0; idx < data.Length / 4; idx++) {
				//result[idx] = (data[pos+3] << 24) | (data[pos + 2] << 16) | (data[pos + 1] << 8) | data[pos];
				result[idx] = BitConverter.ToInt32(data, idx * 4);
			}
			return result;
		}
		/// <summary>
		/// Convert byte array to Float32 array.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <returns>Array of float values</returns>
		public static float[] ToFloat32Array(byte[] data) {
			if (data.Length % 2 != 0) {
				throw new ArgumentException("Length of data array cannot be odd!");
			} else if (data.Length % 4 != 0) {
				throw new ArgumentException("Length of data array must be a multiple of 4!");
			}

			float[] result = new float[data.Length / 4];
			for (int idx = 0; idx < data.Length / 4; idx++) {
				result[idx] = BitConverter.ToSingle(data, idx * 4); //float aka single
			}
			return result;
		}
		/// <summary>
		/// Convert byte array to Float32 List.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <returns>List of float values</returns>
		public static List<float> ToFloat32List(byte[] data) {
			if (data.Length % 2 != 0) {
				throw new ArgumentException("Length of data array cannot be odd!");
			} else if (data.Length % 4 != 0) {
				throw new ArgumentException("Length of data array must be a multiple of 4!");
			}

			List<float> result = new List<float>();
			for (int idx = 0; idx < data.Length / 4; idx++) {
				result.Add(BitConverter.ToSingle(data, idx * 4)); //float aka single
			}
			return result;
		}
		/// <summary>
		/// Convert byte array to SInt64 array.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <returns>Array of long values</returns>
		public static long[] ToSInt64Array(byte[] data) {
			if (data.Length % 2 != 0) {
				throw new ArgumentException("Length of data array cannot be odd!");
			} else if (data.Length % 8 != 0) {
				throw new ArgumentException("Length of data array must be a multiple of 8!");
			}

			long[] result = new long[data.Length / 8];
			for (int idx = 0; idx < data.Length / 8; idx++) {
				result[idx] = BitConverter.ToInt64(data, idx * 8);
			}
			return result;
		}
		/// <summary>
		/// Convert byte array to long List
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <returns>List of long values</returns>
		public static List<long> ToSInt64List(byte[] data) {
			if (data.Length % 2 != 0) {
				throw new ArgumentException("Length of data array cannot be odd!");
			} else if (data.Length % 8 != 0) {
				throw new ArgumentException("Length of data array must be a multiple of 8!");
			}

			List<long> result = new List<long>();
			for (int idx = 0; idx < data.Length / 8; idx++) {
				result.Add(BitConverter.ToInt64(data, idx * 8));
			}
			return result;
		}
        #endregion FromByteArrayToArray



        #region ByteArrayTo
        /// <summary>
        /// Converts the specified byte array to its string representation.
        /// </summary>
        /// <param name="data">The byte array to convert</param>
        /// <returns>A string representation of the byte array, or an empty string if <paramref name="data"/> is <see langword="null"/>.</returns>
        /// <remarks>Non-printable characters are replaced with a period.</remarks>
        public static string ToAString(this byte[] data) {
			if (data is null) return string.Empty;
			return ToAString(data, 0, data.Length);
		}
        /// <summary>
        /// Converts a range of bytes from the specified array to a string, starting at the given index.
        /// </summary>
        /// <param name="data">The byte array to convert</param>
        /// <param name="start">The zero-based index in <paramref name="data"/> at which to begin conversion.</param>
        /// <returns>A string representation of the bytes in <paramref name="data"/> starting at <paramref name="start"/> and continuing to the end of the array, or an empty string if <paramref name="data"/> is <see langword="null"/>.</returns>
        /// <remarks>Non-printable characters are replaced with a period.</remarks>
        public static string ToAString(this byte[] data, int start) {
			if (data is null) return string.Empty;
			return ToAString(data, start, data.Length - start);
		}
        /// <summary>
        /// Converts a range of bytes from the specified array into a string, replacing non-printable ASCII characters with a
        /// period ('.').
        /// </summary>
        /// <param name="data">The byte array containing the data to convert.</param>
        /// <param name="start">The zero-based index in <paramref name="data"/> at which to begin conversion.</param>
        /// <param name="length">The number of bytes to convert starting from <paramref name="start"/>.</param>
        /// <returns>A string representation of the bytes in <paramref name="data"/> starting at <paramref name="start"/> and continuing for <paramref name="length"/> positions, or an empty string if <paramref name="data"/> is <see langword="null"/>.</returns>
        /// <remarks>Non-printable characters are replaced with a period.</remarks>
        public static string ToAString(this byte[] data, int start, int length) {
			if (data is null) return string.Empty;
            string s = System.Text.Encoding.ASCII.GetString(data, start, length);
            char[] buf = s.ToCharArray();
            for (int i = 0; i < buf.Length; i++) {
                char c = buf[i];
                if (c < 31 || c == 127) {
                    buf[i] = '.';
                }
            }
            return new string(buf);
        }



        /// <summary>
        /// Reads two bytes from the specified array starting at the given offset and interprets them as an unsigned 16-bit integer in big-endian order.
        /// </summary>
        /// <remarks>Using the sample array <c>[0x30, 0x30, 0x30, 0x30]</c>, using <see cref="DBPF.Encoding.Binary"/> yields <c>0x30303030</c> since each byte is taken as its literal value, while using <see cref="DBPF.Encoding.Text"/> yields <c>0x0000</c> since each byte is taken as its ASCII text equivalent.</remarks>
        /// <param name="data">The byte array containing the data to read. Must contain at least two bytes starting from <paramref name="offset"/>.</param>
        /// <param name="offset">The zero-based index in <paramref name="data"/> at which to begin reading. Defaults to 0.</param>
        /// <param name="encoding">The encoding type of <paramref name="data"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is <see langword="null"/>.</exception>
        /// <returns>The unsigned 16-bit integer value represented by the two bytes at the specified offset, interpreted in big-endian format.</returns>
        public static ushort ReadIntoUshort(this byte[] data, int offset, DBPF.Encoding encoding) {
            ArgumentNullException.ThrowIfNull(data);

            if (encoding == DBPF.Encoding.Binary) {
                return BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset));
            } else {
                _ = ushort.TryParse(ToAString(data, offset, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort result);
                return result;
            }
        }
        /// <summary>
        /// Reads four bytes from the specified array starting at the given offset and interprets them as a signed 32-bit integer in big-endian order.
        /// </summary>
		/// <remarks>Using the sample array <c>[0x30, 0x30, 0x30, 0x30]</c>, using <see cref="DBPF.Encoding.Binary"/> yields <c>0x30303030</c> since each byte is taken as its literal value, while using <see cref="DBPF.Encoding.Text"/> yields <c>0x0000</c> since each byte is taken as its ASCII text equivalent.</remarks>
        /// <param name="data">The byte array containing the data to read. Must contain at least four bytes starting from <paramref name="offset"/>.</param>
        /// <param name="offset">The zero-based index in <paramref name="data"/> at which to begin reading.</param>
		/// <param name="encoding">The encoding type of <paramref name="data"/>.</param>
		/// <param name="length">The number of bytes to convert starting from <paramref name="offset"/>. The default is 8.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is <see langword="null"/>.</exception>
        /// <returns>The 32-bit signed integer represented by the four bytes at the specified offset in big-endian format.</returns>
        public static int ReadIntoInt(this byte[] data, int offset, DBPF.Encoding encoding, int length = 8) {
            ArgumentNullException.ThrowIfNull(data);

            if (encoding == DBPF.Encoding.Binary) {
                return BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(offset));
            } else {
                _ = int.TryParse(ToAString(data, offset, length), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result);
                return result;
            }
        }
        /// <summary>
        /// Reads four bytes from the specified array starting at the given offset and interprets them as an unsigned 32-bit integer in big-endian order.
        /// </summary>
		/// <remarks>Using the sample array <c>[0x30, 0x30, 0x30, 0x30]</c>, using <see cref="DBPF.Encoding.Binary"/> yields <c>0x30303030</c> since each byte is taken as its literal value, while using <see cref="DBPF.Encoding.Text"/> yields <c>0x0000</c> since each byte is taken as its ASCII text equivalent.</remarks>
        /// <param name="data">The byte array containing the data to read. Must contain at least four bytes starting from <paramref name="offset"/>.</param>
        /// <param name="offset">The zero-based index in <paramref name="data"/> at which to begin reading.</param>
		/// <param name="encoding">The encoding type of <paramref name="data"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is <see langword="null"/>.</exception>
        /// <returns>The 32-bit unsigned integer represented by the four bytes at the specified offset in big-endian format.</returns>
        public static uint ReadIntoUint(this byte[] data, int offset, DBPF.Encoding encoding) {
			ArgumentNullException.ThrowIfNull(data);
			
			if (encoding == DBPF.Encoding.Binary) {
                //The BitConverter functions are dependent on the the endianness of the system, but dbpf files are always big endian
                return BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset));
            } else {
                _ = uint.TryParse(ToAString(data, offset, 8), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result);
                return result;
            }
		}
        /// <summary>
        /// Reads four bytes from the specified array starting at the given offset and interprets them as a signed 64-bit integer in big-endian order.
        /// </summary>
		/// <remarks>Using the sample array <c>[0x30, 0x30, 0x30, 0x30]</c>, using <see cref="DBPF.Encoding.Binary"/> yields <c>0x30303030</c> since each byte is taken as its literal value, while using <see cref="DBPF.Encoding.Text"/> yields <c>0x0000</c> since each byte is taken as its ASCII text equivalent.<br></br><br></br>
        /// 
        /// The <paramref name="length"/> should typically be specified when reading values into a <see cref="DBPFPropertyLong"/>, as the number of chars of the underlying property data type (2/4/8/16 chars) should be read, instead of always the 16 chars defined by a long.</remarks>
        /// <param name="data">The byte array containing the data to read. Must contain at least four bytes starting from <paramref name="offset"/>.</param>
        /// <param name="offset">The zero-based index in <paramref name="data"/> at which to begin reading.</param>
		/// <param name="encoding">The encoding type of <paramref name="data"/>.</param>
		/// <param name="length">The number of bytes to convert starting from <paramref name="offset"/>. Default is 16.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is <see langword="null"/>.</exception>
        /// <returns>The signed 64-bit integer value represented by the eight bytes at the specified offset, interpreted in big-endian format.</returns>
        public static long ReadIntoLong(this byte[] data, int offset, DBPF.Encoding encoding, int length = 16) {
            ArgumentNullException.ThrowIfNull(data);
            
            if (encoding == DBPF.Encoding.Binary) {
                return BinaryPrimitives.ReadInt64BigEndian(data.AsSpan(offset));
            } else {
                _ = long.TryParse(ToAString(data, offset, length), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long result);
                return result;
            }
        }
        /// <summary>
        /// Reads four bytes from the specified array starting at the given offset and interprets them as a single-precision floating-point in big-endian order.
        /// </summary>
        /// <param name="data">The byte array containing the data to read. Must contain at least four bytes starting from <paramref name="offset"/>.</param>
        /// <param name="offset">The zero-based index in <paramref name="data"/> at which to begin reading. Defaults to 0.</param>
		/// <param name="length">The number of bytes to convert starting from <paramref name="offset"/>.</param>
		/// <param name="encoding">The encoding type of <paramref name="data"/>.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is <see langword="null"/>.</exception>
        /// <returns>The single-precision floating-point value read from the specified offset in the byte array.</returns>
        public static float ReadIntoFloat(this byte[] data, int offset, int length, DBPF.Encoding encoding) {
            ArgumentNullException.ThrowIfNull(data);
            
            if (encoding == DBPF.Encoding.Binary) {
                return BinaryPrimitives.ReadSingleBigEndian(data.AsSpan(offset, length));
            } else {
                _ = float.TryParse(ToAString(data, offset, length), out float result);
                return result;
            }
        }
        #endregion FromByteArrayToA



        #region ToBytes
        /// <summary>
        /// Converts the specified string to an array of bytes, using either single-byte or Unicode encoding.
        /// </summary>
        /// <remarks>When <paramref name="singleByteEncoding"/> is <see langword="true"/>, only characters that can be represented as single bytes are supported (ANSI/Windows-1252 encoding). For strings containing multi-byte characters (e.g., Korean, Chinese,  or other non-ASCII characters), or for single byte characters represented as two bytes (<c>T</c>> -> <c>0x54, 0x00</c>), use Unicode encoding with <paramref name="singleByteEncoding"/> set as <see langword="false"/>.</remarks>
        /// <param name="data">The string to convert. If <paramref name="data"/> is <see langword="null"/>, an empty byte array is returned.</param>
        /// <param name="singleByteEncoding">Specify whether the data is single-byte encoded or Unicode encoded.</param>
        /// <returns>A byte array representing the encoded string, or an empty array if <paramref name="data"/> is <see langword="null"/>.</returns>
        public static byte[] ToBytes(this string data, bool singleByteEncoding = false) {
			if (data is null) {
				return [];
			}

			if (singleByteEncoding) {
                byte[] bytes = new byte[data.Length];
                for (int i = 0; i < data.Length; i++) {
                    bytes[i] = Convert.ToByte(data[i]);
                }
                return bytes;
            }
			return System.Text.Encoding.Unicode.GetBytes(data);
		}
		/// <summary>
		/// Converts a long to byte array with the given length.
		/// </summary>
		/// <param name="value">Value to convert</param>
		/// <param name="numPlaces">Length of returned array</param>
		/// <returns>A byte array representing the input value <paramref name="value"/></returns>
		public static byte[] ToBytes(this long value, int numPlaces = 8) {
			byte[] bytes = BitConverter.GetBytes(value);
            if (numPlaces >= bytes.Length) {
                return bytes;
            }
            return bytes[0..numPlaces];
        }
        /// <summary>
        /// Parses an array of booleans into bytes.
        /// </summary>
        /// <param name="data">Data to parse</param>
        /// <returns>The array of data as bytes, or an empty array if <paramref name="data"/> is <see langword="null"/></returns>
        public static byte[] ToBytes(this bool[] data) {
            if (data is null) return [];
            byte[] result = new byte[data.Length];
			for (int idx = 0; idx < result.Length; idx++) {
                result[idx] = data[idx] ? (byte) 0x01 : (byte) 0x00;
            }
			return result;
		}
        /// <summary>
        /// Parses an array of chars (UInt8) into bytes.
        /// </summary>
        /// <param name="data">Data to parse</param>
        /// <returns>The array of data as bytes, or an empty array if <paramref name="data"/> is <see langword="null"/></returns>
        public static byte[] ToBytes(this char[] data) {
            if (data is null) return [];
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++) {
                result[i] = Convert.ToByte(data[i]);
            }
            return result;
        }
        /// <summary>
        /// Parses an array of ushorts (UInt16) into bytes.
        /// </summary>
        /// <param name="data">Data to parse</param>
        /// <returns>The array of data as bytes, or an empty array if <paramref name="data"/> is <see langword="null"/></returns>
        public static byte[] ToBytes(this ushort[] data) {
            if (data is null) return [];
            return MemoryMarshal.AsBytes<ushort>(data.AsSpan()).ToArray();
        }
        /// <summary>
        /// Parses an array of ints (Sint32) into bytes.
        /// </summary>
        /// <param name="data">Data to parse</param>
        /// <returns>The array of data as bytes, or an empty array if <paramref name="data"/> is <see langword="null"/></returns>
        public static byte[] ToBytes(this int[] data) {
            if (data is null) return [];
            return MemoryMarshal.AsBytes<int>(data.AsSpan()).ToArray();
        }
        /// <summary>
        /// Parses an array of uints (UInt32) into bytes.
        /// </summary>
        /// <param name="data">Data to parse</param>
        /// <returns>The array of data as bytes, or an empty array if <paramref name="data"/> is <see langword="null"/></returns>
        public static byte[] ToBytes(this uint[] data) {
            if (data is null) return [];
            return MemoryMarshal.AsBytes<uint>(data.AsSpan()).ToArray();
        }
        /// <summary>
        /// Parses an array of floats (Float32) into bytes.
        /// </summary>
        /// <param name="data">Data to parse</param>
        /// <returns>The array of data as bytes, or an empty array if <paramref name="data"/> is <see langword="null"/></returns>
        public static byte[] ToBytes(this float[] data) {
            if (data is null) return [];
            return MemoryMarshal.AsBytes<float>(data.AsSpan()).ToArray();
        }
        /// <summary>
        /// Parses an array of longs (SInt64) into bytes.
        /// </summary>
        /// <param name="data">Data to parse</param>
        /// <returns>The array of data as bytes, or an empty array if <paramref name="data"/> is <see langword="null"/></returns>
        public static byte[] ToBytes(this long[] data) {
            if (data is null) return [];
            return MemoryMarshal.AsBytes<long>(data.AsSpan()).ToArray();
        }
		#endregion ToBytes
	}
}
