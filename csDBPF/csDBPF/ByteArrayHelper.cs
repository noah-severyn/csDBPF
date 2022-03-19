using System;
using System.Collections.Generic;
using System.Text;

namespace csDBPF {
	public static class ByteArrayHelper {

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

		public static char[] ToUint8Array(byte[] data) {
			char[] result = new char[data.Length];
			for (int idx = 0; idx < data.Length; idx++) {
				result[idx] = (char) data[idx];
			}
			return result;
		}

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

		public static long[] ToSInt64Array(byte[] data) {
			if (data.Length % 2 != 0) {
				throw new ArgumentException("Length of data array cannot be odd!");
			} else if (data.Length % 8 != 0) {
				throw new ArgumentException("Length of data array must be a multiple of 8!");
			}
			long[] result = new long[data.Length / 8];
			int pos = 0;
			for (int idx = 0; idx < data.Length / 8; idx++) {
				result[idx] = (data[pos] << 56) | (data[pos+1] << 48) | (data[pos+2] << 40) | (data[pos+3] << 32) | (data[pos+4] << 24) | (data[pos + 5] << 16) | (data[pos + 6] << 8) | data[pos + 7];
				pos += 8;
			}
			return result;
		}
	}
}
