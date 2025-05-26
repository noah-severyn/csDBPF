using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace csDBPF {
	/// <summary>
	/// Collection of miscellaneous utility methods to use with DBPFFiles.
	/// </summary>
	public static class DBPFUtil {
		private static readonly string[] sc4Extensions = { ".dat", ".sc4lot", ".sc4desc", ".sc4model" };
		private static readonly byte[] DBPF = { 0x44, 0x42, 0x50, 0x46 };

        //TODO - should redo this function as an extension method? Split to Quick and Full versions
        /// <summary>
        /// Filters a list of file paths based on SC4 file extensions.
        /// </summary>
        /// <param name="filesToFilter">List of all files to filter through</param>
        /// <param name="validateIdentifier">Optionally examine the first 4 bytes of the specified file to determine if valid DBPF format. If omitted or set to false, only the file extension will be examined.</param>
        /// <returns>A listing of DBPF files</returns>
        public static List<string> FilterDBPFFiles(IEnumerable<string> filesToFilter, bool validateIdentifier = false) {
			List<string> dbpfFiles = [];
			foreach (string file in filesToFilter) {
                if (IsValidDBPF(file, validateIdentifier)) {
                    dbpfFiles.Add(file);
                }
			}
			return dbpfFiles;
		}



        /// <summary>
        /// Examines the first bytes of the file to determine if the file is valid DBPF or not.
        /// </summary>
        /// <param name="filePath">Full File path of the file to examine</param>
        /// <param name="validateIdentifier">Optionally examine the first 4 bytes of the specified file to determine if valid DBPF format. If omitted or set to false, only the file extension will be examined.</param>
        /// <returns>true if valid SC4 DBPF file, false otherwisee</returns>
        public static bool IsValidDBPF(string filePath, bool validateIdentifier = false) {
            FileStream fs = new FileStream(filePath, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);

            if (validateIdentifier) {
                byte[] firstFour = br.ReadBytes(4);
                br.Close();
                fs.Close();
                return firstFour.SequenceEqual(DBPF);
            } else {
                return Array.IndexOf(sc4Extensions, Path.GetExtension(filePath).ToLower()) > -1;
            }
        }
        /// <summary>
        /// Examines the first bytes of the file to determine if the file is valid DBPF or not.
        /// </summary>
        /// <param name="file">File to examine</param>
        /// <param name="validateIdentifier">Optionally examine the first 4 bytes of the specified file to determine if valid DBPF format. If omitted or set to false, only the file extension will be examined.</param>
        /// <returns>TRUE if valid SC4 DBPF file, FALSE otherwise</returns>
        public static bool IsValidDBPF(FileInfo file, bool validateIdentifier = false) {
            return IsValidDBPF(file.FullName, validateIdentifier);
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
        /// <param name="uppercase">Specify output as uppercase. Default is lowercase.</param>
        /// <param name="prefix">Specify to omit "0x" prefixed to the front of the string. Default is true to include.</param>
        /// <returns>Uppercase string representing the value</returns>
        /// <exception cref="ArgumentOutOfRangeException">Number of places must be between 0 and 8.</exception>
        public static string ToHexString(long value, int places = 8, bool uppercase = false, bool prefix = true) {
            if (places < 0 || places > 16) {
                throw new ArgumentOutOfRangeException(nameof(places), "Number of places must be between 0 and 8.");
            }
            string prepend = (prefix == true ? "0x" : string.Empty);
            if (uppercase) {
                return prepend + (value).ToString($"X{places}");
            } else {
                return prepend + (value).ToString($"x{places}");
            }
        }
        /// <summary>
        /// Returns the uppercase string representation of the provided uint converted to hex, padded by the specified number of places.
        /// </summary>
        /// <param name="value">Value to return</param>
        /// <param name="places">Number of places to pad the value. 0-8 valid; 8 is default</param>
        /// <param name="uppercase">Specify output as uppercase. Default is lowercase.</param>
        /// <param name="prefix">Specify to omit "0x" prefixed to the front of the string. Default is true to include.</param>
        /// <returns>Uppercase string representing the value</returns>
        /// <exception cref="ArgumentOutOfRangeException">Number of places must be between 0 and 8.</exception>
        public static string ToHexString(uint? value, int places = 8, bool uppercase = false, bool prefix = true) {
            if (places < 0 || places > 16) {
                throw new ArgumentOutOfRangeException(nameof(places), "Number of places must be between 0 and 8.");
            }
			if (value != null) {
                string prepend = (prefix == true ? "0x" : string.Empty);
                if (uppercase) {
                    return prepend + ((uint) value).ToString($"X{places}");
                } else {
                    return prepend + ((uint) value).ToString($"x{places}");
                }
				
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

        /// <summary>
        /// Generate a random uint value.
        /// </summary>
        public static uint GenerateRandomUint() {
            //https://stackoverflow.com/a/18332307/10802255
            Random rand = new Random();
            return (uint) (rand.Next(1 << 30)) << 2 | (uint) (rand.Next(1 << 2));
        }
    }
}