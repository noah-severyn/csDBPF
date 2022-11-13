using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Linq;
using System.IO;
using System.Diagnostics;
using csDBPF.Properties;

namespace csDBPF {
	/// <summary>
	/// Represents the header data and entry list as read from a DBPF file.
	/// </summary>
	/// <remarks>
	/// At a high level, a <see cref="DBPFFile"/> ("file") is the container for the DBPF data. This takes the form of a dat/sc4lot/sc4model/sc4desc file. Each file is broken into one or more <see cref="DBPFEntry"/> ("entries" or "subfiles"). 
	/// For Exemplar and Cohort type entries, each entry is composed of one or more <see cref="DBPFProperty"/> ("properties"). Each property corresponds to one of <see cref="XMLExemplarProperty"/> which are generated from the properties XML file. This file stores useful and human friendly information about the property including name, min/max value, default values, etc.
	/// For other type entries, their data is stored in a byte array, and is interpreted (read) differently depending on the type of the entry.
	/// </remarks>
	public class DBPFFile {
		public DBPFHeader Header;
		public FileInfo File;
		public List<DBPFEntry> ListOfEntries; //TODO - make these unmodifiable outside of this scope. see https://stackoverflow.com/a/1710910/10802255
		public List<DBPFTGI> ListOfTGIs; //TODO - make these unmodifiable outside of this scope. see https://stackoverflow.com/a/1710910/10802255

		//------------- BEGIN DBPFFile.Header ------------- \\
		/// <summary>
		/// Contains DBPFFile header data, including all related fields.
		/// </summary>
		public class DBPFHeader {
			private string _identifier;
			private uint _majorVersion;
			private uint _minorVersion;
			private uint _dateCreated;
			private uint _dateModified;
			private uint _indexMajorVersion;
			private uint _indexEntryCount;
			private uint _indexEntryOffset;
			private uint _indexSize;
			public string Identifier {
				get { return _identifier; }
				set {
					string identifierDbpf = "DBPF";
					if (value.CompareTo(identifierDbpf) != 0) {
						throw new Exception("File is not a DBPF file!");
					} else {
						_identifier = value;
					}
				}
			}
			public uint MajorVersion {
				get { return _majorVersion; }
				set {
					if (value != 1) {
						throw new Exception("Unsupported major.minor version. Only 1.0 is supported for SC4 DBPF files.");
					} else {
						_majorVersion = value;
					}
				}
			}
			public uint MinorVersion {
				get { return _minorVersion; }
				set {
					if (value != 0) {
						throw new Exception("Unsupported major.minor version. Only 1.0 is supported for SC4 DBPF files.");
					} else {
						_minorVersion = value;
					}
				}
			}
			public uint DateCreated {
				get { return _dateCreated; }
				set { _dateCreated = value; }
			}
			public uint DateModified {
				get { return _dateModified; }
				set { _dateModified = value; }
			}
			public uint IndexMajorVersion {
				get { return _indexMajorVersion; }
				set {
					if (value != 7) {
						throw new Exception("Unsupported index version. Only 7 is supported for SC4 DBPF files.");
					} else {
						_indexMajorVersion = value;
					}
				}
			}
			public uint IndexEntryCount {
				get { return _indexEntryCount; }
				set { _indexEntryCount = value; }
			}
			public uint IndexEntryOffset {
				get { return _indexEntryOffset; }
				set { _indexEntryOffset = value; }
			}
			public uint IndexSize {
				get { return _indexSize; }
				set { _indexSize = value; }
			}


			/// <summary>
			/// Blank constructor used when also creating a new DBPFFile.
			/// </summary>
			internal DBPFHeader() { }


			/// <summary>
			/// External constructor for creating just a Header for the file. Often used when no other info about the file is required, resulting in much lower overhead as only the few bytes of the file that make up the <see cref="DBPFFile.DBPFHeader"/> are examined.
			/// </summary>
			/// <param name="filePath"></param>
			public DBPFHeader(string filePath) {
				ReadHeader(filePath);
			}

			/// <summary>
			/// Read and parse the first 48 bytes of a file.
			/// </summary>
			/// <param name="filePath">Full path of file to examine</param>
			private void ReadHeader(string filePath) {
				FileInfo file = new FileInfo(filePath);
				FileStream fs = new FileStream(file.FullName, FileMode.Open); //TODO - https://docs.microsoft.com/en-us/dotnet/standard/io/handling-io-errors
				BinaryReader br = new BinaryReader(fs);

				try {
					// Read Header Info
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
				finally {
					br.Close();
					fs.Close();
				}
			}

			public override string ToString() {
				StringBuilder sb = new StringBuilder();
				sb.Append($"Version: {MajorVersion}.{MinorVersion}; ");
				sb.Append($"Created: {DateCreated}; "); //Unix DateTime
				sb.Append($"Modified: {DateModified}; "); //Unix DateTime
				sb.Append($"Index Major Version: {IndexMajorVersion}; ");
				sb.Append($"Index Entry Count: {IndexEntryCount}; ");
				sb.Append($"Index Offset Location: {IndexEntryOffset}; ");
				sb.Append($"Index Size: {IndexSize}; ");
				return sb.ToString();
			}
		}



		//------------- BEGIN DBPFFile ------------- \\
		/// <summary>
		/// Read from an existing DBPF file and instantiate a new DBPFFile object.
		/// </summary>
		/// <param name="filePath">Full path of file to read, including filename and extension.</param>
		public DBPFFile(string filePath) {
			File = new FileInfo(filePath);
			Header = new DBPFHeader();
			ListOfEntries = new List<DBPFEntry>();
			ListOfTGIs = new List<DBPFTGI>();

			bool map = false;
			if (map) {
				ReadAndMap(File);
			} else {
				Read(File);
			}


		}


		public override string ToString() { //TODO - implement DBPFFile.ToString
			return base.ToString();
		}



		/// <summary>
		/// Reads a DBPF file.
		/// </summary>
		/// <remarks>
		/// Use only for short-lived DBPF files for which the content does not change on disk, or does not matter if it does, or if the file is small. Example: only scanning the TGIs of a file.
		/// </remarks>
		/// <param name="file">File of the DBPF object to be used.</param>
		/// <returns>A new DBPFFile object</returns>
		/// <see cref="https://www.wiki.sc4devotion.com/index.php?title=DBPF#Pseudocode"/>
		private void Read(FileInfo file) {
			FileStream fs = new FileStream(file.FullName, FileMode.Open); //TODO - https://docs.microsoft.com/en-us/dotnet/standard/io/handling-io-errors
			BinaryReader br = new BinaryReader(fs);

			try {
				// Read Header Info
				Header.Identifier = ByteArrayHelper.ToAString(br.ReadBytes(4));
				Header.MajorVersion = br.ReadUInt32();
				Header.MinorVersion = br.ReadUInt32();
				br.BaseStream.Seek(12, SeekOrigin.Current); //skip 8 unused bytes
				Header.DateCreated = br.ReadUInt32();
				Header.DateModified = br.ReadUInt32();
				Header.IndexMajorVersion = br.ReadUInt32();
				Header.IndexEntryCount = br.ReadUInt32();
				Header.IndexEntryOffset = br.ReadUInt32();
				Header.IndexSize = br.ReadUInt32();

				//Read Index Info
				long len = br.BaseStream.Length;
				br.BaseStream.Seek((Header.IndexEntryOffset), SeekOrigin.Begin);
				for (int idx = 0; idx < (Header.IndexEntryCount); idx++) {
					uint typeID = br.ReadUInt32();
					uint groupID = br.ReadUInt32();
					uint instanceID = br.ReadUInt32();
					uint offset = br.ReadUInt32();
					uint size = br.ReadUInt32();

					DBPFTGI tgi = new DBPFTGI(typeID, groupID, instanceID);
					DBPFEntry entry = new DBPFEntry(tgi, offset, size, (uint) idx);
					AddEntry(entry);
				}

				//Check for a DIR Record, aka the list of all compressed files (https://www.wiki.sc4devotion.com/index.php?title=DBDF)
				foreach (DBPFEntry entry in ListOfEntries) {
					if (entry.MatchesKnownEntryType(DBPFTGI.DIRECTORY)) { //Type: e86b1eef
						br.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
						int numRecords = (int) entry.CompressedSize / 16;
						for (int idx = 0; idx < numRecords; idx++) {
							//TODO - set uncompressed size here
						}


					}
				}

				//Populate data for non directory entries
				foreach (DBPFEntry entry in ListOfEntries) {
					if (!entry.MatchesKnownEntryType(DBPFTGI.DIRECTORY)) { //Type: e86b1eef
						byte[] readData = new byte[entry.UncompressedSize];
						br.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
						readData = br.ReadBytes(readData.Length);
						entry.ByteData = readData;

						//After the data is set, we can know the other properties of the DBPFEntry, like isCompressed, compressedSize, etc.
						entry.IsCompressed = DBPFCompression.IsCompressed(entry.ByteData);
						entry.CompressedSize = DBPFCompression.GetDecompressedSize(entry.ByteData);
						
					}
				}

			}
			finally {
				br.Close();
				fs.Close();
			}


			//Parse the properties of each entry
			foreach (DBPFEntry entry in ListOfEntries) {
				//GetSubfileFormat(DBPFCompression.Decompress(entry.data));
			}
		}


		/// <summary>
		/// Adds an entry to the entryMap and the TGI of that entry to the tgiMap.
		/// </summary>
		/// <param name="entry">Entry to add</param>
		private void AddEntry(DBPFEntry entry) {
			if (entry == null) {
				throw new ArgumentNullException();
			}
			ListOfEntries.Add(entry);
			ListOfTGIs.Add(entry.TGI);
		}


		/// <summary>
		/// Decodes all entries in the file. See <see cref="DBPFEntry.DecodeEntry"/> for more information
		/// </summary>
		public void DecodeAllEntries() {
			foreach (DBPFEntry entry in ListOfEntries) {
				entry.DecodeEntry();
			}
		}


		/// <summary>
		/// Reads a DBPF file and maps the file from disk to memory.
		/// </summary>
		/// <remarks>
		/// Serves a similar purpose to <see cref="Read(FileInfo)"/>, but it is capable of writing large amount of entries quicker - suitable for large files and files of which the ______ methods will be called frequently.
		/// </remarks>
		/// <see cref="Read(FileInfo)"/>
		/// <param name="file">File of the DBPF object to be used.</param>
		private DBPFFile ReadAndMap(FileInfo file) {
			//this.Read(this.file);
			throw new NotImplementedException();
		}

		//TODO - also implement readCached https://github.com/memo33/jDBPFX/blob/master/src/jdbpfx/DBPFFile.java#L721


		//private string GetSubfileFormat(byte[] dData) {
		//	string identifier;
		//	StringBuilder sb = new StringBuilder();
		//	for (int idx = 0; idx < 5; idx++) {
		//		sb.Append(dData[idx]);
		//	}
		//	identifier = sb.ToString();

		//	//Exemplars (EXMP) first 5 bytes EQZB1 or EQZT1
		//	if (identifier == "EQZB1" || identifier == "EQZT1") {
		//		return "EXMP";
		//	}

		//	//Cohorts (EXMP) first 5 bytes CQZB1 or CQZT1
		//	if (identifier == "CQZB1" || identifier == "CQZT1") {
		//		return "EXMP";
		//	}

		//	return null;
		//}






		//public class DirectDBPFEntry : DBPFEntry {
		//	private readonly uint offset;
		//	private readonly uint size;
		//	private readonly uint index;

		//	//Create a DBPF entry
		//	public DirectDBPFEntry(DBPFTGI tgi, uint offset, uint size, uint index) : base(tgi) {
		//		this.offset = offset;
		//		this.size = size;
		//		this.index = index;
		//	}

		//	//TODO - method equals is unimplemented in memo's code
		//	//TODO - method hashCode is unimplemented in memo's code
		//	public override string ToString() {
		//		return this.TGI.ToString() + " " + this.TGI.label.ToString();
		//	}
		//	public DBPFFile GetEncolsingDBPFFile() {
		//		//return DBPFFile.this; //TODO - huh? this doesnt work
		//		throw new NotImplementedException();
		//	}

		//	/// <summary>
		//	/// Returns a string with this entry's TGI, offset, and size.
		//	/// </summary>
		//	/// <returns>This entry <see cref="ToString"/> with offset and size appended on.</returns>
		//	public string ToDetailString() {
		//		StringBuilder sb = new StringBuilder(ToString());
		//		sb.AppendLine($"Offset: {DBPFUtil.UIntToHexString(offset, 8)} Size: {size}");
		//		return sb.ToString();
		//	}
		//}






		//------------- DBPFFile Static Methods ------------- \\
		public static OrderedDictionary FilterEntries(OrderedDictionary listOfEntries) {
			throw new NotImplementedException();
		}
	}
}