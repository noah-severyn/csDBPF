using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Linq;
using System.IO;

namespace csDBPF {
    /// <summary>
    /// Contains the header data and all entries for a DBPF file.
    /// </summary>
    /// <remarks>
    /// At a high level, a <see href="https://wiki.sc4devotion.com/index.php?title=DBPF">DBPFFile</see> ("file") is the container for the DBPF data, and takes the form of a .dat/.sc4lot/.sc4model/.sc4desc file. The main components of the file include the Header, ListOfTGIs, and ListOfEntries. The Header stores important information about the file itself, and the ListOfTGIs and ListOfEntries store information about subfiles. Each DBPFFile is broken into one or more <see cref="DBPFEntry"/> ("entries" or "subfiles"), which inform what kind of data the entry stores and how it should be interpreted.
    /// Exemplar and Cohort type entries are composed of one or more <see cref="DBPFProperty"/> ("properties"). Each property corresponds to one of <see cref="XMLExemplarProperty"/> which are generated from the properties XML file.
	/// For other type entries, the data is interpreted directly from its byte array. This process varies depending on the type of entry (text, bitmap, xml, etc.).
	/// The data for a particular entry or property will remain in its raw byte form until a DecodeEntry() or DecodeProperty() function is called to translate the byte data into a "friendly" format.
    /// </remarks>
    public partial class DBPFFile {
		/// <summary>
		/// Stores key information about the DBPFFile. Is the first 96 bytes of the file. 
		/// </summary>
		public DBPFHeader Header;

		/// <summary>
		/// Represents a file system object for this DBPF file. 
		/// </summary>
		public FileInfo File;

		/// <summary>
		/// Size of all entries in this file,  in bytes.
		/// </summary>
		/// <remarks>
		/// This does not include the size allocated for the Header (96 bytes) or the Index (entry count * 20 bytes).
		/// </remarks>
		public long DataSize { get; private set; }

		/// <summary>
		/// List of all entries in this file.
		/// </summary>
		/// <remarks>
		/// This is private because when an entry is added or removed, other operations happen simultaneously to adjust <see cref="_listOfTGIs"/>, <see cref="DataSize"/>, etc.
		/// </remarks>
		private readonly List<DBPFEntry> _listOfEntries; //TODO - add in documentation about a pro tip to use LINQ to filter these based on the output of GetEntries or GetTGIs

		/// <summary>
		/// List of all TGIs in this file.
		/// </summary>
		/// <remarks>
		/// Can be used for quick inspection because no entry data is processed.
		/// </remarks>
		private readonly List<TGI> _listOfTGIs;

        /// <summary>
        /// Comma delineated list of issues encountered when loading this file.
        /// </summary>
        /// <remarks>
        /// The format is consistent with <see cref="DBPFEntry.IssueLog"/>. It is a multi line string of <see cref="TGI.ToString"/> followed by the message. Format is: FileName, Type, Group, Instance, TGIType, TGISubtype, Message.
        /// </remarks>
        private readonly StringBuilder _issueLog;



		/// <summary>
		/// Instantiates a DBPFFile from a file path. If the file exists, its contents are read into the new DBPFFile; if the file does not exist then a new DBPFFile is created with default Header values.
		/// </summary>
		/// <param name="fileName">File to read</param>
		public DBPFFile(string fileName) : this(new FileInfo(fileName)) {}

		/// <summary>
		/// Instantiates a DBPFFile from a FileInfo object. If the file exists, its contents are read into the new DBPFFile; if the file does not exist then a new DBPFFile is created with default Header values.
		/// </summary>
		/// <param name="file">File to read</param>
		public DBPFFile(FileInfo file) : this() {
			File = file;
			if (!file.Exists) {
                return;
			}

			bool map = false;
			try {
				if (map) {
					ReadAndMap();
				} else {
					Read();
				}
			} 
			catch (InvalidDataException) { 
				
			}
		}

		/// <summary>
		/// Instantiates a new DBPFFile from scratch. 
		/// </summary>
		public DBPFFile() {
			Header = new DBPFHeader();
            _listOfEntries = new List<DBPFEntry>();
            _listOfTGIs = new List<TGI>();
            _issueLog = new StringBuilder();
        }

        /// <inheritdoc/>
        public override string ToString() {
			return $"{File.Name}: {_listOfEntries.Count} subfiles";
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
        /// <returns>A new DBPFFile object</returns>
        /// <see href="https://www.wiki.sc4devotion.com/index.php?title=DBPF#Pseudocode"/>
        private void Read() {
			FileStream fs = new FileStream(File.FullName, FileMode.Open);
			BinaryReader br = new BinaryReader(fs); 

			try {
				// Read Header info
				Header = new DBPFHeader(br);
				DataSize = Header.IndexEntryOffset - 96;

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
		/// Serves a similar purpose to <see cref="Read()"/>, but it is capable of writing large amount of entries quicker - suitable for large files and files of which the ______ methods will be called frequently.
		/// </remarks>
		private DBPFFile ReadAndMap() {
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
        /// Decompresses (if necessary) and decodes all entries in the file.
        /// </summary>
		/// <remarks>
		/// For more information, see <see cref="DBPFEntry.Decode()"/> and the specific implementations for each entry type.
		/// </remarks>
        public void DecodeAllEntries() {
			foreach (DBPFEntry entry in _listOfEntries) {
				entry.Decode();
            }
		}


		public void EncodeAllEntries() {
            foreach (DBPFEntry entry in _listOfEntries) {
                entry.Encode();
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
			return _listOfEntries.Find(entry => entry.TGI.Matches(TGI));
        }
        /// <summary>
        /// Returns all entries in this file.
        /// </summary>
        /// <returns>A list of all entries in this file</returns>
        public List<DBPFEntry> GetEntries() {
            return _listOfEntries;
        }
        /// <summary>
        /// Returns all entries in this file of the specified type.
        /// </summary>
        /// <returns>A list of entries in this file of the specified type</returns>
        public List<DBPFEntry> GetEntries(TGI entryType) {
            return _listOfEntries.FindAll(e => e.TGI.Matches(entryType));
        }



        /// <summary>
        /// Saves the current instance to disk using the <see cref="File"/> property.
        /// </summary>
        public void Save() {
            SaveAs(File.FullName);
		}
		/// <summary>
		/// Saves the current instance to disk at the specified path.
		/// <param name="filePath">File to save as</param>
		/// </summary>
		public void SaveAs(string filePath) {
			FileInfo file;
			if (filePath is null) {
				file = File;
			} else {
				file = new FileInfo(filePath);
			}

			//Update and write Header
			if (Header.Identifier is null) {
				Header = new DBPFHeader();
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
				fs.Write(BitConverter.GetBytes(entry.TGI.TypeID));
				fs.Write(BitConverter.GetBytes(entry.TGI.GroupID));
				fs.Write(BitConverter.GetBytes(entry.TGI.InstanceID));
				fs.Write(BitConverter.GetBytes(entry.Offset));
				if (entry.IsCompressed) {
                    fs.Write(BitConverter.GetBytes(entry.CompressedSize));
                } else {
                    fs.Write(BitConverter.GetBytes(entry.UncompressedSize));
                }
				
			}
		}


		/// <summary>
		/// Updates the position number and offset of each entry based on the current size and position of each entry.
		/// </summary>
		private void UpdateIndex() {
			uint offset = 96;
            _listOfEntries[0].Offset = 96;
			if (_listOfEntries.Count == 1) {
				return;
			}

            for (int idx = 1; idx < _listOfEntries.Count; idx++) {
				offset += _listOfEntries[idx - 1].GetSize();
				_listOfEntries[idx].Offset = offset;
				_listOfEntries[idx].IndexPos = (uint) idx;
			}
		}



		/// <summary>
		/// Add an entry to this file.
		/// </summary>
		/// <param name="entry">Entry to add</param>
		public void AddEntry(DBPFEntry entry) {
			entry.IndexPos = (uint) (_listOfEntries.Count + 1);
			entry.Offset = (uint) DataSize + 96;

			_listOfEntries.Add(entry);
			_listOfTGIs.Add(entry.TGI);

            //Non decoded entries (or newly created entries that have not been encoded yet) will not have ByteData set
            if (entry.ByteData is null) {
				DataSize += entry.GetSize();
            } else {
                DataSize += entry.ByteData.LongLength;
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
			if (_listOfEntries.Any(e => e.TGI.Matches(entry.TGI))) {
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
			DataSize -= _listOfEntries[position].ByteData.LongLength;
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
			DataSize = 0;
		}

		/// <summary>
		/// Builds the Directory subfile if any entries in this file are compressed.
		/// </summary>
		public void RebuildDirectory() {
			if (_listOfEntries.Any(entry => entry.IsCompressed == true)) {
				DBPFEntryDIR dir =  new DBPFEntryDIR();
				dir.Build(_listOfEntries);
			}
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
			//throw new NotImplementedException();
		}
		public void UpdateEntry(int index) {
			//TODO - implement Update for a position
			throw new NotImplementedException();
		}

		public void SortEntries() {
            //_listOfEntries = _listOfEntries.OrderBy(e => e.TGI);
			//_listOfTGIs.OrderBy(_);
		}
	}
}