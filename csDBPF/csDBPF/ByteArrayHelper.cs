using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace csDBPF {
	/// <summary>
	/// Helper methods to parse a byte array into an array of one of the DBPF data types. 
	/// </summary>
	public static class ByteArrayHelper {
		//TODO - replace all of these as MemoryMarshall.Case<To,From> https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.memorymarshal.cast?view=net-7.0

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

		#region FromByteArrayToA
		/// <summary>
		/// Reads a byte array and returns a string of the entire array.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <returns>A string of parsed data</returns>
		public static string ToAString(byte[] data) {
			if (data is null) return null;

			return ToAString(data, 0, data.Length);
		}
		/// <summary>
		/// Reads a byte array and returns a string from the specified location to the end of the array.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <param name="start">Location to start parsing at</param>
		/// <returns>A string of parsed data</returns>
		public static string ToAString(byte[] data, int start) {
			if (data is null) return null;

			return ToAString(data, start, data.Length - start);
		}
		/// <summary>
		/// Reads a byte array and returns a string from the specified location for a determined length.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <param name="start">Location to start parsing at</param>
		/// <param name="length">Length of the provided data to parse</param>
		/// <returns>A string of parsed data</returns>
		/// <remarks>
		/// Any non-printable characters are replaced with a period ('.').
		/// </remarks>
		public static string ToAString(byte[] data, int start, int length) {
			if (data is null) return null;

			StringBuilder sb = new StringBuilder();
			for (int idx = start; idx < start + length; idx++) {
				//Check to avoid problematic non-printable characters
				if (data[idx] < 31 || data[idx] == 127) {
					sb.Append('.');
				} else {
					sb.Append((char) data[idx]);
				}
			}
			return sb.ToString();
		}

		/// <summary>
		/// Sequentially reads 4 bytes and assigns them to a uint in big-endian order.
		/// </summary>
		/// <param name="data">Array to read from</param>
		/// <param name="offset">Location in array to start at. Default is 0</param>
		/// <returns>Uint value</returns>
		public static uint ReadBytesIntoUint(byte[] data, int offset = 0) {
			return (uint) ((data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3]);
		}

		/// <summary>
		/// Sequentially reads 2 bytes from the specified position and assigns them to a uint in big-endian order.
		/// </summary>
		/// <param name="data">Array to read from</param>
		/// <param name="offset">Location in array to start at. Default is 0</param>
		/// <returns>Ushort value</returns>
		public static ushort ReadBytesIntoUshort(byte[] data, int offset = 0) {
			return (ushort) ((data[offset] << 8) | data[offset + 1]);
		}


		/// <summary>
		/// Sequentially reads 1 byte, converts it to string equivalent, then parses back into a byte.
		/// </summary>
		/// <param name="data">Array to read from</param>
		/// <param name="offset">Location in array to start at. Default is 0</param>
		/// <returns>Byte value</returns>
		public static byte ReadTextIntoByte(byte[] data, int offset = 0) {
			byte.TryParse(ToAString(data, offset, 8), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte result);
			return result;
		}

		/// <summary>
		/// Sequentially reads 4 bytes, converts them to string equivalents, then parses them into a uint.
		/// </summary>
		/// <param name="data">Array to read from</param>
		/// <param name="offset">Location in array to start at. Default is 0</param>
		/// <returns>Uint value</returns>
		public static uint ReadTextToUint(byte[] data, int offset = 0) {
			uint.TryParse(ToAString(data, offset, 8), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result);
			return result;
		}

		/// <summary>
		/// Sequentially reads a number of bytes, converts them to string equivalents, then parses them into an int.
		/// </summary>
		/// <param name="data">Array to read from</param>
		/// <param name="offset">Location in array to start at. Default is 0</param>
		/// <param name="length">Length to read</param>
		/// <returns></returns>
		public static int ReadTextToInt(byte[] data, int offset, int length) {
			int.TryParse(ToAString(data, offset, length), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result);
			return result;
		}

		/// <summary>
		/// Sequentially reads a number of bytes, converts them to string equivalents, then parses them into a long.
		/// </summary>
		/// <param name="data">Array to read from</param>
		/// <param name="offset">Location in array to start at. Default is 0</param>
		/// <param name="length">Length to read</param>
		/// <returns></returns>
		public static long ReadTextToLong(byte[] data, int offset, int length) {
			long.TryParse(ToAString(data, offset, length), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long result);
			return result;
		}

		/// <summary>
		/// Sequentially reads a number of bytes, converts them to string equivalents, then parses them into a float.
		/// </summary>
		/// <param name="data">Array to read from</param>
		/// <param name="offset">Location in array to start at. Default is 0</param>
		/// <param name="length">Length to read</param>
		/// <returns></returns>
		public static float ReadTextToFloat(byte[] data, int offset, int length) {
			float.TryParse(ToAString(data, offset, length), out float result);
			return result;
		}


		#endregion FromByteArrayToA


		//Convert from the specific data type to a byte[]
		#region ToByteArray
		/// <summary>
		/// Reads a string and parses into a byte array the same length as the string
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <param name="singleByte">Parse type. FALSE (Default) is two byte output per char: Unicode; TRUE is one byte per char: ANSI (Windows-1252) </param>
		/// <returns>A byte array of parsed data</returns>
		public static byte[] ToBytes(string data, bool singleByte = false) {
			List<byte> bytes = new List<byte>();
			foreach (char c in data) {
				if (singleByte) {
					bytes.Add(Convert.ToByte(c));
				} else {
					//Convert char to two byte int, then get the bytes of that int
					bytes.AddRange(BitConverter.GetBytes(Convert.ToInt16(c)));
				}
			}
			return bytes.ToArray();
		}
		/// <summary>
		/// Converts a long to byte array with the given length.
		/// </summary>
		/// <param name="value">Value to convert</param>
		/// <param name="numPlaces">Length of returned array</param>
		/// <returns></returns>
		public static byte[] ToBytes(long value, int numPlaces = 8) {
			byte[] bytes = BitConverter.GetBytes(value);
			return bytes[0..numPlaces];
		}
		/// <summary>
		/// Pareses the boolean array and returns the corresponding byte array.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <returns>A byte array of parsed data</returns>
		public static byte[] ToByteArray(bool[] data) {
			byte[] result = new byte[data.Length];
			for (int idx = 0; idx < result.Length; idx++) {
				if (data[idx] == true) {
					result[idx] = 0x01;
				} else {
					result[idx] = 0x00;
				}
			}
			return result;
		}
		/// <summary>
		/// Pareses the char (UInt8) array and returns the corresponding byte array.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <returns>A byte array of parsed data</returns>
		public static byte[] ToByteArray(char[] data) {
			byte[] result = new byte[data.Length];
			Array.Copy(data, result, data.Length);
			return result;
		}
		/// <summary>
		/// Pareses the ushort (UInt16) array and returns the corresponding byte array.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <returns>A byte array of parsed data</returns>
		public static byte[] ToByteArray(ushort[] data) {
			byte[] result = new byte[data.Length * 2];
			for (int pos = 0; pos < data.Length; pos++) {
				Array.Copy(BitConverter.GetBytes(data[pos]), 0, result, pos * 2, 2);
			}
			return result;
		}
		/// <summary>
		/// Pareses the int (Sint32) array and returns the corresponding byte array.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <returns>A byte array of parsed data</returns>
		public static byte[] ToByteArray(int[] data) {
			byte[] result = new byte[data.Length * 4];
			for (int pos = 0; pos < data.Length; pos++) {
				Array.Copy(BitConverter.GetBytes(data[pos]), 0, result, pos * 4, 4);
			}
			return result;
		}
		/// <summary>
		/// Pareses the uint (UInt32) array and returns the corresponding byte array.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <returns>A byte array of parsed data</returns>
		public static byte[] ToByteArray(uint[] data) {
			byte[] result = new byte[data.Length * 4];
			for (int pos = 0; pos < data.Length; pos++) {
				Array.Copy(BitConverter.GetBytes(data[pos]), 0, result, pos * 4, 4);
			}
			return result;
		}
		/// <summary>
		/// Pareses the float (Float32) array and returns the corresponding byte array.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <returns>A byte array of parsed data</returns>
		public static byte[] ToByteArray(float[] data) {
			byte[] result = new byte[data.Length * 4];
			Buffer.BlockCopy(data, 0, result, 0, result.Length);
			return result;
		}
		/// <summary>
		/// Pareses the long (SInt64) array and returns the corresponding byte array.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <returns>A byte array of parsed data</returns>
		public static byte[] ToByteArray(long[] data) {
			byte[] result = new byte[data.Length * 8];
			for (int pos = 0; pos < data.Length; pos++) {
				Array.Copy(BitConverter.GetBytes(data[pos]), 0, result, pos * 8, 8);
			}
			return result;
		}

		public static byte[] ToByteArray(Array data) {
			Type type = data.GetType().GetElementType();
			switch (type.Name) {
				case "Int32":
					return ToByteArray((int[]) data);
				case "Float32":
					return ToByteArray((float[]) data);
				case "UInt32":
					return ToByteArray((uint[]) data);
				case "Boolean":
					return ToByteArray((bool[]) data);
				case "Byte": //Uint8
					return (byte[]) data;
				case "Int64":
					return ToByteArray((long[]) data);
				case "UInt16":
					return ToByteArray((ushort[]) data);
				default:
					return null;
			}
		}


		#endregion ToByteArrayFrom


		/// <summary>
		/// Finds the first instance of a given byte starting at the specified offset.
		/// </summary>
		/// <param name="data">Array to search in</param>
		/// <param name="byteToFind">Byte value to find</param>
		/// <param name="offset">Location in array to start at</param>
		/// <returns>Index of next occurrence of the target byte; 0 if byte is not found</returns>
		public static int FindNextInstanceOf(byte[] data, byte byteToFind, int offset = 0) {
			for (int idx = offset; idx < data.Length; idx++) {
				if (data[idx] == byteToFind) {
					return idx;
				}
			}
			return 0;
		}
	}
}
