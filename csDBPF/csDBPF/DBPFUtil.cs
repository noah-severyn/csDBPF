using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace csDBPF {
    /// <summary>
    /// A collection of utility methods to use with DBPF files.
    /// </summary>
    public static partial class DBPFUtil {
        /// <summary>
        /// Valid SC4 file extensions.
        /// </summary>
        /// <remarks>
        /// Includes extensions <c>.dat</c>, <c>.sc4lot</c>, <c>.sc4desc</c>, <c>.sc4model</c>, <c>.dll</c>, and <c>.ini</c>.
        /// </remarks>
        public static readonly HashSet<string> SC4Extensions = new(StringComparer.OrdinalIgnoreCase) {
            ".dat", ".sc4lot", ".sc4desc", ".sc4model", ".dll", ".ini"
        };
        private static readonly byte[] DBPF = [0x44, 0x42, 0x50, 0x46];

        /// <summary>
        /// Filters a list of file paths for known SC4 file extensions, or optionally examining the file's first four bytes for the magic identifier instead.
        /// </summary>
        /// <param name="filesToFilter">List of all files to filter through</param>
        /// <param name="validateIdentifier">Optionally examine the first 4 bytes of each for a valid DBPF format. If omitted or set to <see langword="false"/>, only the file extension will be examined.</param>
        /// <returns>A listing of DBPF files</returns>
        public static IEnumerable<string> FilterDBPFFiles(this IEnumerable<string> filesToFilter, bool validateIdentifier = false) {
            List<string> dbpfFiles = [];
            foreach (string file in filesToFilter) {
                if (file.IsDBPF(validateIdentifier)) {
                    dbpfFiles.Add(file);
                }
            }
            return dbpfFiles;
        }



        /// <summary>
        /// Determine if a file is a DBPF file via its extension, or optionally examining the file's first four bytes for the magic identifier instead.
        /// </summary>
        /// <param name="filePath">Full file path to examine</param>
        /// <param name="validateIdentifier">Optionally examine the first 4 bytes of each for a valid DBPF format. If omitted or set to <see langword="false"/>, only the file extension will be examined.</param>
        /// <returns><see langword="true"/> if file is a SC4 DBPF file; otherwise, <see langword="false"/></returns>
        public static bool IsDBPF(this string filePath, bool validateIdentifier = false) {
            if (!validateIdentifier) {
                string extension = Path.GetExtension(filePath);
                return SC4Extensions.Contains(extension);
            }

            try {
                // Fast fail if file doesn't exist or is too small
                FileInfo fi = new FileInfo(filePath);
                if (!fi.Exists || fi.Length < 4) {
                    return false;
                }

                using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4);
                Span<byte> buffer = stackalloc byte[4];
                int bytesRead = fs.Read(buffer);

                // Manual comparison is faster than byte[].SequenceEqual for exactly 4 bytes
                return buffer[0] == DBPF[0] && buffer[1] == DBPF[1] && buffer[2] == DBPF[2] && buffer[3] == DBPF[3];
            }
            catch {
                return false;
            }
        }

        /// <summary>
        /// Determine if a file is a DBPF file via its extension, and optionally examine its first four bytes for the magic string.
        /// </summary>
        /// <param name="file">File to examine</param>
        /// <param name="validateIdentifier">Optionally examine the first 4 bytes of each for a valid DBPF format. If omitted or set to <see langword="false"/>, only the file extension will be examined.</param>
        /// <returns><see langword="true"/> if file is a SC4 DBPF file; otherwise, <see langword="false"/></returns>
        public static bool IsDBPF(this FileInfo file, bool validateIdentifier = false) {
            return IsDBPF(file.FullName, validateIdentifier);
        }



        /// <summary>
        /// Returns the uppercase string representation of the provided uint converted to hex, padded by the specified number of places.
        /// </summary>
        /// <param name="value">Value to return</param>
        /// <param name="places">Number of places to pad the value. 0-8 valid; 8 is default</param>
        /// <param name="uppercase">Specify output as uppercase. Default is lowercase.</param>
        /// <param name="prefix">Optionally prepend the hex string with "0x". Default is <see langword="true"/> to include.</param>
        /// <returns>A formatted hex string representing the value</returns>
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
        /// <param name="prefix">Optionally prepend the hex string with "0x". Default is <see langword="true"/> to include.</param>
        /// <returns>A formatted hex string representing the value, or an empty string if <paramref name="value"/> is null</returns>
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
                return string.Empty;
            }
        }



        /// <summary>
        /// Formats a string of TGI values in the same format as <see cref="TGI.ToString"/>.
        /// </summary>
        /// <remarks>The input string must contain three hexadecimal numbers, each prefixed with <c>0x</c>, but may be split with any delimiter.</remarks>
        /// <param name="tgi">TGI string to parse</param>
        /// <exception cref="ArgumentException">If the TGI string is in an improper format</exception>
        /// <returns>A  TGI properly formated delimited by comma space, in the format of <c>0x########, 0x########, 0x########</c>, with leading zeros added up to 8 characters each.</returns>
        public static string FormatTgiString(string tgi) {
            if (Regex.Matches(tgi, "0x").Count != 3) {
                throw new ArgumentException($"TGI of <{tgi}> is not in the proper format.");
            }

            int startPos = tgi.IndexOf("0x", 2); //Find non-alphanumeric delimiter based on locn of the second '0x'
            int idx = startPos;
            do {
                idx--;
            } while (!char.IsLetterOrDigit(tgi[idx]));
            idx++;
            string separator = tgi.Substring(idx, startPos - idx);

            string cleaned = tgi.Replace(separator, ", ");
            int firstDelim = cleaned.IndexOf(',');
            int secondDelim = cleaned.IndexOf(',', firstDelim + 1);

            string x1 = cleaned.Substring(2, firstDelim - 2).PadLeft(8, '0');
            string x2 = cleaned.Substring(firstDelim + 4, secondDelim - firstDelim - 4).PadLeft(8, '0');
            string x3 = cleaned.Substring(secondDelim + 4, cleaned.Length - secondDelim - 4).PadLeft(8, '0');

            return $"0x{x1}, 0x{x2}, 0x{x3}";
        }



        /// <summary>
        /// Convert Unix timestamp to a <see cref="DateTime"/> object.
        /// </summary>
        /// <param name="time">A Unix timestamp.</param>
        /// <returns>A <see cref="DateTime"/> object equal to the provided Unix timestamp</returns>
        public static DateTime UnixToDateTime(uint time) {
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