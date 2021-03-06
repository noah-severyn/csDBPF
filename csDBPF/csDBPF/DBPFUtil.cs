using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;

namespace csDBPF {
	public static class DBPFUtil {
		private static readonly string[] sc4Extensions = { "dat", "sc4lot", "sc4desc", "sc4model" };



		/// <summary>
		/// Filters a list of file paths based on SC4 file extensions.
		/// </summary>
		/// <param name="filesToFilter">List of all files to filter through</param>
		/// <returns>Tuple of List <string> (sc4Files,skippedFiles)</returns>
		public static (List<string>, List<string>) SortFilesByExtension(List<string> filesToFilter) {
			List<string> sc4Files = new List<string>();
			List<string> skippedFiles = new List<string>();

			string extension;
			foreach (string file in filesToFilter) {
				extension = file.Substring(file.LastIndexOf(".") + 1);
				if (sc4Extensions.Any(extension.Contains) && IsFileDBPF(file)) { //https://stackoverflow.com/a/2912483/10802255
					sc4Files.Add(file);
				} else {
					skippedFiles.Add(file);
				}
			}

			return (sc4Files, skippedFiles);
		}

		/// <summary>
		/// Examines the file <see cref="DBPFFile.DBPFHeader"/> to determine if the file is valid DBPF or not.
		/// </summary>
		/// <param name="fileName">Full path of file</param>
		/// <returns>TRUE if valid SC4 DBPF file, FALSE otherwise</returns>
		public static bool IsFileDBPF(string fileName) {
			try {
				//In order to determine if the file is DBPF or not, all we need to look at is first few bytes which make up the header - no need to examine any of the rest of the file, so we just create a Header here instead of a DBPFFile.
				DBPFFile.DBPFHeader header = new DBPFFile.DBPFHeader(fileName);
				return true;
			}
			catch (Exception) {
				return false;
			}
		}



		#region ReverseBytes
		/// <summary>
		/// Reverses the byte order for a ushort. Example: 3 (0x0003) returns 768 (0x0300)
		/// </summary>
		/// <remarks>
		/// See:https://www.csharp-examples.net/reverse-bytes/
		/// </remarks>
		/// <param name="value">Value to reverse</param>
		/// <returns>Reversed ushort</returns>
		public static ushort ReverseBytes(ushort value) {
			return (ushort) ((value & 0x00FFU) << 8 | (value & 0xFF00U) >> 8);
		}

		/// <summary>
		/// Reverses the byte order for a uint. See <see cref="ReverseBytes(uint)"/>.
		/// </summary>
		/// <param name="value">Value to reverse</param>
		/// <returns>Reversed uint</returns>
		public static uint ReverseBytes(uint value) {
			return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 | (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
		}

		/// <summary>
		/// Reverses the byte order for a ulong. See <see cref="ReverseBytes(uint)"/>.
		/// </summary>
		/// <param name="value">Value to reverse</param>
		/// <returns>Reversed uint</returns>
		public static long ReverseBytes(long value) {
			return (value & 0x00000000000000FFL) << 56 | (value & 0x000000000000FF00L) << 40 | (value & 0x0000000000FF0000L) << 24 | (value & 0x00000000FF000000L) << 8 |
		 (value & 0x000000FF00000000L) >> 8 | (value & 0x0000FF0000000000L) >> 24 | (value & 0x00FF000000000000L) >> 40 | (value & 0x7F00000000000000L) >> 56;
		}
		#endregion


		/// <summary>
		/// Returns the uppercase string representation of the provided uint converted to hex, padded by the specified number of places.
		/// </summary>
		/// <param name="value">Value to return</param>
		/// <param name="places">Number of places to pad the value. 0-8 valid; 8 is default</param>
		/// <returns>Uppercase string representing the uint</returns>
		public static string UIntToHexString(uint? value, int places = 8) {
			if (places < 0 || places > 8) {
				throw new ArgumentOutOfRangeException("places", "Number of places must be between 0 and 8.");
			}
			if (value != null) {
				return ((uint) value).ToString($"X{places}");
			} else {
				return value.ToString();
			}
		}


		/// <summary>
		/// Convert Unix datetime to a <see cref="DateTime"/> object.
		/// </summary>
		/// <param name="time">Unix time</param>
		/// <returns><see cref="DateTime"/> object equal to the provided Unix time</returns>
		public static DateTime UnixToDate(uint time) {
			return DateTimeOffset.FromUnixTimeSeconds(time).UtcDateTime;
		}


		/// <summary>
		/// Appends an array of byte values sequentially into a string.
		/// </summary>
		/// <param name="data">Byte data to print.</param>
		/// <returns>String of 2 character bytes, space separated</returns>
		public static string PrintByteValues(byte[] data) {
			StringBuilder sb = new StringBuilder();
			foreach (byte b in data) {
				sb.Append(b.ToString("X2") + " ");
			}
			return sb.ToString();
		}
	}
}