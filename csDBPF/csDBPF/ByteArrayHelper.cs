using System;
using System.Collections.Generic;
using System.Text;

namespace csDBPF {
	/// <summary>
	/// Helper methods to parse a byte array into an array of one of the DBPF data types. 
	/// </summary>
	public static class ByteArrayHelper {

		#region FromByteArrayTo
		/// <summary>
		/// Convert byte array to bool array.
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
		/// Convert byte array to UInt8 array.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <returns>Array of char values</returns>
		public static char[] ToUint8Array(byte[] data) {
			char[] result = new char[data.Length];
			for (int idx = 0; idx < data.Length; idx++) {
				result[idx] = (char) data[idx];
			}
			return result;
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
			int pos = 0;
			for (int idx = 0; idx < data.Length / 2; idx++) {
				result[idx] = (ushort) (data[pos] << 8 | data[pos + 1]);
				pos += 2;
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
			int pos = 0;
			for (int idx = 0; idx < data.Length / 4; idx++) {
				result[idx] = (uint) ((data[pos] << 24) | (data[pos + 1] << 16) | (data[pos + 2] << 8) | data[pos + 3]);
				pos += 4;
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
			int pos = 0;
			for (int idx = 0; idx < data.Length / 4; idx++) {
				result[idx] = (data[pos] << 24) | (data[pos + 1] << 16) | (data[pos + 2] << 8) | data[pos + 3];
				pos += 4;
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
			int pos = 0;
			for (int idx = 0; idx < data.Length / 4; idx++) {
				result[idx] = (data[pos] << 24) | (data[pos + 1] << 16) | (data[pos + 2] << 8) | data[pos + 3];
				pos += 4;
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
			int pos = 0;
			for (int idx = 0; idx < data.Length / 8; idx++) {
				result[idx] = DBPFUtil.ReverseBytes(BitConverter.ToInt64(data, pos));
				pos += 8;
			}
			return result;
		}

		/// <summary>
		/// Reads a byte array and returns a string of the entire array.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <returns>A string of parsed data</returns>
		public static string ToAString(byte[] data) {
			return ToAString(data, 0, data.Length);
		}

		/// <summary>
		/// Reads a byte array and returns a string from the specified location to the end of the array.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <param name="start">Location to start parsing at</param>
		/// <returns>A string of parsed data</returns>
		public static string ToAString(byte[] data, int start) {
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
		#endregion FromByteArrayTo

		#region ToByteArrayFrom

		#endregion ToByteArrayFrom
		/// <summary>
		/// Reads a string and parses into a byte array the same length as the string
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <returns>A byte array of parsed data</returns>
		/// <remarks>
		/// String encoding is single byte ANSI (Windows-1252)
		/// </remarks>
		public static byte[] StringToByteArray(string data) {
			char[] chars = data.ToCharArray();
			byte[] result = new byte[data.Length];
			for (int idx = 0; idx < data.Length; idx++) {
				result[idx] = Convert.ToByte(chars[idx]);
			}
			return result;
		}
	}
}
