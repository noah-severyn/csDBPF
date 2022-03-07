using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace csDBPF {
	public static class DBPFUtil {
		private static readonly string[] sc4Extensions = { "dat", "sc4lot", "sc4desc", "sc4model" };



		/// <summary>
		/// Filters a list of file paths based on SC4 file extensions.
		/// </summary>
		/// <param name="filesToFilter">List of all files to filter through</param>
		/// <returns>Tuple of List <string> (sc4Files,skippedFiles)</returns>
		public static (List<string>, List<string>) FilterFilesByExtension(List<string> filesToFilter) {
			List<string> sc4Files = new List<string>();
			List<string> skippedFiles = new List<string>();

			string extension;
			foreach (string file in filesToFilter) {
				extension = file.Substring(file.LastIndexOf(".") + 1);
				if (sc4Extensions.Any(extension.Contains)) { //https://stackoverflow.com/a/2912483/10802255
					sc4Files.Add(file);
					Trace.WriteLine(file);
				} else {
					skippedFiles.Add(file);
				}
			}

			return (sc4Files, skippedFiles);
		}


		/// <summary>
		/// Reverses the byte order for a uint. Example: 1697917002 (0x 65 34 28 4A) returns 1244148837 (0x 4A 28 34 65)
		/// </summary>
		/// <remarks>
		/// See:https://stackoverflow.com/a/18145923/10802255
		/// </remarks>
		/// <param name="value">Integer value to reverse</param>
		/// <returns></returns>
		public static uint ReverseBytes(uint value) {
			return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 | (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
		}

		/// <summary>
		/// Returns a string representation of the provided uint converted to hex, padded by the specified number of places
		/// </summary>
		/// <param name="value">Value to return</param>
		/// <param name="places">Number of places to pad the value. Should usually be 8. 0-8 valid.</param>
		/// <returns></returns>
		public static string UIntToHexString(uint? value, int places) {
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
		/// Reads a byte array and returns a string of the entire array.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <returns>A string of parsed data</returns>
		public static string CharsFromByteArray(byte[] data) {
			return CharsFromByteArray(data, 0, data.Length);
		}

		/// <summary>
		/// Reads a byte array and returns a string from the specified location to the end of the array.
		/// </summary>
		/// <param name="data">Data to parse</param>
		/// <param name="start">Location to start parsing at</param>
		/// <returns>A string of parsed data</returns>
		public static string CharsFromByteArray(byte[] data, int start) {
			return CharsFromByteArray(data, start, data.Length - start);
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
		public static string CharsFromByteArray(byte[] data, int start, int length) {
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

	}
}