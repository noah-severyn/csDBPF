using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;

namespace csDBPF {
	/// <summary>
	/// Collection of miscellaneous utility methods to use with DBPFFiles.
	/// </summary>
	public static class DBPFUtil {
		private static readonly string[] sc4Extensions = { "dat", "sc4lot", "sc4desc", "sc4model" };
		private static readonly byte[] DBPF = { 0x44, 0x42, 0x50, 0x46 };


		/// <summary>
		/// Filters a list of file paths based on SC4 file extensions.
		/// </summary>
		/// <param name="filesToFilter">List of all files to filter through</param>
		/// <param name="examineFileContents">Optionally examine the Header (first 28 bytes) of each file to determine if valid DBPF format. If set to false only the file extension will be examined.</param>
		/// <returns>Tuple of List&lt;FileInfo&gt;(dbpfFiles,skippedFiles)</returns>
		public static (List<FileInfo>, List<FileInfo>) FilterDBPFFiles(List<string> filesToFilter, bool examineFileContents) {
			List<FileInfo> dbpfFiles = new List<FileInfo>();
			List<FileInfo> skippedFiles = new List<FileInfo>();


			foreach (string item in filesToFilter) {
				FileInfo file = new FileInfo(item);
				if (!examineFileContents) {
					if (sc4Extensions.Any(file.Extension.Contains)) {
						dbpfFiles.Add(file);
					} else {
						skippedFiles.Add(file);
					}
				} else {
					if (IsValidDBPF(file)) { //https://stackoverflow.com/a/2912483/10802255
						dbpfFiles.Add(file);
					} else {
						skippedFiles.Add(file);
					}
				}
			}

			return (dbpfFiles, skippedFiles);
		}



        /// <summary>
        /// Examines the first bytes of the file to determine if the file is valid DBPF or not.
        /// </summary>
        /// <param name="filePath">Full File path of the file to examine</param>
        /// <param name="validateDBPFVersion">Should the version information in the header be validated</param>
        /// <returns>true if valid SC4 DBPF file, false otherwisee</returns>
		/// /// <remarks>
		/// In most circumstances, validateDBPFVersions should be omitted or set to false. If set to true, a temporary copy of <see cref="DBPFFile.DBPFHeader"/> is created to validate multiple fields to check for DBPF version 1.0 used with SC4. There is significantly more overhead with this call, especially when iterating over multiple files. 
		/// </remarks>
        public static bool IsValidDBPF(string filePath, bool validateDBPFVersion = false) {
            return IsValidDBPF(new FileInfo(filePath), validateDBPFVersion);
        }

        /// <summary>
        /// Examines the first bytes of the file to determine if the file is valid DBPF or not.
        /// </summary>
        /// <param name="file">File to examine</param>
        /// <param name="validateDBPFVersion">Should the version information in the header be validated</param>
        /// <returns>TRUE if valid SC4 DBPF file, FALSE otherwise</returns>
		/// <remarks>
		/// In most circumstances, validateDBPFVersions should be omitted or set to false. If set to true, a temporary copy of <see cref="DBPFFile.DBPFHeader"/> is created to validate multiple fields to check for DBPF version 1.0 used with SC4. There is significantly more overhead with this call, especially when iterating over multiple files. 
		/// </remarks>
        public static bool IsValidDBPF(FileInfo file, bool validateDBPFVersion = false) {
			FileStream fs = new FileStream(file.FullName, FileMode.Open);
			BinaryReader br = new BinaryReader(fs);

			if (validateDBPFVersion) {
				//To determine if the file is DBPF or not, can just look at the first few bytes which make up the header - no need to examine any of the rest of the file.
				try {
					DBPFFile.DBPFHeader header = new DBPFFile.DBPFHeader(br);
				}

				catch (InvalidDataException) {
					return false;
				}

				finally {
					br.Close();
					fs.Close();
				}

				return true;
			} 
			
			else {
                byte[] firstFour = br.ReadBytes(4); 
				br.Close();
                fs.Close();
                return firstFour.SequenceEqual(DBPF);
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
	}
}