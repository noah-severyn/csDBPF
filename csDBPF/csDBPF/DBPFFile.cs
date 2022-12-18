using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Linq;
using System.IO;
using System.Diagnostics;
using csDBPF.Properties;
using System.Net;
using csDBPF.Entries;

namespace csDBPF
{
    /// <summary>
    /// Contains the header data and all entries for a DBPF file.
    /// </summary>
    /// <remarks>
    /// At a high level, a <see cref="DBPFFile"/> ("file") is the container for the DBPF data. This takes the form of a .dat/.sc4lot/.sc4model/.sc4desc file. Each file is broken into one or more <see cref="DBPFEntry"/> ("entries" or "subfiles"). 
    /// For Exemplar and Cohort type entries, each entry is composed of one or more <see cref="DBPFProperty"/> ("properties"). Each property corresponds to one of <see cref="XMLExemplarProperty"/> which are generated from the properties XML file. This file stores useful and human friendly information about the property including name, min/max value, default values, etc.
    /// For other type entries, their data is stored in a byte array, and is interpreted differently depending on the type of the entry.
    /// </remarks>
    public class DBPFFile {
		/// <summary>
		/// Stores key information about the DBPFFile. The Header is the first 96 bytes of the DBPFFile. 
		/// </summary>
		public DBPFHeader Header;
		/// <summary>
		/// Represents the file system file for this instance. 
		/// </summary>
		public FileInfo File;

		private List<DBPFEntry> _listOfEntries;
		/// <summary>
		/// List of all entries in this file.
		/// </summary>
		public List<DBPFEntry> ListOfEntries {
			get { return _listOfEntries; }
		}

		private List<DBPFTGI> _listOfTGIs;
		/// <summary>
		/// List of all TGIs in this file.
		/// </summary>
		/// <remarks>
		/// Can be used for quick inspection because no entry data is processed.
		/// </remarks>
		public List<DBPFTGI> ListOfTGIs {
			get { return _listOfTGIs; }
		}

		private long _fileSize;
		/// <summary>
		/// File size in bytes.
		/// </summary>
		public long FileSize {
			get { return _fileSize; }
			set { _fileSize = value; }
		}


		//------------- BEGIN DBPFFile.Header ------------- \\
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
			public uint DateCreated { get; private set; }
			/// <summary>
			/// Modification time in Unix timestamp format.
			/// </summary>
			public uint DateModified { get; private set; }
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
			public uint IndexEntryCount { get; private set; }
			/// <summary>
			/// Byte location of the first index in the file.
			/// </summary>
			public uint IndexEntryOffset { get; private set; }
			/// <summary>
			/// Size of the index table in bytes.
			/// </summary>
			public uint IndexSize { get; private set; }


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
			/// Initialize Header information with default values.
			/// </summary>
			public void InitializeBlank() {
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
		/// Instantiates a DBPFFile from a file path. If the file exists, its contents are read into the new DBPFFile; if the file does not exist then a new DBPFFile is created with default Header values.
		/// </summary>
		/// <param name="fileName">File to read</param>
		public DBPFFile(string fileName) : this(new FileInfo(fileName)) {}

		/// <summary>
		/// Instantiates a DBPFFile from a FileInfo object. If the file exists, its contents are read into the new DBPFFile; if the file does not exist then a new DBPFFile is created with default Header values.
		/// </summary>
		/// <param name="file">File to read</param>
		public DBPFFile(FileInfo file) {
			File = file;
			Header = new DBPFHeader();
			_listOfEntries = new List<DBPFEntry>();
			_listOfTGIs = new List<DBPFTGI>();
			_fileSize = 96;

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
			FileStream fs = new FileStream(file.FullName, FileMode.Open);
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
					_listOfTGIs.Add(tgi);
					offsets.Add(offset);
					sizes.Add(size);
				}

				for (int idx = 0; idx < _listOfTGIs.Count; idx++) {
					br.BaseStream.Seek(offsets[idx], SeekOrigin.Begin);
					byteData = br.ReadBytes((int) sizes[idx]);

					switch (_listOfTGIs[idx].Category) {
						case "EXMP":
							_listOfEntries.Add(new DBPFEntryEXMP(_listOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));
							break;
						case "LTEXT":
							_listOfEntries.Add(new DBPFEntryLTEXT(_listOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));
							break;
						case "DIR":
							_listOfEntries.Add(new DBPFEntryDIR(_listOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));
							break;
						default:
							break;
					}
				}
			}

			finally {
				br.Close();
				fs.Close();
			}


			//Parse the properties of each entry
			foreach (DBPFEntry entry in _listOfEntries) {
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

		//TODO - implement readCached https://github.com/memo33/jDBPFX/blob/master/src/jdbpfx/DBPFFile.java#L721




		/// <summary>
		/// Decodes all entries in the file. See <see cref="DBPFEntry.DecodeEntry()"/> for more information
		/// </summary>
		public void DecodeAllEntries() {
			foreach (DBPFEntry entry in _listOfEntries) {
				entry.DecodeEntry();
			}
		}



		/// <summary>
		/// Return the nth entry in the file by index.
		/// </summary>
		/// <param name="index">Index position in file.</param>
		/// <returns>The nth DBPFEntry</returns>
		public DBPFEntry GetEntry(int index) {
			return _listOfEntries[index];
		}
		/// <summary>
		/// Return the first entry matching the specified Instance ID.
		/// </summary>
		/// <param name="instance">IID to search for</param>
		/// <returns>A matching DBPFEntry</returns>
		public DBPFEntry GetEntry(uint instance) {
			return _listOfEntries.Find(i => i.TGI.InstanceID == instance);
		}
		/// <summary>
		/// Return the first entry matching the specified TGI.
		/// </summary>
		/// <param name="TGI">TGI set to search for</param>
		/// <returns>A matching DBPFEntry</returns>
		public DBPFEntry GetEntry(DBPFTGI TGI) {
			return _listOfEntries.Find(entry => entry.TGI.Equals(TGI));
		}



		/// <summary>
		/// Saves the current instance to disk using the <see cref="File"/> property.
		/// </summary>
		public void Save() {
			Save(File.FullName);
		}



		/// <summary>
		/// Saves the current instance to disk at the specified path.
		/// <param name="filePath">File to save as</param>
		/// </summary>
		public void Save(string filePath) {
			FileInfo file;
			if (filePath is null) {
				file = File;
			} else {
				file = new FileInfo(filePath);
			}

			using FileStream fs = new(file.FullName, FileMode.Create);
			//Write Header
			fs.Write(ByteArrayHelper.ToBytes(Header.Identifier));
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
			foreach (DBPFEntry entry in _listOfEntries) {
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
			foreach (DBPFEntry entry in _listOfEntries) {
				fs.Write(BitConverter.GetBytes(entry.TGI.TypeID.Value));
				fs.Write(BitConverter.GetBytes(entry.TGI.GroupID.Value));
				fs.Write(BitConverter.GetBytes(entry.TGI.InstanceID.Value));
				fs.Write(BitConverter.GetBytes(entry.Offset));
				fs.Write(BitConverter.GetBytes(entry.CompressedSize));

			}
		}



		/// <summary>
		/// Add an entry to this file.
		/// </summary>
		/// <param name="entry"></param>
		public void AddEntry(DBPFEntry entry) {
			_listOfEntries.Add(entry);
			_listOfTGIs.Add(entry.TGI);
			_fileSize += entry.ByteData.LongLength;
		}



		/// <summary>
		/// Remove the entry matching the specified TGI from this file.
		/// </summary>
		/// <param name="tgi">Entry TGI to remove</param>
		/// <remarks>
		/// If more than one entry matches the given TGI then no entries are removed.
		/// </remarks>
		public void RemoveEntry(DBPFTGI tgi) {
			int matches = _listOfTGIs.Count(x => x.Equals(tgi));
			if (matches != 1) {
				return;
			}

			int index = _listOfTGIs.IndexOf(tgi);
			RemoveEntry(index);
		}



		/// <summary>
		/// Remove the entry at the specified position from this file.
		/// </summary>
		/// <param name="position">Entry position to remove</param>
		public void RemoveEntry(int position) {
			_listOfEntries.RemoveAt(position);
			_listOfTGIs.RemoveAt(position);
			_fileSize -= _listOfEntries[position].ByteData.LongLength;
		}



		/// <summary>
		/// Clears all entries from this file.
		/// </summary>
		public void RemoveAllEntries() {
			_listOfEntries.Clear();
			_listOfTGIs.Clear();
			_fileSize = 96; //Header size
		}


		
		/// <summary>
		/// Updates the directory subfile with all compressed items in this file.
		/// </summary>
		public void UpdateDirectory() {
			DBPFEntryDIR dir = (DBPFEntryDIR) GetEntry(DBPFTGI.DIRECTORY);
			if (dir is null) {
				dir = new DBPFEntryDIR();
			}
			dir.Update(ListOfEntries);
		}
	}
}