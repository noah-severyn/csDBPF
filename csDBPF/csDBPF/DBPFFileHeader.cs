using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csDBPF {
    public partial class DBPFFile {
        /// <summary>
		/// Stores key information about the DBPFFile. The Header is the first 96 bytes of the DBPFFile. 
		/// </summary>
		public class DBPFHeader {
            //Only have backing fields for the fields with setter logic
            private string _identifier;
            private uint _majorVersion;
            private uint _minorVersion;
            private uint _indexMajorVersion;

            /// <summary>
            /// File type identifier. Must be "DBPF".
            /// </summary>
            public string Identifier {
                get { return _identifier; }
                private set {
                    string identifierDbpf = "DBPF";
                    if (value.CompareTo(identifierDbpf) != 0) {
                        throw new InvalidDataException("File is not a DBPF file!");
                    } else {
                        _identifier = value;
                    }
                }
            }
            /// <summary>
            /// DBPF format major version. Always 1 for SC4.
            /// </summary>
            public uint MajorVersion {
                get { return _majorVersion; }
                private set {
                    if (value != 1) {
                        throw new InvalidDataException("Unsupported major.minor version. Only 1.0 is supported for SC4 DBPF files.");
                    } else {
                        _majorVersion = value;
                    }
                }
            }
            /// <summary>
            /// DBPF format minor version. Always 0 for SC4.
            /// </summary>
            public uint MinorVersion {
                get { return _minorVersion; }
                private set {
                    if (value != 0) {
                        throw new InvalidDataException("Unsupported major.minor version. Only 1.0 is supported for SC4 DBPF files.");
                    } else {
                        _minorVersion = value;
                    }
                }
            }
            /// <summary>
            /// Creation time in Unix timestamp format.
            /// </summary>
            public uint DateCreated { get; private set; }
            /// <summary>
            /// Modification time in Unix timestamp format.
            /// </summary>
            public uint DateModified { get; private set; }
            /// <summary>
            /// Defines the Index version. Always 7 for SC4.
            /// </summary>
            public uint IndexMajorVersion {
                get { return _indexMajorVersion; }
                private set {
                    if (value != 7) {
                        throw new InvalidDataException("Unsupported index version. Only 7 is supported for SC4 DBPF files.");
                    } else {
                        _indexMajorVersion = value;
                    }
                }
            }
            /// <summary>
            /// Number of subfiles within this file.
            /// </summary>
            /// <remarks>The index table is very similar to the directory file (DIR) within a DPBF package. The difference being that the Index Table lists every file in the package, whereas the directory file only lists the compressed files within the package.
            /// </remarks>
            public uint IndexEntryCount { get; private set; }
            /// <summary>
            /// Byte location of the first index entry in the file.
            /// </summary>
            public uint IndexEntryOffset { get; private set; }
            /// <summary>
            /// Size of the index table in bytes. Equivalent to <c>IndexEntryCount * 20</c> bytes.
            /// </summary>
            public uint IndexSize { get; private set; }


            /// <summary>
            /// Initialize a header with default values.
            /// </summary>
            public DBPFHeader() {
                Identifier = "DBPF";
                MajorVersion = 1;
                MinorVersion = 0;
                DateCreated = (uint) DateTimeOffset.Now.ToUnixTimeSeconds();
                DateModified = 0;
                IndexMajorVersion = 7;
                IndexEntryCount = 0;
                IndexEntryOffset = 0;
                IndexSize = 0;
            }
            /// <summary>
            /// Initialize Header information from an existing stream.
            /// </summary>
            /// <param name="br">Stream to read from</param>
            public DBPFHeader(BinaryReader br) {
                Identifier = ByteArrayHelper.ToAString(br.ReadBytes(4));
                MajorVersion = br.ReadUInt32();
                MinorVersion = br.ReadUInt32();
                br.BaseStream.Seek(12, SeekOrigin.Current); //skip 8 unused bytes
                DateCreated = br.ReadUInt32();
                DateModified = br.ReadUInt32();
                IndexMajorVersion = br.ReadUInt32();
                IndexEntryCount = br.ReadUInt32();
                IndexEntryOffset = br.ReadUInt32();
                IndexSize = br.ReadUInt32();
            }

            /// <summary>
            /// Returns a string that represents the current object.
            /// </summary>
            /// <returns>Returns a string that represents the current object.</returns>
            public override string ToString() {
                StringBuilder sb = new StringBuilder();
                sb.Append($"Version: {MajorVersion}.{MinorVersion}; ");
                sb.Append($"Created: {DateCreated}; ");
                sb.Append($"Modified: {DateModified}; ");
                sb.Append($"Index Major Version: {IndexMajorVersion}; ");
                sb.Append($"Index Entry Count: {IndexEntryCount}; ");
                sb.Append($"Index Offset Location: {IndexEntryOffset}; ");
                sb.Append($"Index Size: {IndexSize}; ");
                return sb.ToString();
            }


            /// <summary>
            /// Update header fields to the current state of the DBPF file.
            /// </summary>
            /// <param name="dbpf">DBPFFile to examine</param>
            internal void Update(DBPFFile dbpf) {
                DateModified = (uint) DateTimeOffset.Now.ToUnixTimeSeconds();
                IndexEntryCount = (uint) dbpf.CountEntries();
                IndexEntryOffset = (uint) dbpf.DataSize + 96;
                IndexSize = IndexEntryCount * 20; //each Index entry has 5x uint values: T, G, I, offset, size
            }

            //internal byte[] Encode() {
            //	byte[] bytes = new byte[96];

            //             //Update and write Header
            //             //if (Identifier is null) {
            //             //    InitializeBlank();
            //             //}
            //             //Update();
            //             //using FileStream fs = new(file.FullName, FileMode.Create);

            //             fs.Write(ByteArrayHelper.ToBytes(Header.Identifier, true));
            //             fs.Write(BitConverter.GetBytes(Header.MajorVersion));
            //             fs.Write(BitConverter.GetBytes(Header.MinorVersion));
            //             fs.Write(new byte[12]); //12 bytes are unused
            //             fs.Write(BitConverter.GetBytes(Header.DateCreated));
            //             fs.Write(BitConverter.GetBytes(Header.DateModified));
            //             fs.Write(BitConverter.GetBytes(Header.IndexMajorVersion));
            //             fs.Write(BitConverter.GetBytes(Header.IndexEntryCount));
            //             fs.Write(BitConverter.GetBytes(Header.IndexEntryOffset));
            //             fs.Write(BitConverter.GetBytes(Header.IndexSize));
            //             fs.Write(new byte[48]);
            //         }
        }
    }
}
