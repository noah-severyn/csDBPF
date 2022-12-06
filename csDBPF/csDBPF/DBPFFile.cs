using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Linq;
using System.IO;
using System.Diagnostics;
using csDBPF.Properties;
using System.Net;
using System.Drawing;
using csDBPF.EntryTypes;

namespace csDBPF {
	/// <summary>
	/// Represents the header data and entry list as read from a DBPF file.
	/// </summary>
	/// <remarks>
	/// At a high level, a <see cref="DBPFFile"/> ("file") is the container for the DBPF data. This takes the form of a .dat/.sc4lot/.sc4model/.sc4desc file. Each file is broken into one or more <see cref="DBPFEntry"/> ("entries" or "subfiles"). 
	/// For Exemplar and Cohort type entries, each entry is composed of one or more <see cref="DBPFProperty"/> ("properties"). Each property corresponds to one of <see cref="XMLExemplarProperty"/> which are generated from the properties XML file. This file stores useful and human friendly information about the property including name, min/max value, default values, etc.
	/// For other type entries, their data is stored in a byte array, and is interpreted differently depending on the type of the entry.
	/// </remarks>
	public class DBPFFile {
		public DBPFHeader Header;
		public FileInfo File;
		public List<DBPFEntry> ListOfEntries; //TODO - make these unmodifiable outside of this scope. see https://stackoverflow.com/a/1710910/10802255
		public List<DBPFTGI> ListOfTGIs; //TODO - make these unmodifiable outside of this scope. see https://stackoverflow.com/a/1710910/10802255

		//------------- BEGIN DBPFFile.Header ------------- \\
		/// <summary>
		/// Stores key information about the DBPFFile. The Header is the first 48 bytes of the DBPFFile. 
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

			/// <summary>
			/// File type identifier. Must be "DBPF".
			/// </summary>
			public string Identifier {
				get { return _identifier; }
				set {
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
				set {
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
				set {
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
			public uint DateCreated {
				get { return _dateCreated; }
				set { _dateCreated = value; }
			}
			/// <summary>
			/// Modification time in Unix timestamp format.
			/// </summary>
			public uint DateModified {
				get { return _dateModified; }
				set { _dateModified = value; }
			}
			/// <summary>
			/// Defines the Index verion. Always 7 for SC4.
			/// </summary>
			public uint IndexMajorVersion {
				get { return _indexMajorVersion; }
				set {
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
			/// <remarks>The index table is very similar to the directory file (DIR) within a DPBF package. The difference being that the Index Table lists every file in the package, whereas the directory file only lists the compressed files within the package. Reader presents a directory file that is a mashup of these two entities, listing every file in the package, as well as indicating whether or not that particular file is compressed. </remarks>
			public uint IndexEntryCount {
				get { return _indexEntryCount; }
				set { _indexEntryCount = value; }
			}
			/// <summary>
			/// Byte location of the first index in the file.
			/// </summary>
			public uint IndexEntryOffset {
				get { return _indexEntryOffset; }
				set { _indexEntryOffset = value; }
			}
			/// <summary>
			/// Size of the index table in bytes.
			/// </summary>
			public uint IndexSize {
				get { return _indexSize; }
				set { _indexSize = value; }
			}


			/// <summary>
			/// Instantiate a new DBPFHeader. All properties remain unset until one of the Intialize() functions are called.
			/// </summary>
			public DBPFHeader() { }


			/// <summary>
			/// Initialize Header information from an existing stream.
			/// </summary>
			/// <param name="br">Stream to read from.</param>
			/// <exception cref="InvalidDataException">If file is not valid DBPF format.</exception>
			public void Initialize(BinaryReader br) {
				_identifier = ByteArrayHelper.ToAString(br.ReadBytes(4));
				_majorVersion = br.ReadUInt32();
				_minorVersion = br.ReadUInt32();
				br.BaseStream.Seek(12, SeekOrigin.Current); //skip 8 unused bytes
				_dateCreated = br.ReadUInt32();
				_dateModified = br.ReadUInt32();
				_indexMajorVersion = br.ReadUInt32();
				_indexEntryCount = br.ReadUInt32();
				_indexEntryOffset = br.ReadUInt32();
				_indexSize = br.ReadUInt32();
			}

			/// <summary>
			/// Initialize Header information with default values.
			/// </summary>
			public void InitializeBlank() {
				_identifier = "DBPF";
				_majorVersion = 1;
				_minorVersion = 0;
				_dateCreated = (uint) DateTimeOffset.Now.ToUnixTimeSeconds();
				_dateModified = 0;
				_indexMajorVersion = 7;
				_indexEntryCount = 0;
				_indexEntryOffset = 0;
				_indexSize = 0;
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
		}



		//------------- BEGIN DBPFFile ------------- \\
		/// <summary>
		/// Instantiates a DBPFFile from a file path. If the file exists, its contents are read into the new DBPFFile; if the file does not exist then a new DBPFFile is created with default values.
		/// </summary>
		/// <param name="fileName">File to read</param>
		public DBPFFile(string fileName) : this(new FileInfo(fileName)) {}

		/// <summary>
		/// Instantiates a DBPFFile from a FileInfo object. If the file exists, its contents are read into the new DBPFFile; if the file does not exist then a new DBPFFile is created with default values.
		/// </summary>
		/// <param name="file">File to read</param>
		public DBPFFile(FileInfo file) {
			File = file;
			Header = new DBPFHeader();
			ListOfEntries = new List<DBPFEntry>();
			ListOfTGIs = new List<DBPFTGI>();

			if (!file.Exists) {
				Header.InitializeBlank();
				return;
			}

			bool map = false;
			try {
				if (map) {
					ReadAndMap(File);
				} else {
					Read(File);
				}
			} 
			catch (InvalidDataException) { 
				
			}
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>Returns a string that represents the current object.</returns>
		public override string ToString() {
			return $"{File.Name}: {Header.IndexEntryCount} subfiles";
		}



		/// <summary>
		/// Reads a DBPF file.
		/// </summary>
		/// <remarks>
		/// Use only for short-lived DBPF files for which the content does not change on disk, or does not matter if it does, or if the file is small. Example: only scanning the TGIs of a file.
		/// </remarks>
		/// <param name="file">File of the DBPF object to be used.</param>
		/// <returns>A new DBPFFile object</returns>
		/// <see ref="https://www.wiki.sc4devotion.com/index.php?title=DBPF#Pseudocode"/>
		private void Read(FileInfo file) {
			FileStream fs = new FileStream(file.FullName, FileMode.Open); //TODO - https://docs.microsoft.com/en-us/dotnet/standard/io/handling-io-errors
			BinaryReader br = new BinaryReader(fs); 

			try {
				// Read Header Info
				Header.Initialize(br);

				//Read Index Info
				List<uint> offsets = new List<uint>();
				List<uint> sizes = new List<uint>();
				byte[] byteData;

				br.BaseStream.Seek((Header.IndexEntryOffset), SeekOrigin.Begin);
				for (int idx = 0; idx < (Header.IndexEntryCount); idx++) {
					uint typeID = br.ReadUInt32();
					uint groupID = br.ReadUInt32();
					uint instanceID = br.ReadUInt32();
					uint offset = br.ReadUInt32();
					uint size = br.ReadUInt32();

					DBPFTGI tgi = new DBPFTGI(typeID, groupID, instanceID);
					ListOfTGIs.Add(tgi);
					offsets.Add(offset);
					sizes.Add(size);
				}

				for (int idx = 0; idx < ListOfTGIs.Count; idx++) {
					br.BaseStream.Seek(offsets[idx], SeekOrigin.Begin);
					byteData = br.ReadBytes((int) sizes[idx]);

					//TODO - should add two levels of label: base and detail
					switch (ListOfTGIs[idx].) {
						case "EXEMPLAR":
							ListOfEntries.Add(new EntryLTEXT(ListOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));



						default:
							break;
					}
					ListOfEntries.Add(entry);
				}



				//foreach (DBPFTGI tgi in ListOfTGIs) {
				//	//Check for a DIR Record, aka the list of all compressed files (https://www.wiki.sc4devotion.com/index.php?title=DBDF)
				//	//Also a DIR is never compressed so CompressedSize=UncompressedSize
					
				//	if (entry.MatchesKnownEntryType(DBPFTGI.DIRECTORY)) { //Type: e86b1eef
				//		br.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
				//		entry.ByteData = br.ReadBytes((int) entry.CompressedSize);
				//		entry.IsCompressed = false;

				//		int numRecords = (int) entry.CompressedSize / 16;
				//	}

				//	//Populate data for non directory entries
				//	else {
				//		byte[] readData = new byte[entry.UncompressedSize];
				//		br.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
				//		readData = br.ReadBytes(readData.Length);
				//		entry.ByteData = readData;

				//		//After the data is set, we can know the other properties of the DBPFEntry, like isCompressed, compressedSize, etc.
				//		entry.IsCompressed = DBPFCompression.IsCompressed(entry.ByteData);
				//		if (entry.IsCompressed) {
				//			entry.CompressedSize = DBPFCompression.GetDecompressedSize(entry.ByteData);
				//		}
				//	}

					
				//}
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




		/// <summary>
		/// Decodes all entries in the file. See <see cref="DBPFEntry.DecodeEntry()"/> for more information
		/// </summary>
		public void DecodeAllEntries() {
			foreach (DBPFEntry entry in ListOfEntries) {
				entry.DecodeEntry();
			}
		}

		/// <summary>
		/// Return the nth entry in the file by index.
		/// </summary>
		/// <param name="index">Index position in file.</param>
		/// <returns></returns>
		public DBPFEntry GetEntry(int index) {
			return ListOfEntries[index];
		}
		/// <summary>
		/// Return the entry matching the specified Instance ID.
		/// </summary>
		/// <param name="instance">IID to search for</param>
		/// <returns></returns>
		public DBPFEntry GetEntry(uint instance) {
			return ListOfEntries.Find(i => i.TGI.Instance == instance);
		}
		/// <summary>
		/// Return the entry matching the specified TGI set.
		/// </summary>
		/// <param name="TGI">TGI set to search for</param>
		/// <returns></returns>
		public DBPFEntry GetEntry(DBPFTGI TGI) {
			throw new NotImplementedException();
		}

		/// <summary>
		/// Saves the current instance to disk using the <see cref="File">File</see> property.
		/// <param name="file">File to save as</param>
		/// </summary>
		public void Save(string? filePath) {
			FileInfo file;
			if (filePath is null) {
				file = File;
			} else {
				file = new FileInfo(filePath);
			}

			using FileStream fs = new(file.FullName, FileMode.Create);
			//Write Header
			fs.Write(ByteArrayHelper.ToByteArray(Header.Identifier));
			fs.Write(BitConverter.GetBytes(Header.MajorVersion));
			fs.Write(BitConverter.GetBytes(Header.MinorVersion));
			fs.Write(new byte[12]); //12 bytes are unused
			fs.Write(BitConverter.GetBytes(Header.DateCreated));
			fs.Write(BitConverter.GetBytes(Header.DateModified));
			fs.Write(BitConverter.GetBytes(Header.IndexMajorVersion));
			fs.Write(BitConverter.GetBytes(Header.IndexEntryCount));
			fs.Write(BitConverter.GetBytes(Header.IndexEntryOffset));
			fs.Write(BitConverter.GetBytes(Header.IndexSize));
			fs.Write(new byte[48]);

			//Write Entries
			foreach (DBPFEntry entry in ListOfEntries) {
				fs.Write(entry.ByteData);
			}

			//Write directory file
			//foreach (DBPFEntry entry in ListOfEntries) {
			//	if (entry.IsCompressed) {
			//		fs.Write(BitConverter.GetBytes(entry.TGI.Type.Value));
			//		fs.Write(BitConverter.GetBytes(entry.TGI.Group.Value));
			//		fs.Write(BitConverter.GetBytes(entry.TGI.Instance.Value));
			//		fs.Write(BitConverter.GetBytes(entry.CompressedSize));
			//	}
			//}

			//Write Index
			//--should be done after all content has been written because we need to know the location of each file in the archive and its size.
			foreach (DBPFEntry entry in ListOfEntries) {
				fs.Write(BitConverter.GetBytes(entry.TGI.Type.Value));
				fs.Write(BitConverter.GetBytes(entry.TGI.Group.Value));
				fs.Write(BitConverter.GetBytes(entry.TGI.Instance.Value));
				fs.Write(BitConverter.GetBytes(entry.Offset));
				fs.Write(BitConverter.GetBytes(entry.CompressedSize));

			}
		}


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