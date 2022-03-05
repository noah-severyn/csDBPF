using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace csDBPF {
	/// <summary>
	/// Represents the header data and entry list as read from a DBPF file.
	/// </summary>
	public class DBPFFile {
		public Header header;
		public FileInfo file;
		public OrderedDictionary entryMap; //TODO - make these unmodifiable outside of this scope. see (Java) Collections.unmodifiableSet
		public Dictionary<uint, DBPFTGI> tgiMap; //TODO - make these unmodifiable outside of this scope. see (Java) Collections.unmodifiableSet

		/// <summary>
		/// Contains DBPFFile header data, including all related fields.
		/// </summary>
		public class Header {
			private uint _identifier;
			private uint _majorVersion;
			private uint _minorVersion;
			private uint _dateCreated;
			private uint _dateModified;
			private uint _indexMajorVersion;
			private uint _indexEntryCount;
			private uint _indexEntryOffset;
			private uint _indexSize;
			public uint identifier {
				get { return _identifier; }
				set {
					uint identifierDbpf = (uint) 0x44425046; //1145196614 decimal = 44425046 hex = DBPF ASCII
					if (value.CompareTo(identifierDbpf) != 0) {
						throw new Exception("File is not a DBPF file!");
					} else {
						_identifier = value;
					}
				}
			}
			public uint majorVersion {
				get { return _majorVersion; }
				set {
					if (value != (uint) 0x1000000) { //16777216 decimal = 1000000 hex
						throw new Exception("Unsupported major.minor version. Only 1.0 is supported for SC4 DBPF files.");
					} else {
						_majorVersion = value;
					}
				}
			}
			public uint minorVersion {
				get { return _minorVersion; }
				set {
					if (value != (uint) 0) {
						throw new Exception("Unsupported major.minor version. Only 1.0 is supported for SC4 DBPF files.");
					} else {
						_minorVersion = value;
					}
				}
			}
			public uint dateCreated {
				get { return _dateCreated; }
				set { _dateCreated = value; }
			}
			public uint dateModified {
				get { return _dateModified; }
				set { _dateModified = value; }
			}
			public uint indexMajorVersion {
				get { return _indexMajorVersion; }
				set {
					if (value != (uint) 0x7000000) { //117440512 decimal = 7000000 hex
						throw new Exception("Unsupported index version. Only 7 is supported for SC4 DBPF files.");
					} else {
						_indexMajorVersion = value;
					}
				}
			}
			public uint indexEntryCount {
				get { return _indexEntryCount; }
				set { _indexEntryCount = value; }
			}
			public uint indexEntryOffset {
				get { return _indexEntryOffset; }
				set { _indexEntryOffset = value; }
			}
			public uint indexSize {
				get { return _indexSize; }
				set { _indexSize = value; }
			}

			//empty constructor to prevent automatic creation of blank constructor and assigning default values to fields
			public Header() { }

			public override string ToString() {
				StringBuilder sb = new StringBuilder();
				sb.Append($"Version: {majorVersion}.{minorVersion}; ");
				sb.Append($"Created: {dateCreated}; "); // TODO - add functions to output these in a readable format - possibly functions in a util class?
				sb.Append($"Modified: {dateModified}; "); // TODO - add functions to output these in a readable format - possibly functions in a util class?
				sb.Append($"Index Major Version: {indexMajorVersion}; ");
				sb.Append($"Index Entry Count: {indexEntryCount}; ");
				sb.Append($"Index Offset Location: {indexEntryOffset}; ");
				sb.Append($"Index Size: {indexSize}; ");
				return sb.ToString();
			}
		}

		//------------- BEGIN DBPFFile ------------- \\
		/// <summary>
		/// Read from an existing DBPF file and instantiate a new DBPFFile object.
		/// </summary>
		/// <param name="filePath">Full path of file to read, including filename and extension.</param>
		public DBPFFile(string filePath) {
			this.file = new FileInfo(filePath);
			this.header = new Header();
			this.entryMap = new OrderedDictionary();
			this.tgiMap = new Dictionary<uint, DBPFTGI>();

			bool map = false;
			if (map) {
				this.ReadAndMap(this.file);
			} else {
				this.Read(this.file);
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
			entryMap.Add(entry.indexPos, entry);
			tgiMap.Add(entry.indexPos, entry.TGI);
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
				this.header.identifier = DBPFUtil.ReverseBytes(br.ReadUInt32());
				this.header.majorVersion = DBPFUtil.ReverseBytes(br.ReadUInt32());
				this.header.minorVersion = DBPFUtil.ReverseBytes(br.ReadUInt32());
				br.BaseStream.Seek(12, SeekOrigin.Current); //skip 8 unused bytes
				this.header.dateCreated = DBPFUtil.ReverseBytes(br.ReadUInt32());
				this.header.dateModified = DBPFUtil.ReverseBytes(br.ReadUInt32());
				this.header.indexMajorVersion = DBPFUtil.ReverseBytes(br.ReadUInt32());
				this.header.indexEntryCount = DBPFUtil.ReverseBytes(br.ReadUInt32());
				this.header.indexEntryOffset = br.ReadUInt32(); //TODO - figure out why this works as it's different than all of the others ... unless none of the uint should be reversed???
				this.header.indexSize = DBPFUtil.ReverseBytes(br.ReadUInt32());

				//Read Index Info
				long len = br.BaseStream.Length;
				br.BaseStream.Seek((this.header.indexEntryOffset), SeekOrigin.Begin);
				for (int idx = 0; idx < (this.header.indexEntryCount >> 24); idx++) {
					uint typeID = br.ReadUInt32();
					uint groupID = br.ReadUInt32();
					uint instanceID = br.ReadUInt32();
					uint offset = br.ReadUInt32();
					uint size = br.ReadUInt32();

					DBPFTGI tgi = new DBPFTGI(typeID, groupID, instanceID);
					DBPFEntry entry = new DBPFEntry(tgi, offset, size, (uint) idx);
					this.AddEntry(entry);
					Trace.WriteLine(tgi.ToString());
				}

				//Check for a DIR Record (https://www.wiki.sc4devotion.com/index.php?title=DBDF)
				foreach (DBPFEntry entry in this.entryMap.Values) {
					if (entry.TGI.MatchesKnownTGI(DBPFTGI.DIRECTORY)) { //Type: e86b1eef
						br.BaseStream.Seek(entry.offset, SeekOrigin.Begin);
						int numRecords = (int) entry.compressedSize / 16;
						for (int idx = 0; idx < numRecords; idx++) {

						}


					}
				}
			}
			finally {
				br.Close();
				fs.Close();
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



		//private DBPFFile(string filePath, uint majorVersion, uint minorVersion, uint dateCreated, uint dateModified, uint indexMajorVersion, uint indexEntryCount, uint indexEntryOffset, uint indexSize) {
		//	this.file = new FileInfo(filePath);
		//	this.header = new Header();

		//	this.header.majorVersion = majorVersion;
		//	this.header.minorVersion = minorVersion;
		//	this.header.dateCreated = dateCreated;
		//	this.header.dateModified = dateModified;
		//	this.header.indexMajorVersion = indexMajorVersion;
		//	this.header.indexEntryCount = indexEntryCount;
		//	this.header.indexEntryOffset = indexEntryOffset;
		//	this.header.indexSize = indexSize;
		//	this.entryMap = new OrderedDictionary();
		//	this.tgiMap = new Dictionary<uint, DBPFTGI>();
		//}



		//TODO - implement read function, also readMapped https://github.com/memo33/jDBPFX/blob/master/src/jdbpfx/DBPFFile.java#L659


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
		//		throw new NotImplementedException(); //TODO - implement this
		//	}
		//	public DBPFFile GetEncolsingDBPFFile() {
		//		//return DBPFFile.this; //TODO - huh? this doesnt work
		//		throw new NotImplementedException();
		//	}

		//	public string ToDetailString() {
		//		StringBuilder sb = new StringBuilder(ToString());
		//		sb.AppendLine($"Offset: {DBPFUtil.ToHex(offset, 8)} Size: {size}");
		//		return sb.ToString();
		//	}
		//}
	}
}