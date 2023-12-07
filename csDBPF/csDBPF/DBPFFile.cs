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
		/// Stores key information about the DBPFFile. Is the first 96 bytes of the file. 
		/// </summary>
		public DBPFHeader Header;

		/// <summary>
		/// Represents a file system object for this DBPF file. 
		/// </summary>
		public FileInfo File;

		private long _dataSize;
		/// <summary>
		/// Size of all entries in this file,  in bytes.
		/// </summary>
		/// <remarks>
		/// This does not include the size allocated for the Header (96 bytes) or the Index (entry count * 20 bytes).
		/// </remarks>
		public long DataSize {
			get { return _dataSize; }
		}

		/// <summary>
		/// List of all entries in this file.
		/// </summary>
		/// <remarks>
		/// This is private because when an entry is added or removed, other operations happen simultaneously to adjust <see cref="_listOfTGIs"/>, <see cref="_dataSize"/>, etc.
		/// </remarks>
		private readonly List<DBPFEntry> _listOfEntries; //TODO - add in documentation about a pro tip to use LINQ to filter these based on the output of GetEntries or GetTGIs

		/// <summary>
		/// List of all TGIs in this file.
		/// </summary>
		/// <remarks>
		/// Can be used for quick inspection because no entry data is processed.
		/// </remarks>
		//private readonly List<DBPFTGI> _listOfTGIs;
		private readonly List<TGI> _listOfTGIs;

        /// <summary>
        /// Comma delineated list of issues encountered when loading this file.
        /// </summary>
        /// <remarks>
        /// The format is consistent with <see cref="DBPFEntry.IssueLog"/>. It is a multi line string of <see cref="TGI.ToString"/> followed by the message. Format is: FileName, Type, Group, Instance, TGIType, TGISubtype, Message.
        /// </remarks>
        private readonly StringBuilder _issueLog;

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
			/// Byte location of the first index in the file.
			/// </summary>
			public uint IndexEntryOffset { get; private set; }
			/// <summary>
			/// Size of the index table in bytes.
			/// </summary>
			public uint IndexSize { get; private set; }


			/// <summary>
			/// Instantiate a new DBPFHeader. All properties remain unset until  <see cref="InitializeBlank"/> or <see cref="Initialize(BinaryReader)"/> is called.
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
			//_listOfTGIs = new List<DBPFTGI>();
			_listOfTGIs = new List<TGI>();
			_issueLog = new StringBuilder();

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
		/// Reads the header and TGI list of a DBPF file.
		/// </summary>
		/// <param name="file"></param>
		private void ReadQuick(FileInfo file) {

		}



        /// <summary>
        /// Reads a DBPF file.
        /// </summary>
        /// <remarks>
        /// Use only for short-lived DBPF files for which the content does not change on disk, or does not matter if it does, or if the file is small.
        /// </remarks>
        /// <param name="file">File of the DBPF object to be used</param>
        /// <returns>A new DBPFFile object</returns>
        /// <see href="https://www.wiki.sc4devotion.com/index.php?title=DBPF#Pseudocode"/>
        private void Read(FileInfo file) {
			FileStream fs = new FileStream(file.FullName, FileMode.Open);
			BinaryReader br = new BinaryReader(fs); 

			try {
				// Read Header info
				Header.Initialize(br);

				//Read Index info
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

					_listOfTGIs.Add(new TGI(typeID, groupID, instanceID));
					offsets.Add(offset);
					sizes.Add(size);
				}


				//Read Entry data
				for (int idx = 0; idx < _listOfTGIs.Count; idx++) {
					br.BaseStream.Seek(offsets[idx], SeekOrigin.Begin);
					byteData = br.ReadBytes((int) sizes[idx]);

					switch (_listOfTGIs[idx].GetEntryType()) {
						case "EXMP":
							_listOfEntries.Add(new DBPFEntryEXMP(_listOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));
							break;
						case "LTEXT":
							_listOfEntries.Add(new DBPFEntryLTEXT(_listOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));
							break;
						case "DIR":
							_listOfEntries.Add(new DBPFEntryDIR(offsets[idx], sizes[idx], (uint) idx, byteData));
							break;
                        case "S3D":
                            _listOfEntries.Add(new DBPFEntryS3D(_listOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));
                            break;
                        case "FSH":
                            _listOfEntries.Add(new DBPFEntryFSH(_listOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));
                            break;
                        case "PNG":
                            _listOfEntries.Add(new DBPFEntryPNG(_listOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));
                            break;
                        case "LUA":
                            _listOfEntries.Add(new DBPFEntryLUA(_listOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));
                            break;
                        case "UI":
                            _listOfEntries.Add(new DBPFEntryUI(_listOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));
                            break;
                        case "WAV":
                            _listOfEntries.Add(new DBPFEntryWAV(_listOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));
                            break;
                        default:
                            LogMessage("Unknown TGI identifier.", _listOfTGIs[idx]);
                            _listOfEntries.Add(new DBPFEntryUnknown(_listOfTGIs[idx], offsets[idx], sizes[idx], (uint) idx, byteData));
                            break;
					}
				}
			}

			catch {
				LogMessage("Unable to read DBPF file. Format unknown.");
			}

			finally {
				br.Close();
				fs.Close();
			}


			//Parse the properties of each entry
			foreach (DBPFEntry entry in _listOfEntries) {
				//GetSubfileFormat(DBPFCompression.Decompress(entry.data));
			}

			//Initially populate issue log.
            foreach (DBPFEntry entry in _listOfEntries) {
				if (entry.IssueLog.ToString().Length>1) {
                    _issueLog.AppendLine(entry.IssueLog.ToString());
                }
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



        
        private void LogMessage(string message, TGI tgi = new TGI()) {
			 _issueLog.AppendLine(File.Name + "," + tgi.ToString().Replace(" ", "") + "," + message);
        }


        /// <summary>
        /// Return the messages/issues raised when reading this file and its entries. Format is: FileName, Type, Group, Instance, TGIType, TGISubtype, Message. Designed to be written to a CSV file.
        /// </summary>
        /// <returns>A comma separated string of issues encountered</returns>
        public string GetIssueLog() {
			StringBuilder issues = new StringBuilder(_issueLog.ToString());

            foreach (DBPFEntry entry in _listOfEntries) {
                if (entry.IssueLog.ToString().Length > 0) {
                    issues.Append(File.Name + entry.IssueLog.ToString());
                }
            }
            return issues.ToString();
		}



        /// <summary>
        /// Decodes all entries in the file.
        /// </summary>
		/// <remarks>
		/// For more information, see <see cref="DBPFEntry.DecodeEntry()"/> and the specific implementations for each entry type.
		/// </remarks>
        public void DecodeAllEntries() {
			foreach (DBPFEntry entry in _listOfEntries) {
				entry.DecodeEntry();
            }
		}



		/// <summary>
		/// Return the nth entry in the file by index. 0-based index.
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
		public DBPFEntry GetEntry(TGI TGI) {
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

			//Update and write Header
			if (Header.Identifier is null) {
				Header.InitializeBlank();
			}
			Header.Update(this);
			using FileStream fs = new(file.FullName, FileMode.Create);
			fs.Write(ByteArrayHelper.ToBytes(Header.Identifier,true));
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

			//Write all entries
			RebuildDirectory();
			foreach (DBPFEntry entry in _listOfEntries) {
				fs.Write(entry.ByteData);
			}

			//Write Index
			UpdateIndex();
			long pos = fs.Position;
			foreach (DBPFEntry entry in _listOfEntries) {
				fs.Write(BitConverter.GetBytes(entry.TGI.TypeID.Value));
				fs.Write(BitConverter.GetBytes(entry.TGI.GroupID.Value));
				fs.Write(BitConverter.GetBytes(entry.TGI.InstanceID.Value));
				fs.Write(BitConverter.GetBytes(entry.Offset));
				fs.Write(BitConverter.GetBytes(entry.CompressedSize));
			}
		}


		/// <summary>
		/// Updates the position number and offset of each entry based on the current size and position of each entry.
		/// </summary>
		private void UpdateIndex() {
			uint offset = 0;
			for (int idx = 0; idx < _listOfEntries.Count; idx++) {
				if (idx == 0) {
					_listOfEntries[0].Offset = 96;
					offset = 96;
				} else {
					offset += _listOfEntries[idx - 1].CompressedSize;
					_listOfEntries[idx].Offset = offset;
				}

				_listOfEntries[idx].IndexPos = (uint) idx;
			}
		}



		/// <summary>
		/// Add an entry to this file.
		/// </summary>
		/// <param name="entry">Entry to add</param>
		public void AddEntry(DBPFEntry entry) {
			entry.IndexPos = (uint) (_listOfEntries.Count + 1);
			entry.Offset = (uint) _dataSize + 96;

			_listOfEntries.Add(entry);
			_listOfTGIs.Add(entry.TGI);
			try {
                _dataSize += entry.ByteData.LongLength;
            }
			catch (NullReferenceException) { //Non decoded entries will not have byte data set, so test if they are compressed or uncompressed and use that size.
				if (entry.IsCompressed) { //TODO - think this is wrong b/c if entry is not decoded then this not set?????
					_dataSize += entry.CompressedSize;
				} else {
					_dataSize += entry.UncompressedSize;
				}
			}
			
		}

		/// <summary>
		/// Add multiple entries to this file.
		/// </summary>
		/// <param name="entries">Entries to add</param>
		public void AddEntries(IEnumerable<DBPFEntry> entries) {
			foreach (DBPFEntry entry in entries) {
				AddEntry(entry);
			}
		}

		/// <summary>
		/// Add the entry to this file if a matching TGI is not found, otherwise update the corresponding entry.
		/// </summary>
		/// <param name="entry">Entry to add</param>
		public void AddOrUpdateEntry(DBPFEntry entry) {
			if (_listOfEntries.Any(e => e.TGI.Equals(entry.TGI))) {
				UpdateEntry(entry);
			} else {
				AddEntry(entry);
			}
		}

		/// <summary>
		/// Add each entry to this file if a matching TGI is not found, otherwise update the corresponding entry.
		/// </summary>
		/// <param name="entries">Entries to add</param>
		public void AddOrUpdateEntries(IEnumerable<DBPFEntry> entries) {
			foreach (DBPFEntry entry in entries) {
				AddOrUpdateEntry(entry);
			}
		}

		/// <summary>
		/// Remove the entry matching the specified TGI from this file.
		/// </summary>
		/// <param name="tgi">Entry TGI to remove</param>
		/// <remarks>
		/// If more than one entry matches the given TGI then no entries are removed.
		/// </remarks>
		public void RemoveEntry(TGI tgi) {
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
			_dataSize -= _listOfEntries[position].ByteData.LongLength;
		}


		public void RemoveEntries(TGI tgi) {
            //TODO - implement RemoveEntries for all matching TGIs
            throw new NotImplementedException();
		}

		/// <summary>
		/// Clears all entries from this file.
		/// </summary>
		public void RemoveAllEntries() {
			_listOfEntries.Clear();
			_listOfTGIs.Clear();
			_dataSize = 0;
		}

		/// <summary>
		/// Builds the Directory subfile if any entries in this file are compressed.
		/// </summary>
		public void RebuildDirectory() {
			if (_listOfEntries.Any(entry => entry.IsCompressed == true)) {
				DBPFEntryDIR dir =  new DBPFEntryDIR();
				dir.Build(_listOfEntries);
				AddOrUpdateEntry(dir);
			}
		}

		/// <summary>
		/// Returns all entries in this file.
		/// </summary>
		/// <returns>All entries in this file</returns>
		public List<DBPFEntry> GetEntries() {
			return _listOfEntries;
		}

		/// <summary>
		/// Returns the count of entries in this file.
		/// </summary>
		/// <returns>The count of entries</returns>
		public int CountEntries() {
			return _listOfEntries.Count;
		}

        /// <summary>
        /// Returns a list of all TGI sets in this file.
        /// </summary>
        /// <returns>A list of all TGI sets</returns>
        /// <remarks>
        /// Can be used for quick inspection of this file instead of <see cref="GetEntries()"/> because no entry data is processed.
        /// </remarks>
        public List<TGI> GetTGIs() {
			return _listOfTGIs;
		}

		/// <summary>
		/// Returns the count of TGIs in this file.
		/// </summary>
		/// <returns>The count of TGI sets</returns>
		/// <remarks>
		/// This may be used in lieu of <see cref="CountEntries()"/> for more performant operation if no entry data is required.
		/// </remarks>
		public int CountTGIs() {
			return _listOfTGIs.Count;
		}

		public void UpdateAllEntries(IEnumerable<DBPFEntry> entries) {
			//TODO - implement Update for a list of entries
			//any entries with matching TGIs will be overwritten (first occurrence only) and any new ones will be added - skip DIR files
			throw new NotImplementedException();
		}

		public void UpdateEntry(DBPFEntry entry) {
			//TODO - implement Update for a TGI
			throw new NotImplementedException();
		}
		public void UpdateEntry(int index) {
			//TODO - implement Update for a position
			throw new NotImplementedException();
		}
	}
}